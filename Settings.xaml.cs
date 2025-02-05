using IWshRuntimeLibrary;
using Microsoft.Win32;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace GPU_trace
{

    /// <summary>
    /// Логика взаимодействия для Settings.xaml
    /// </summary>
    public partial class Settings : Window
    {
        private static bool matchToken = true, matchGPUlim = true, matchMEMlim = true;

        public Settings()
        {
            InitializeComponent();
        }
        
        public void InitWindow()
        {
            tb_version.Text = Monitor.version;
            if (System.IO.File.Exists("data\\Settings.ini"))
            {
                tbox_token.Text = Monitor.token;
                tbox_GPUlim.Text = Monitor.GPUlimit.ToString();
                tbox_MEMlim.Text = Monitor.MEMlimit.ToString();
                tbox_filename.Text = Monitor.filename;
            }
            if (Monitor.autorun) check_autorun.IsChecked = true; else check_autorun.IsChecked = false;
            if (Monitor.useTelegramBot)
            {
                check_telegram.IsChecked = true;

            }
            else
            {
                check_telegram.IsChecked = false;
                tbox_token.IsEnabled = false;
            }
            HideIcons();
        }

        /// <summary>
        /// Срабатывает раз в секунду из основного окна
        /// </summary>
        public void UpdateSetinfo()
        {
            ToggleStatus();
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
            //this.DialogResult = false;
            MainWindow.settingwindow = false;
            this.Hide();
            Owner.Show();
        }
        /// <summary>
        /// Кнопка сохранения
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveButton_MouseDown(object sender, MouseButtonEventArgs e)
        {

            Monitor.filename = tbox_filename.Text;

            if (check_telegram.IsChecked == true)
            {
                if (matchToken)
                {
                    Monitor.useTelegramBot = true;
                    Monitor.token = tbox_token.Text;
                    img_tokenPositive.Visibility = Visibility.Visible;
                }
                else
                {
                    Monitor.AddToLog("Токен введён не верно.");
                    img_tokenNegative.Visibility = Visibility.Visible;
                }
            }

            if (check_autorun.IsChecked == true)
            {
                FileInfo fileinfo = new FileInfo("GPU trace.exe");
                try
                {
                    CreateShortcut("GPU trace", Environment.GetFolderPath(Environment.SpecialFolder.Startup), fileinfo.FullName); //Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
                    Monitor.autorun = true;
                    img_AoutostartPositive.Visibility = Visibility.Visible;
                }
                catch (Exception ex)
                {
                    Monitor.AddToLog($"Ошибка при попытке создать ярлык.\n{ex}");
                    Monitor.autorun = false;
                    img_AutostartGPUNegative.Visibility = Visibility.Visible;
                }
            }
            else
            {
                try
                {
                    //удаляем  
                    System.IO.File.Delete($"{Environment.GetFolderPath(Environment.SpecialFolder.Startup)}\\GPU trace.lnk");
                    img_AoutostartPositive.Visibility = Visibility.Visible;
                }
                catch (Exception ex)
                {
                    Monitor.AddToLog($"Ошибка при попытке удалить ярлык.\n{ex}");
                    img_AutostartGPUNegative.Visibility = Visibility.Visible;
                }

            }
            if (matchGPUlim && tbox_GPUlim.Text.Length != 0) { Monitor.inputGPUlimit = Convert.ToDouble(tbox_GPUlim.Text); img_GPUPositive.Visibility = Visibility.Visible; } else { Monitor.inputGPUlimit = Monitor.GPUlimit; img_GPUNegative.Visibility = Visibility.Visible; }
            if (matchMEMlim && tbox_MEMlim.Text.Length != 0) { Monitor.inputMEMlimit = Convert.ToDouble(tbox_MEMlim.Text); img_MEMPositive.Visibility = Visibility.Visible; } else { Monitor.inputMEMlimit = Monitor.MEMlimit; img_MEMNegative.Visibility = Visibility.Visible; }

            if (Monitor.inputGPUlimit != Monitor.GPUlimit && matchGPUlim || Monitor.inputMEMlimit != Monitor.MEMlimit && matchMEMlim)
            {
                if (matchGPUlim) { Monitor.GPUlimit = Monitor.inputGPUlimit; }
                if (matchMEMlim) { Monitor.MEMlimit = Monitor.inputMEMlimit; }
                if (Monitor.useTelegramBot) Monitor.sendingLimits = true;
                Monitor.AddToLog($"Отправка значений");
            }
            Monitor.SaveSettings();
            //ToggleStatus();
            border.BorderBrush = Brushes.Green;
            //this.DialogResult = true;

        }

        /// <summary>
        /// Скрывает все иконки
        /// </summary>
        private void HideIcons()
        {
            img_AoutostartPositive.Visibility = Visibility.Hidden;
            img_AutostartGPUNegative.Visibility = Visibility.Hidden;
            img_tokenNegative.Visibility = Visibility.Hidden;
            img_tokenPositive.Visibility = Visibility.Hidden;
            img_GPUNegative.Visibility = Visibility.Hidden;
            img_GPUPositive.Visibility = Visibility.Hidden;
            img_MEMNegative.Visibility = Visibility.Hidden;
            img_MEMPositive.Visibility = Visibility.Hidden;
            tb_errortext.Visibility = Visibility.Hidden;
        }

        /// <summary>
        /// нажатие Enter
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnKeyDownHandler(object sender, KeyEventArgs e)
        {
            //if (e.Key == Key.Return)
            //{
            //    if (Regex.IsMatch(tbox_GPUlim.Text, @"[0-9]{1,3}")) { Monitor.GPUlimit = Convert.ToDouble(tbox_GPUlim.Text); }
            //    if (Regex.IsMatch(tbox_GPUlim.Text, @"[0-9]{1,3}")) { Monitor.MEMlimit = Convert.ToDouble(tbox_MEMlim.Text); }

            //    if (check_telegram.IsChecked == true)
            //    {
            //        Monitor.useTelegramBot = true;
            //        if (tbox_token.Text.Length > 10)
            //        {
            //            Monitor.token = tbox_token.Text;

            //        }
            //        else
            //        {
            //            MessageBox.Show("Токен введён не верно.");
            //            this.DialogResult = false;
            //        }
            //    }
            //    Monitor.SaveSettings();
            //    this.DialogResult = true;
            //}
        }

        /// <summary>
        /// Переключение элементов интерфейса
        /// </summary>
        public void ToggleStatus()
        {
            if (Monitor.sendingLimits)
            {
                tb_save.Visibility = Visibility.Visible;
            }
            else
            {
                tb_save.Visibility = Visibility.Hidden;
            }
        }

        /// <summary>
        /// Создание ярыка
        /// </summary>
        /// <param name="ShortcutPath"></param>
        /// <param name="TargetPath"></param>
        public static void CreateShortcut(string shortcutName, string shortcutPath, string targetFileLocation)
        {
            string shortcutLocation = System.IO.Path.Combine(shortcutPath, shortcutName + ".lnk");
            WshShell shell = new WshShell();
            IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutLocation);

            shortcut.Description = "Запуск приложения для отслеживания";   // The description of the shortcut
            shortcut.WorkingDirectory = targetFileLocation.Replace(shortcutName + ".exe","");
            shortcut.IconLocation = "Source\\GPUtrace.ico";           // The icon of the shortcut
            shortcut.TargetPath = targetFileLocation;                 // The path of the file that will launch when the shortcut is run
            shortcut.Save();                                    // Save the shortcut
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
            InitWindow();
        }

        /// <summary>
        /// Включение чекбокса телеграм
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void check_telegram_Checked(object sender, RoutedEventArgs e)
        {
            //HomeButler.useTelegramBot = true;
            tbox_token.IsEnabled = true;
        }

        /// <summary>
        /// Выключение чекбокса телеграм
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void check_telegram_Unchecked(object sender, RoutedEventArgs e)
        {
            //HomeButler.useTelegramBot = false;
            tbox_token.IsEnabled = false;
        }

        private void Tbox_filename_MouseDown(object sender, MouseButtonEventArgs e)
        {
            
        }

        private void But_Open_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
                tbox_filename.Text = openFileDialog.FileName;
        }

        /// <summary>
        /// Меняет цвет при неправильном вводе
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Tbox_token_KeyDown(object sender, KeyEventArgs e)
        {
            
        }
        /// <summary>
        /// Меняет цвет при неправильном вводе
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Tbox_GPUlim_KeyDown(object sender, KeyEventArgs e)
        {
            
        }
        /// <summary>
        /// Меняет цвет при неправильном вводе
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Tbox_token_KeyUp(object sender, KeyEventArgs e)
        {
            HideIcons();
            border.BorderBrush = Brushes.YellowGreen;
            matchToken = true;
            if (tbox_token.Text.Length <=20 && Regex.IsMatch(tbox_token.Text, @"[A-z]{14,20}"))
            {
                tbox_token.BorderBrush = Brushes.Green;
            }
            else
            {
                tbox_token.BorderBrush = Brushes.Red;
                matchToken = false;
            }
        }

        private void Storyboard_Completed(object sender, EventArgs e)
        {
            
        }

        private void tb_save_completed(object sender, EventArgs e)
        {
            
        }

        private void check_autorun_Checked(object sender, RoutedEventArgs e)
        {
            HideIcons();
        }

        private void check_autorun_Unchecked(object sender, RoutedEventArgs e)
        {
            HideIcons();
        }

        /// <summary>
        /// Меняет цвет при неправильном вводе
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Tbox_GPUlim_KeyUp(object sender, KeyEventArgs e)
        {
            HideIcons();
            border.BorderBrush = Brushes.YellowGreen;
            matchGPUlim = true;
            int input;
            if (Regex.IsMatch(tbox_GPUlim.Text, @"[0-9]{1,3}"))
            {
                input = Convert.ToInt32(tbox_GPUlim.Text);
                if (input < 40)
                {
                    tb_errortext.Text = $"Лимит не должен быть ниже стандартной температуры GPU!";
                    tb_errortext.Visibility = Visibility.Visible;
                    matchGPUlim = false;
                }
                if (input > 110)
                {
                    tb_errortext.Text = $"Предупреждать не имеет смысла, если GPU сгорит!";
                    tb_errortext.Visibility = Visibility.Visible;
                    matchGPUlim = false;
                }
            }


            if (matchGPUlim) tbox_GPUlim.BorderBrush = Brushes.Green; else tbox_GPUlim.BorderBrush = Brushes.Red;
        }
        /// <summary>
        /// Меняет цвет при неправильном вводе
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Tbox_MEMlim_KeyUp(object sender, KeyEventArgs e)
        {
            HideIcons();
            border.BorderBrush = Brushes.YellowGreen;
            matchMEMlim = true;
            int input;
            if (Regex.IsMatch(tbox_MEMlim.Text, @"[0-9]{1,3}"))
            {
                input = Convert.ToInt32(tbox_MEMlim.Text);
                if (input < 40)
                {
                    tb_errortext.Text = $"Лимит не должен быть ниже стандартной температуры памяти!";
                    tb_errortext.Visibility = Visibility.Visible;
                    matchMEMlim = false;
                }
                if (input > 110)
                {
                    tb_errortext.Text = $"Предупреждать не имеет смысла, если память сгорит!";
                    tb_errortext.Visibility = Visibility.Visible;
                    matchMEMlim = false;
                }
            }


            if (matchMEMlim) tbox_MEMlim.BorderBrush = Brushes.Green; else tbox_MEMlim.BorderBrush = Brushes.Red;
        }
    }
}
