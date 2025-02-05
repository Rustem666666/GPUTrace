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
