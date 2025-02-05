using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Media;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;

namespace GPU_trace
{
    //////////////////Блок прозрачности окна
    internal enum AccentState
    {
        ACCENT_DISABLED = 1,
        ACCENT_ENABLE_GRADIENT = 0,
        ACCENT_ENABLE_TRANSPARENTGRADIENT = 2,
        ACCENT_ENABLE_BLURBEHIND = 3,
        ACCENT_INVALID_STATE = 4
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct AccentPolicy
    {
        public AccentState AccentState;
        public int AccentFlags;
        public int GradientColor;
        public int AnimationId;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct WindowCompositionAttributeData
    {
        public WindowCompositionAttribute Attribute;
        public IntPtr Data;
        public int SizeOfData;
    }

    internal enum WindowCompositionAttribute
    {
        // ...
        WCA_ACCENT_POLICY = 19
        // ...
    }

    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static int timerTick, timerUsual = 1;
        private const int checkMinutes = 1, backupSeconds = 30;
        private bool alarm;
        Error littleError;
        SoundPlayer simpleSound;
        WindowState prevState;
        public static bool settingwindow = false;
        public static int postDataInterval = 15;
        public static DateTime initDate = DateTime.Now; //Фиксируем время запуска приложения
        public static DateTime checkDate = DateTime.Now; //Фиксируем время проверки

        Settings settings = new Settings();



        public MainWindow()
        {
            DispatcherTimer timer = new DispatcherTimer();
            simpleSound = new SoundPlayer(@"data//Balloon.wav");
            if (!Directory.Exists("data\\backup")) { Directory.CreateDirectory("data\\backup"); }
            //if (File.Exists(Monitor.paramFilename))
            //{
            //    try
            //    {
            //        Monitor.ReadParamList(ref Monitor.Params);
            //    }
            //    catch (Exception)
            //    {
            //        Monitor.AddToLog($"Ошибка при чтении {Monitor.paramFilename}, попытка восстановить...");
            //        try
            //        {
            //            Monitor.RestoreParamList();
            //            Monitor.ReadParamList(ref Monitor.Params);
            //        }
            //        catch (Exception)
            //        {
            //            Monitor.AddToLog($"Ошибка при попытке восстановления {Monitor.paramFilename}.");
            //        }
            //    }

            //}
            //else { Monitor.AddToLog($"Файл {Monitor.paramFilename} не найден"); }
            if (File.Exists(Monitor.settingsfilename)) { Monitor.ReadSettings(); } else { Monitor.AddToLog($"Файл {Monitor.settingsfilename} не найден"); }

            InitializeComponent();

            timerTick = timerUsual;
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += timer_Tick;
            timer.Start();
            tb_version.Text = $"ver {Monitor.version}";
            Title = "GPU trace";
            
            if (Monitor.useTelegramBot)
            {
                //Monitor.InitClient();
            }
            ShowInTaskbar = false;
            
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            bool error = true;
            if (File.Exists(Monitor.filename)) //Если файл лога существует, если нет, сразу пишем Нет данных
            {
                Monitor.GetGPUZValues();
                tbFilename.Text = Regex.Match(Monitor.filename, @"([\w\s]+\.\w+)").Groups[0].Value.Replace(".txt", "");
                
                if ((Int32)DateTime.Now.Subtract(Monitor.dataDate).TotalMinutes < 1)
                {
                    error = false;
                    
                } else { Monitor.AddToLog($"Данные устарели. Последние были: {Monitor.dataDate.ToString("F")}"); }
            }

            if (error) //Нет данных если true
            {
                tb_GPUtemp.Visibility = Visibility.Hidden;
                tb_GPUTitle.Visibility = Visibility.Hidden;
                tb_Memtemp.Visibility = Visibility.Hidden;
                tb_MemTitle.Visibility = Visibility.Hidden;
                tbFilename.Visibility = Visibility.Hidden;
                tb_NoData.Visibility = Visibility.Visible;
                tb_runGPUZ.Visibility = Visibility.Visible;

                border.BorderBrush = Brushes.Red;
                Monitor.AddToLog($"Файл {Monitor.filename} не найден.");
            }
            else
            {
                ///Значения по умолчанию. Скрываем всё
                tb_GPUtemp.Visibility = Visibility.Visible;
                tb_GPUTitle.Visibility = Visibility.Visible;
                tb_Memtemp.Visibility = Visibility.Visible;
                tb_MemTitle.Visibility = Visibility.Visible;
                tbFilename.Visibility = Visibility.Visible;
                tb_version.Visibility = Visibility.Visible;
                tb_Label.Visibility = Visibility.Visible;
                tb_NoData.Visibility = Visibility.Hidden;
                tb_runGPUZ.Visibility = Visibility.Hidden;
                tb_errortext.Visibility = Visibility.Hidden;
                tb_save.Visibility = Visibility.Hidden;

                tb_GPUtemp.Text = Monitor.GPUtemp.ToString()+"\u2103";
                tb_Memtemp.Text = Monitor.MEMtemp.ToString() + "\u2103";
                if (Monitor.GPUtemp >= Monitor.GPUlimit || Monitor.MEMtemp >= Monitor.MEMlimit)
                {
                    if (Monitor.GPUtemp >= Monitor.GPUlimit) Monitor.AddToLog($"Температура GPU превысила лимит {Monitor.GPUtemp}");
                    if (Monitor.MEMtemp >= Monitor.MEMlimit) Monitor.AddToLog($"Температура памяти превысила лимит {Monitor.MEMtemp}");
                    alarm = true;
                }
                else { alarm = false; }

                if (alarm) //Визуальный сигнал
                {
                    border.BorderBrush = Brushes.Red;
                    simpleSound.Play();
                    if (Monitor.GPUtemp >= Monitor.GPUlimit) { tb_GPUtemp.Foreground = Brushes.Red; tb_GPUTitle.Foreground = Brushes.Red; }
                    if (Monitor.MEMtemp >= Monitor.MEMlimit) { tb_Memtemp.Foreground = Brushes.Red; tb_MemTitle.Foreground = Brushes.Red; }
                }
                else
                {
                    border.BorderBrush = Brushes.Gold;
                    tb_GPUtemp.Foreground = Brushes.GreenYellow;
                    tb_GPUTitle.Foreground = Brushes.GreenYellow;
                    tb_Memtemp.Foreground = Brushes.GreenYellow;
                    tb_MemTitle.Foreground = Brushes.GreenYellow;

                    if (Monitor.sendingLimits)
                    {
                        tb_version.Visibility = Visibility.Hidden;
                        tb_Label.Visibility = Visibility.Hidden;
                        tb_errortext.Visibility = Visibility.Hidden;
                        tb_save.Visibility = Visibility.Visible;
                        postDataInterval = 2;
                    }
                    else
                    {
                        postDataInterval = 15;
                    }

                    if (Monitor.errors.Count > 0)
                    {
                        tb_version.Visibility = Visibility.Hidden;
                        tb_Label.Visibility = Visibility.Hidden;
                        tb_save.Visibility = Visibility.Hidden;
                        tb_errortext.Visibility = Visibility.Visible;

                        littleError = Monitor.errors.Pop();

                        tb_errortext.Text = littleError.DESCR;
                    }

                    FileInfo log = new FileInfo(Monitor.filename);
                    if (log.Length > 70000000) //Если файл вырос до 70 мб
                    {
                        try
                        {
                            log.Delete();
                        }
                        catch (Exception)
                        {
                            //Monitor.AddToLog($"Ошибка при попытке удаления {Monitor.filename}.");
                        }
                        if (Monitor.errors.Count < 15) Monitor.errors.Push(new Error(2, "Необходим перезапуск GPU-Z."));
                    }

                }

                if ((Int32)DateTime.Now.Subtract(checkDate).TotalSeconds >= postDataInterval) //Отмечаемся на сервере
                {
                    checkDate = DateTime.Now;

                    //if (Monitor.Get("http://85.234.3.164"))
                    //{
                    //    Monitor.AddToLog("Сервер доступен.");
                    //    if (Monitor.sendingLimits) Monitor.PostData(Monitor.GPUtemp, Monitor.MEMtemp, Monitor.GPUlimit, Monitor.MEMlimit); else Monitor.PostData(Monitor.GPUtemp, Monitor.MEMtemp, 0, 0);
                    //}
                    //else
                    //{
                    //    Monitor.AddToLog("Сервер не отвечает.");
                    //    Monitor.errors.Clear();
                    //    Monitor.errors.Push(new Error(1, "Сервер не отвечает."));
                    //}

                    if (Monitor.sendingLimits)
                    {
                        if (Monitor.PostData(Monitor.GPUtemp, Monitor.MEMtemp, Monitor.GPUlimit, Monitor.MEMlimit) == false)
                        {
                            Monitor.errors.Clear();
                            Monitor.errors.Push(new Error(1, "Сервер не отвечает."));
                        }
                    }
                    else
                    {
                        if (Monitor.PostData(Monitor.GPUtemp, Monitor.MEMtemp, 0, 0) == false)
                        {
                            Monitor.errors.Clear();
                            Monitor.errors.Push(new Error(1, "Сервер не отвечает."));
                        }
                    }


                }

                //if ((Int32)DateTime.Now.Subtract(checkDate).TotalSeconds >= backupSeconds && Monitor.Params.Count != 0) Monitor.BackupParamList(); //Бэкап каждые 30 секунд

                //if ((Int32)DateTime.Now.Subtract(checkDate).TotalMinutes >= checkMinutes) //Проверка раз в минуту
                //{
                //    checkDate = DateTime.Now;
                //    if (Monitor.Params.Count != 0) { Monitor.SaveParamList(ref Monitor.Params); } //Сохраняем список параметров в файл


                //    foreach (var chat in Monitor.Params)
                //    {
                //        if (chat.TRACEGPU)// 
                //        {
                //            if (Monitor.GPUtemp >= Monitor.GPUlimit) Monitor.Send(chat.CHATID, $"{Monitor.TextConverter($"Температура GPU превысила лимит:\n")} *{StringTransform.TransformCase(Monitor.GPUtemp + " ", (long)Monitor.GPUtemp, "градус", "градуса", "градусов")}*");
                //        }
                //        if (chat.TRACEMEM)
                //        {
                //            if (Monitor.MEMtemp >= Monitor.MEMlimit) Monitor.Send(chat.CHATID, $"{Monitor.TextConverter($"Температура памяти превысила лимит:\n")} *{StringTransform.TransformCase(Monitor.MEMtemp + " ", (long)Monitor.MEMtemp, "градус", "градуса", "градусов")}*");
                //        }
                //    }
                //}


                if (settingwindow) settings.UpdateSetinfo();
            }
        }


        [DllImport("user32.dll")]
        internal static extern int SetWindowCompositionAttribute(IntPtr hwnd, ref WindowCompositionAttributeData data);

        /// <summary>
        /// Инициализация прозрачности
        /// </summary>
        internal void EnableBlur()
        {
            var windowHelper = new WindowInteropHelper(this);

            var accent = new AccentPolicy();
            accent.AccentState = AccentState.ACCENT_ENABLE_BLURBEHIND;

            var accentStructSize = Marshal.SizeOf(accent);

            var accentPtr = Marshal.AllocHGlobal(accentStructSize);
            Marshal.StructureToPtr(accent, accentPtr, false);

            var data = new WindowCompositionAttributeData();
            data.Attribute = WindowCompositionAttribute.WCA_ACCENT_POLICY;
            data.SizeOfData = accentStructSize;
            data.Data = accentPtr;

            SetWindowCompositionAttribute(windowHelper.Handle, ref data);

            Marshal.FreeHGlobal(accentPtr);
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            EnableBlur();
            if (Monitor.CheckUpdate())
            {
                UpdateWindow updateWindow = new UpdateWindow();
                updateWindow.Owner = this;
                if (updateWindow.ShowDialog() == true)
                {
                    //Show();
                    //MessageBox.Show("Обновляем");
                    Process.Start("http://85.234.3.164/GPUtraceClient.php");
                }
                else
                {
                    //Show();
                    //MessageBox.Show("Не обновляем.");
                }
            }
        }

        /// <summary>
        /// Перетаскивание окна
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ToolBar_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                try
                {
                    this.DragMove();
                }
                catch (Exception)
                {

                }

            }
        }
        /// <summary>
        /// Кнопка закрытия приложения
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CloseButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            settings.Close();
            this.Close();
        }
        /// <summary>
        /// Кнопка сворачивания
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MinButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
            Hide();
        }
        /// <summary>
        /// Кнопка сохранения
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SettingsButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //Settings settings = new Settings();
            settings.Owner = this;
            if (settingwindow == false)
            {
                this.Hide();
                //if (settings.ShowDialog() == true)
                //{
                //    Show();
                //    //MessageBox.Show("Сохранено");
                //}
                //else
                //{
                //    Show();
                //    //MessageBox.Show("Не сохранено.");
                //}
                settings.Show();
                settings.InitWindow();
                settingwindow = true;
            }
            
            
        }

        /// <summary>
        /// Сворачивание в трей
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
                Hide();
            else
                prevState = WindowState;
        }
        
        /// <summary>
        /// Разворачивание из трея
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TaskbarIcon_TrayLeftMouseDown(object sender, RoutedEventArgs e)
        {
            Show();
            WindowState = prevState;
        }

    }


}
