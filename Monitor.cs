using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Net;
using System.Collections.Specialized;
using System.Net.Http;

namespace GPU_trace
{
    class Param
    {
        private long chatid;
        private DateTime lastMessageDate;
        private bool traceGPU, traceMEM;

        /// <summary>
        /// Добавление параметров чата в список
        /// </summary>
        /// <param name="CHATID">ID чата</param>
        /// <param name="LASTMESSAGEDATE">дата последнего сообщения</param>
        /// <param name="TRACEGPU">Отслеживание температуры GPU</param>
        /// <param name="TRACEMEM">Отслеживание температуры MEM</param>
        public Param(long CHATID, DateTime LASTMESSAGEDATE, bool TRACEGPU, bool TRACEMEM)
        {
            chatid = CHATID; //ID чата
            lastMessageDate = LASTMESSAGEDATE; //дата последнего сообщения
            traceGPU = TRACEGPU; //Отслеживание температуры GPU
            traceMEM = TRACEMEM; //Отслеживание температуры Памяти
        }

        public long CHATID { get { return chatid; } set { chatid = value; } }
        public DateTime LASTMESSAGEDATE { get { return lastMessageDate; } set { lastMessageDate = value; } }
        public bool TRACEGPU { get { return traceGPU; } set { traceGPU = value; } }
        public bool TRACEMEM { get { return traceMEM; } set { traceMEM = value; } }
    }

    /// <summary>
    /// Универсальный склонятор на любые слова, либо указывать корень слова + окончания, либо сами варианты слов (как в вашем случае год/лет - окончаний нет)
    /// </summary>
    public static class StringTransform
    {
        static readonly int[] WordCaseMap = { 2, 0, 1, 1, 1, 2, 2, 2, 2, 2 };

        /// <summary>
        /// var t = StringTransform.TransformCase("вам " + i + " ", i, "год", "года", "лет");
        /// </summary>
        /// <param name="wordRoot"></param>
        /// <param name="number"></param>
        /// <param name="endings"></param>
        /// <returns></returns>
        public static string TransformCase(this string wordRoot, long number, string[] endings)
        {
            return wordRoot + endings[WordCaseMap[(number % 100 / 10 == 1 ? 0 : 1) * (number % 10)]];
        }

        /// <summary>
        /// var t = StringTransform.TransformCase(i + " байт", i, "", "а", "ов");
        /// </summary>
        /// <param name="wordRoot"></param>
        /// <param name="number"></param>
        /// <param name="nominative"></param>
        /// <param name="genitiveSingular"></param>
        /// <param name="genitivePlural"></param>
        /// <returns></returns>
        public static string TransformCase(this string wordRoot, long number, string nominative, string genitiveSingular, string genitivePlural)
        {
            string[] endings = { nominative, genitiveSingular, genitivePlural };
            return TransformCase(wordRoot, number, endings);
        }
    }

    public class Error
    {
        private int code;
        private string description;

        /// <summary>
        /// Добавление ошибок
        /// </summary>
        /// <param name="CODE">код ошибки</param>
        /// <param name="DESCR">описание ошибки;</param>
        public Error(int CODE, string DESCR)
        {
            code = CODE; //код ошибки
            description = DESCR; //описание ошибки
        }

        public int CODE { get { return code; } set { code = value; } }
        public string DESCR { get { return description; } set { description = value; } }
    }

    class Monitor
    {
        //public static TelegramBotClient client;
        //public static ParseMode parseMode;
        //public static List<Param> Params = new List<Param>();
        public static string token = "Не используется", version = "2.3", updVersion = version;
        public static bool useTelegramBot = false, sendingLimits = false, autorun = false;
        public static string logfilename = "data\\log.txt", paramFilename = "data\\Parameters.ini", paramFileBackup = "data\\backup\\Parameters_backup.ini", settingsfilename = "data\\Settings.ini", filename = $"{Environment.GetFolderPath(Environment.SpecialFolder.Personal)}\\GPU-Z Sensor Log.txt";
        public static double GPUlimit, MEMlimit, GPUtemp, MEMtemp, inputGPUlimit, inputMEMlimit;
        public static DateTime dataDate;
        public static Stack<Error> errors = new Stack<Error>();
        private static string line;
        private static Queue<string> meQueue = new Queue<string>();
        private const int LINES_KEPT = 1;


        /// <summary>
        /// Инициализация бота не используем в новой версии
        /// </summary>
        //public static void InitClient()
        //{
        //    client = new TelegramBotClient(token);
        //    var cts = new CancellationTokenSource();
        //    var cancellationToken = cts.Token;
        //    var receiverOptions = new ReceiverOptions
        //    {
        //        AllowedUpdates = { }, // receive all update types
        //    };
        //    client.StartReceiving(
        //        HandleUpdateAsync,
        //        HandleErrorAsync,
        //        receiverOptions,
        //        cancellationToken
        //    );

        //}

        /// <summary>
        /// Метод отправляет сообщение
        /// </summary>
        /// <param name="id">id чата</param>
        /// <param name="myText">текст</param>
        //public async static void Send(long id, string myText)
        //{
        //    try
        //    {
        //        await client.SendTextMessageAsync(id, myText, parseMode = ParseMode.MarkdownV2);
        //    }
        //    catch (Exception ex)
        //    {
        //        AddToLog($"Ошибка при отправке сообщения в телеграмм канал.\n {ex}");
        //    }

        //}
        /// <summary>
        /// Отпарвить меню в чат бота
        /// </summary>
        /// <param name="id"></param>
        //public async static void SendMenu(long id)
        //{
        //    var replyKeyboardMarkup = new ReplyKeyboardMarkup(
        //                                        new KeyboardButton[][]
        //                                            {
        //                                                      new KeyboardButton[] { $"Отслеживай GPU", $"Отслеживай память" },
        //                                                      new KeyboardButton[] { "Лимит GPU 65", $"Лимит памяти 85" },
        //                                                      new KeyboardButton[] { "Показатели" },

        //                                            })
        //    {
        //        ResizeKeyboard = true
        //    };

        //    try
        //    {
        //        await client.SendTextMessageAsync(id, "Что я могу?", replyMarkup: replyKeyboardMarkup);
        //    }
        //    catch (Exception)
        //    {
        //        AddToLog("Ошибка при попытке отправить меню в чат.");
        //    }
        //}

        /// <summary>
        /// Отслеживание сообщений в боте телеграм
        /// </summary>
        //public static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        //{
        //    Param foundParam;

        //    if (update.Type == UpdateType.Message)
        //    {
        //        var msg = update.Message;
        //        AddToLog(msg.Chat.Id.ToString() + ".txt", $"\n{DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss")}: {msg.From}: {msg.Text}.");

        //        if (Params.Exists(item => item.CHATID == msg.Chat.Id)) //Проверяем список параметров
        //        {
        //            foundParam = Params.Find(item => item.CHATID == msg.Chat.Id);
        //            foundParam.LASTMESSAGEDATE = DateTime.Now;
        //        }
        //        else
        //        {
        //            Params.Add(new Param(msg.Chat.Id, DateTime.Now, false, false));
        //        }

        //        if (msg.Text == "Отслеживай GPU")
        //        {
        //            foundParam = Params.Find(item => item.CHATID == msg.Chat.Id);
        //            if (foundParam.TRACEGPU)
        //            {
        //                Send(msg.Chat.Id, $"Уведомление о превышении температуры GPU уже включено, выключаю...");
        //                foundParam.TRACEGPU = false;
        //            }
        //            else
        //            {
        //                foundParam.TRACEGPU = true;
        //                Send(msg.Chat.Id, $"Включено уведомление о превышении температуры GPU");
        //            }
        //        }

        //        if (msg.Text == "Отслеживай память")
        //        {
        //            foundParam = Params.Find(item => item.CHATID == msg.Chat.Id);
        //            if (foundParam.TRACEMEM)
        //            {
        //                Send(msg.Chat.Id, $"Уведомление о превышении температуры памяти уже активно, выключаю...");
        //                foundParam.TRACEMEM = false;
        //            }
        //            else
        //            {
        //                foundParam.TRACEMEM = true;
        //                Send(msg.Chat.Id, $"Включено уведомление о превышении температуры памяти");
        //            }
        //        }

        //        if (Regex.IsMatch(msg.Text, @"[Лл]имит GPU [0-9]{1,3}"))
        //        {
        //            string pattern = @"[Лл]имит GPU [0-9]{1,3}";
        //            Regex r = new Regex(pattern, RegexOptions.IgnoreCase);
        //            Match words = r.Match(msg.Text);

        //            string searchString = Convert.ToString(words.Groups[0].Value).Remove(0, 9);
        //            GPUlimit = Convert.ToDouble(searchString);
        //            SaveSettings();


        //            Send(msg.Chat.Id, $"Установлен новый лимит GPU:\n*{StringTransform.TransformCase(GPUlimit + " ", (long)GPUlimit, "градус", "градуса", "градусов")}*");
        //        }

        //        if (Regex.IsMatch(msg.Text, @"[Лл]имит памяти [0-9]{1,3}"))
        //        {
        //            string pattern = @"[Лл]имит памяти [0-9]{1,3}";
        //            Regex r = new Regex(pattern, RegexOptions.IgnoreCase);
        //            Match words = r.Match(msg.Text);

        //            string searchString = Convert.ToString(words.Groups[0].Value).Remove(0, 12);
        //            MEMlimit = Convert.ToDouble(searchString);
        //            SaveSettings();
        //            Send(msg.Chat.Id, $"Установлен новый лимит памяти:\n*{StringTransform.TransformCase(MEMlimit + " ", (long)MEMlimit, "градус", "градуса", "градусов")}*");
        //        }

        //        if (Regex.IsMatch(msg.Text, @"[Пп]омоги"))
        //        {
        //            SendMenu(msg.Chat.Id);
        //        }

        //        if (Regex.IsMatch(msg.Text, @"[Пп]оказатели"))
        //        {
        //            Send(msg.Chat.Id, $"Температура GPU:\n*{StringTransform.TransformCase(GPUtemp + " ", (long)GPUtemp, "градус", "градуса", "градусов")}*\nТемпература памяти:\n*{StringTransform.TransformCase(MEMtemp + " ", (long)MEMtemp, "градус", "градуса", "градусов")}*");
        //        }
        //    }

        //}

        //public static async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        //{
        //    // Некоторые действия
        //    //Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(exception));
        //    AddToLog(Newtonsoft.Json.JsonConvert.SerializeObject(exception));
        //}


        /// <summary>
        /// Запись в лог файл
        /// </summary>
        /// <param name="v"></param>
        public static void AddToLog(string text)
        {
            if (System.IO.File.Exists(logfilename))
            {
                FileInfo file = new FileInfo(logfilename);
                if (file.Length > 150000) //Если файл лога вырос до 150 кб
                {
                    System.IO.File.WriteAllText(logfilename, $"\n{DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss")}: " + text);
                }
                else
                {
                    System.IO.File.AppendAllText(logfilename, $"\n{DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss")}: " + text);
                }
            }
            else
            {
                Directory.CreateDirectory("data");
                System.IO.File.WriteAllText(logfilename, $"\n{DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss")}: " + text);
            }

        }
        /// <summary>
        /// Запись в лог файл.
        /// </summary>
        /// <param name="filename">Имя файла куда будет идти запись</param>
        /// <param name="text">Текст который нужно записать</param>
        public static void AddToLog(string chatId, string text)
        {
            try
            {
                if (System.IO.File.Exists("data\\" + chatId))
                {
                    FileInfo file = new FileInfo("data\\" + chatId);
                    if (file.Length > 150000) //Если файл лога вырос до 150 кб
                    {
                        System.IO.File.WriteAllText("data\\" + chatId, text);
                    }
                    else
                    {
                        System.IO.File.AppendAllText("data\\" + chatId, text);
                    }
                }
                else
                {
                    System.IO.File.WriteAllText("data\\" + chatId, text);
                }

            }

            catch (Exception)
            {
                AddToLog($"Ошибка при попытке прочитать файл {chatId}.");
            }


        }
        /// <summary>
        /// Метод экранирует символы, которые могут вызвать ошибку при отправке в телеграм, и возвращает строку
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string TextConverter(string text)
        {
            //In all other places characters '_', '*', '[', ']', '(', ')', '~', '`', '>', '#', '+', '-', '=', '|', '{', '}', '.', '!'
            StringBuilder st = new StringBuilder(text);
            st.Replace("_", "\\_");
            st.Replace("*", "\\*");
            st.Replace("[", "\\[");
            st.Replace("]", "\\]");
            st.Replace("(", "\\(");
            st.Replace(")", "\\)");
            st.Replace("~", "\\~");
            st.Replace("`", "\\`");
            st.Replace(">", "\\>");
            st.Replace("#", "\\#");
            st.Replace("+", "\\+");
            st.Replace("-", "\\-");
            st.Replace("=", "\\=");
            st.Replace("|", "\\|");
            st.Replace("{", "\\{");
            st.Replace("}", "\\}");
            st.Replace(".", "\\.");
            st.Replace("!", "\\!");
            return st.ToString();
        }

        /////////////Параметры не используются в новой версии
        /// <summary>
        /// Записывает коллекцию параметров в файл
        /// </summary>
        /// <param name="param_list">Коллекция параметров.</param>
        public static void SaveParamList(ref List<Param> param_list)
        {
            StringBuilder text = new StringBuilder();
            foreach (var item in param_list)
            {
                text.Append($"{item.CHATID};{item.LASTMESSAGEDATE.ToString("dd.MM.yyyy HH:mm:ss")};{item.TRACEGPU};{item.TRACEMEM}\n");
            }

            System.IO.File.WriteAllText(paramFilename, text.ToString());

        }
        /// <summary>
        /// Читает коллекцию параметров из файла.
        /// </summary>
        /// <param name="member_list">Коллекция параметров</param>
        public static void ReadParamList(ref List<Param> param_list)
        {
            string[] list, ac;
            param_list.Clear();
            DateTime lm = new DateTime();
            try
            {
                list = System.IO.File.ReadAllLines(paramFilename);
            }
            catch (FileNotFoundException)
            {
                AddToLog($"Файл {paramFilename} не существует.");
                return;
            }
            for (int i = 0; i < list.Length; i++)
            {
                try
                {
                    ac = list[i].Split(new char[] { ';' });
                    lm = DateTime.ParseExact(Regex.Match(ac[1], @"[0-9]{2}.[0-9]{2}.[0-9]{4} [0-9]{1,2}:[0-9]{2}:[0-9]{2}").Groups[0].Value, "dd.MM.yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                    param_list.Add(new Param(Convert.ToInt64(ac[0]), lm, Convert.ToBoolean(ac[2]), Convert.ToBoolean(ac[3])));
                }
                catch (Exception)
                {
                    AddToLog($"Ошибка при чтении параметра списка {paramFilename}");
                }

            }
        }
        /// <summary>
        /// Проверка целостности списка параметров
        /// </summary>
        /// <returns></returns>
        private static bool CheckParamList()
        {
            string[] list, ac;
            DateTime lm = new DateTime();
            Param test;
            try
            {
                list = System.IO.File.ReadAllLines(paramFilename);
            }
            catch (FileNotFoundException)
            {
                AddToLog($"Файл {paramFilename} не существует. Результат проверки отрицательный.");
                return false;
            }
            for (int i = 0; i < list.Length; i++)
            {
                try
                {
                    ac = list[i].Split(new char[] { ';' });
                    lm = DateTime.ParseExact(Regex.Match(ac[1], @"[0-9]{2}.[0-9]{2}.[0-9]{4} [0-9]{1,2}:[0-9]{2}:[0-9]{2}").Groups[0].Value, "dd.MM.yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                    test = new Param(Convert.ToInt64(ac[0]), lm, Convert.ToBoolean(ac[2]), Convert.ToBoolean(ac[3]));
                }
                catch (Exception)
                {
                    AddToLog($"Ошибка при чтении {paramFilename}. Результат проверки отрицательный.");
                    return false;
                }

            }
            return true;
        }
        /// <summary>
        /// Создаёт резервную копию списка параметров
        /// </summary>
        public static void BackupParamList()
        {
            if (CheckParamList())
            {
                System.IO.File.Copy(paramFilename, paramFileBackup, true);
            }
        }
        /// <summary>
        /// Восстанавливает резервную копию списка параметров
        /// </summary>
        public static void RestoreParamList()
        {
            System.IO.File.Copy(paramFileBackup, paramFilename, true);
        }

        /// <summary>
        /// Запись данных в файл
        /// </summary>
        public static void SaveSettings()
        {
            StringBuilder text = new StringBuilder();
            
            text.Append($"{useTelegramBot}\n{token}\n{GPUlimit}\n{MEMlimit}\n{filename}\n{autorun}\n");
            try
            {
                File.WriteAllText(settingsfilename, text.ToString());
            }
            catch (Exception ex)
            {
                AddToLog($"Ошибка при попытке записи файла {settingsfilename}");
            }
            
            
        }
        /// <summary>
        /// Чтение параметров из файла
        /// </summary>
        public static void ReadSettings()
        {
            string[] list;
            try
            {
                list = System.IO.File.ReadAllLines(settingsfilename);
                useTelegramBot = Convert.ToBoolean(list[0]);
                token = list[1];
                GPUlimit = Convert.ToDouble(list[2]);
                MEMlimit = Convert.ToDouble(list[3]);
                filename = list[4];
                autorun = Convert.ToBoolean(list[5]);
            }
            catch (FileNotFoundException)
            {
                AddToLog($"Файл {settingsfilename} не существует.");
            }


        }

        /// <summary>
        /// Обновить значения температур.
        /// </summary>
        public static void GetGPUZValues()
        {
            FileInfo log = new FileInfo(filename);
            string[] ac;

            try
            {
                using (StreamReader streamReader = new StreamReader(log.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
                {
                    line = string.Empty;
                    while ((line = streamReader.ReadLine()) != null)
                    {
                        if (meQueue.Count == LINES_KEPT)
                            meQueue.Dequeue();

                        meQueue.Enqueue(line);
                    }
                    line = meQueue.Dequeue();
                    ac = line.Split(new char[] { ',' });
                }
                GPUtemp = Convert.ToDouble(ac[3].Replace(" ", "").Replace(".", ","));
                MEMtemp = Convert.ToDouble(ac[5].Replace(" ", "").Replace(".", ",")); 
                dataDate = DateTime.ParseExact(Regex.Match(ac[0], @"[0-9]{4}-[0-9]{2}-[0-9]{2} [0-9]{1,2}:[0-9]{2}:[0-9]{2}").Groups[0].Value, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture); //2022-05-20 15:47:25
                
            }
            catch (Exception ex)
            {
                AddToLog($"Ошибка при попытке чтения {filename} {ex} {line}");
            }
            
        }

        public static bool Get(string url) //Проверка url на доступность
        {
            HttpWebRequest httpWRequest = (HttpWebRequest)WebRequest.Create(url);
            httpWRequest.Method = "GET";
            httpWRequest.Timeout = 50;
            httpWRequest.UserAgent = "Mozilla / 5.0(Windows NT 10.0; Win64; x64; rv: 65.0) Gecko / 20100101 Firefox / 65.0";
            httpWRequest.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
            httpWRequest.KeepAlive = true;
            httpWRequest.CookieContainer = new CookieContainer();

            HttpWebResponse httpWResponse = (HttpWebResponse)httpWRequest.GetResponse();

            if (httpWResponse.ContentLength == 0)
            {
                return false;
            }

            StreamReader sr = httpWResponse.Headers["Content-Type"].IndexOf("windows-1251", StringComparison.Ordinal) > 0 ?
                new StreamReader(httpWResponse.GetResponseStream(), Encoding.GetEncoding("windows-1251")) :
                new StreamReader(httpWResponse.GetResponseStream(), Encoding.UTF8);
            return true; //sr.ReadToEnd();
        }

        /// <summary>
        /// Отмечаемся на сервере что всё хорошо.
        /// </summary>
        /// <param name="tempGPU">Температура GPU</param>
        /// <param name="tempMEM">Температура памяти</param>
        /// <param name="limGPU">Лимит температуры gpu, если 0 то не отправляется</param>
        /// <param name="limMEM">Лимит температуры памяти, если 0 то не отправляется</param>
        /// <returns>Возвращает true если всё прошло гладко</returns>
        public static bool PostData(double tempGPU, double tempMEM, double limGPU, double limMEM)
        {
            string url = "http://85.234.3.164/GetData.php";
            StringBuilder sb = new StringBuilder();
            string[] ac;
            string tgpu = GPUtemp.ToString().Replace(",","."), tmem = MEMtemp.ToString().Replace(",", "."), lgpu, lmem;
            double resGPUlim, resMemLim;
            bool save = false;
            if (limGPU != 0) lgpu = limGPU.ToString(); else lgpu = "";
            if (limMEM != 0) lmem = limMEM.ToString(); else lmem = "";
            //Посылаем запрос с ID вопроса, чтобы получить правильный вариант ответа.
            try
            {
                using (WebClient client = new WebClient())
                {
                    client.Headers.Add("Referer", url); client.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/98.0.4758.109 Safari/537.36 OPR/84.0.4316.31");
                    client.Headers.Add("X-Requested-With", "XMLHttpRequest");
                    byte[] response =
                    client.UploadValues(url, new NameValueCollection()
                    {
                            { "Token", token },
                            { "GPUtemp", tgpu },
                            { "GPUlim", lgpu },
                            { "MEMtemp", tmem },
                            { "MEMlim", lmem }
                    });
                    //
                    sb.Append(Encoding.UTF8.GetString(response));
                }
                ac = sb.ToString().Split(new char[] { ';' });
                resGPUlim = Convert.ToDouble(ac[0].Replace(".", ","));
                resMemLim = Convert.ToDouble(ac[1].Replace(".", ","));
                //AddToLog($"Отпарвленные лимиты GPU {limGPU}, MEM {limMEM}. Полученные лимиты GPU {resGPUlim} MEM {resMemLim}. Ожидаемые лимиты GPU {inputGPUlimit} MEM {inputMEMlimit}.");

                if (inputGPUlimit == resGPUlim && inputMEMlimit == resMemLim)
                {
                    sendingLimits = false;
                }

                if (sendingLimits == false) //В режиме отправки лимитов, данные не должны записываться
                {
                    if (ac[0].Length > 1)
                    {
                        if (GPUlimit != resGPUlim)
                        {
                            GPUlimit = resGPUlim;
                            save = true;
                            AddToLog($"Установлен лимит GPU {GPUlimit}");
                        }

                    }
                    if (ac[1].Length > 1)
                    {
                        if (MEMlimit != resMemLim)
                        {
                            MEMlimit = resMemLim;
                            save = true;
                            AddToLog($"Установлен лимит памяти {MEMlimit}");
                        }
                    }
                }
                
                if (save) SaveSettings();
                return true;
            }
            catch (Exception e)
            {
                AddToLog($"Ошибка при попытке получить ответ от сайта {url} {e}");
                return false;
            }
        }

        /// <summary>
        /// Проверить обнвления программы
        /// </summary>
        public static bool CheckUpdate()
        {
            string url = "http://85.234.3.164/GPUtraceClient.php", resp;

            using (WebClient wc = new WebClient())
            {
                try
                {
                    wc.Headers.Add("user-agent", "Mozilla/5.0 (Windows; Windows NT 5.1; rv:1.9.2.4) Gecko/20100611 Firefox/3.6.4");
                    resp = wc.DownloadString(url);
                }
                catch (Exception)
                {
                    AddToLog($"Ошибка при попытке проверить обновления.");
                    resp = "Ошибка";
                }
                
            }

            
            if (Regex.IsMatch(resp, @"GPU Trace Client v[0-9.]{1,3}") && resp != "Ошибка")
            {
                string searchString = Regex.Match(resp, @"GPU Trace Client v[0-9.]{1,3}").Groups[0].Value;
                updVersion = Regex.Match(searchString, @"[0-9.]{1,3}").Groups[0].Value;
                if (Convert.ToDouble(version.Replace(".", ",")) < Convert.ToDouble(updVersion.Replace(".", ",")))
                {
                    AddToLog($"Вышло обновление, версия {updVersion}");
                    return true;
                }
                else
                {
                    AddToLog($"Обновлений нет, версия актуальная {version}");
                    return false;
                }
            }
            return false;

        }
    }
}
