using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using Microsoft.Win32;

using mshtml;

namespace Feedback
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        #region Fields

        // Узнаем, что мы в настройках
        private bool _AreInSettings = false;

        private string fname;

        #endregion Fields

        #region Constructors

        //private string SendToMail = "test@npso66.ru";

        public MainWindow()
        {
            //Timeline.DesiredFrameRateProperty.OverrideMetadata(typeof(Timeline), new FrameworkPropertyMetadata { DefaultValue = 1 });
            SetBrowserFeatureControl();

            InitializeComponent();

            App currentApp = Application.Current as App;
            currentApp.Contact_active = First_block;
            currentApp.Mailto = "support@fciit.ru";

            SolidColorBrush whiteBrush = new SolidColorBrush(Colors.White)
            {
                Opacity = 0.5
            };
            currentApp.Contact_active.Background = whiteBrush;

            First_block_img.RenderTransform = new TranslateTransform(0, 2.6);
            First_block_img.Source = new BitmapImage(new Uri("pack://application:,,,/Feedback;component/Resources/tick.png"));

            currentApp.Category_global_active = Category_Fond_global;
            currentApp.Category_global_active.Visibility = Visibility.Visible;

            Category_Fond_Sub_active = Sub_Fond_Enot;
            Category_Fond_Sub_active.Visibility = Visibility.Visible;

            for (int i = 0; i < Properties.Settings.Default.PassNumber; i++)
                Properties.Settings.Default.Password = Properties.Settings.Default.Password.DecryptString();

            Properties.Settings.Default.PassNumber = 0;
            Config_password.Password = Properties.Settings.Default.Password;

            DataContext = this;
            this.LocationChanged += new EventHandler(MainWindow_LocationChanged);
        }

        #endregion Constructors

        #region Properties

        // Переменная для определения, в настройках мы или нет
        public bool AreInSettings
        {
            get { return _AreInSettings; }
            set
            {
                _AreInSettings = value;
                OnPropertyChanged();
            }
        }

        // Доменное имя почты
        private string Host { get; set; }

        // Счетчик для того, чтобы выставлять настройки для Росреестра только один раз
        private int Nav_counter { get; set; } = 0;

        #endregion Properties

        #region Methods

        // Делаем скриншот
        public string TakeScreenShot()
        {
            fname = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()) + ".jpeg";
            Timer t = new Timer();
            t.Interval = 500; // In milliseconds
            t.AutoReset = false; // Stops it from repeating
            t.Elapsed += new ElapsedEventHandler(TimerElapsed);
            t.Start();

            return fname;
        }

        // Таймер для завершения работы программы
        private void ForceExit(object sender, ElapsedEventArgs e)
        {
            //App.Current.Shutdown();
            Environment.Exit(0);
        }

        // Сохраняем настройки
        private void SaveSettings()
        {
            Properties.Settings.Default.PassNumber += 1;
            Properties.Settings.Default.Password = Properties.Settings.Default.Password.EncryptString();
            Properties.Settings.Default.Save();
        }

        private void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width, System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height);
            using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bmp))
            {
                g.CopyFromScreen(0, 0, 0, 0, System.Windows.Forms.Screen.PrimaryScreen.Bounds.Size);
                bmp.Save(fname);
            }
        }

        #endregion Methods

        #region Переменные для файловых вложений

        // Файловые вложения к письму
        private string _FileAttachment_1, _FileAttachment_2, _FileAttachment_3;

        public event PropertyChangedEventHandler PropertyChanged;

        public string FileAttachment_1
        {
            get { return _FileAttachment_1; }
            set
            {
                _FileAttachment_1 = value;
                if (value != null)
                    ButtonAttachment_1.Content = " Удалить файл ";
                else
                    ButtonAttachment_1.Content = " Добавить файл ";
                OnPropertyChanged();
            }
        }

        public string FileAttachment_2
        {
            get { return _FileAttachment_2; }
            set
            {
                _FileAttachment_2 = value;
                if (value != null)
                    ButtonAttachment_2.Content = " Удалить файл ";
                else
                    ButtonAttachment_2.Content = " Добавить файл ";
                OnPropertyChanged();
            }
        }

        public string FileAttachment_3
        {
            get { return _FileAttachment_3; }
            set
            {
                _FileAttachment_3 = value;
                if (value != null)
                    ButtonAttachment_3.Content = " Удалить файл ";
                else
                    ButtonAttachment_3.Content = " Добавить файл ";
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Raises this object's PropertyChanged event.
        /// </summary>
        /// <param name="propertyName"> The property that has a new value. </param>
        protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName]string propertyName = null)
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null)
            {
                var e = new PropertyChangedEventArgs(propertyName);
                handler(this, e);
            }
        }

        #endregion Переменные для файловых вложений

        #region Настройка popup для росреестра

        private void mainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            PositionPopup();
        }

        private void MainWindow_LocationChanged(object sender, EventArgs e)
        {
            PositionPopup();
        }

        //Reposition the popup to keep it on the center of the window.
        private void PositionPopup()
        {
            var offset = close_min.HorizontalOffset;
            close_min.HorizontalOffset = offset + 1;
            close_min.HorizontalOffset = offset;
        }

        #endregion Настройка popup для росреестра

        #region Кнопки | Настройки

        // Настройки - > Кнопка "Перейти к написанию письма"
        private void BackToMainFormButton_Click(object sender, RoutedEventArgs e)
        {
            AreInSettings = false;
            SaveSettings();

            Header_configform.Visibility = Visibility.Collapsed;
            ConfigForm.Visibility = Visibility.Collapsed;

            Header_mailform.Visibility = Visibility.Visible;
            MailForm.Visibility = Visibility.Visible;
        }

        private void ButtonAttachment_Click(object sender, RoutedEventArgs e)
        {
            // Выбор действия кнопки: "Удалить файл"
            if ((sender as Button).Content.ToString() == " Удалить файл ")
            {
                switch ((sender as Button).Name)
                {
                    case "ButtonAttachment_1":
                        FileAttachment_1 = null;
                        break;

                    case "ButtonAttachment_2":
                        FileAttachment_2 = null;
                        break;

                    case "ButtonAttachment_3":
                        FileAttachment_3 = null;
                        break;

                    default:
                        break;
                }
            }
            else
            // Выбор действия кнопки: "Добавить файл"
            {
                OpenFileDialog file = new OpenFileDialog();
                file.Filter = "All Files (*.*)|*.*";
                file.FilterIndex = 1;
                file.Multiselect = true;

                if (file.ShowDialog() == true)
                    switch ((sender as Button).Name)
                    {
                        case "ButtonAttachment_1":
                            FileAttachment_1 = file.FileName;
                            break;

                        case "ButtonAttachment_2":
                            FileAttachment_2 = file.FileName;
                            break;

                        case "ButtonAttachment_3":
                            FileAttachment_3 = file.FileName;
                            break;

                        default:
                            break;
                    }
            }
        }

        // Кнопка "Сделать скриншот"
        private void ButtonScreenshot_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;

            Screenshot popup = new Screenshot();
            //popup.ShowDialog();
            popup.Visibility = Visibility.Visible;
        }

        // Кнопка "Закрыть"
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            SaveSettings();
            close_min.IsOpen = false;
            App.Current.MainWindow.Visibility = Visibility.Collapsed;

            Timer t = new Timer();
            t.Interval = 1000;
            t.AutoReset = true;
            t.Elapsed += new ElapsedEventHandler(ForceExit);
            t.Start();
        }

        // Кнопка "Свернуть"
        private void MinButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        // Кнопка "Отправить"
        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            // Скрываем правую часть и отображаем только "Отправка письма, подождите"
            Header_mailform.Visibility = Visibility.Collapsed;
            MailForm.Visibility = Visibility.Collapsed;
            Header_configform.Visibility = Visibility.Collapsed;
            ConfigForm.Visibility = Visibility.Collapsed;
            SendingAnimation.Visibility = Visibility.Visible;

            // Локальные переменные для удобства
            App currentApp = Application.Current as App;
            Properties.Settings Settings = Properties.Settings.Default;

            // Формируем контактную информацию
            string phone_num = Regex.Replace(Config_phone.Text, @"^\+7", "98").Replace("-", "");
            TimeZone localZone = TimeZone.CurrentTimeZone;
            DateTime currentDate = DateTime.Now;
            TimeSpan currentOffset = localZone.GetUtcOffset(currentDate);
            string curr_utc = "UTC+" + currentOffset.ToString().Substring(0, 2);
            string ProblemStart = "Контактное лицо: " + Settings.FIO +
                                   System.Environment.NewLine +
                                   "Телефон для связи: " + phone_num +
                                   System.Environment.NewLine +
                                   "Часовой пояс: " + curr_utc;
            ProblemStart = ProblemStart + "-----------" + System.Environment.NewLine;
            string ProblemEnd = new TextRange(Problem.Document.ContentStart, Problem.Document.ContentEnd).Text;
            string ProblemInfo = ProblemStart + ProblemEnd;

            //string Priority_current = ((ComboBoxItem)Priority.SelectedItem).Content.ToString();

            // Находим активную категорию
            ComboBox Category_active = App.FindChild<ComboBox>(currentApp.Category_global_active, "");

            // Формируем Заголовок письма
            string Category_Fond_current = "";
            if (Category_Fond_global.Visibility == Visibility.Visible)
            {
                Category_Fond_current = " -> " + ((ComboBoxItem)Category_Fond_Sub_active.SelectedItem).Content.ToString();
            }

            string MailHeader = Category_active.Text + Category_Fond_current;

            //// Находим домен
            //System.Net.Mail.MailAddress address = new System.Net.Mail.MailAddress(Settings.Mail);
            //Host = address.Host;

            // Пробуем подключиться
            if (!SmtpHelper.TestConnection(Settings.Server, Settings.Port))
            {
                MessageBox.Show("Не удается подключиться к почтовому серверу, проверьте настройки.");
            }
            else
            {
                SmtpHelper.Counter = 0;
                SmtpHelper.SendMail(ProblemInfo, MailHeader, currentApp.Mailto, Settings.User, Settings.Password, Settings.Mail, Settings.Server, Settings.Port, FileAttachment_1, FileAttachment_2, FileAttachment_3);
            }

            // Снова показываем правую часть
            SendingAnimation.Visibility = Visibility.Collapsed;
            Header_mailform.Visibility = Visibility.Visible;
            MailForm.Visibility = Visibility.Visible;
        }

        // Кнопка "Настройки"
        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            AreInSettings = true;
            Header_mailform.Visibility = Visibility.Collapsed;
            MailForm.Visibility = Visibility.Collapsed;

            Header_configform.Visibility = Visibility.Visible;
            ConfigForm.Visibility = Visibility.Visible;
        }

        #endregion Кнопки | Настройки

        #region Подменю категорий для ФЦИИТ

        public ComboBox Category_Fond_Sub_active { get; private set; }

        private void Category_Fond_DropDownClosed(object sender, EventArgs e)
        {
            switch ((sender as ComboBox).Text)
            {
                case "Сервисы ЕИС еНот":
                    Category_Fond_Sub_active.Visibility = Visibility.Collapsed;
                    Category_Fond_Sub_active = Sub_Fond_Enot;
                    Category_Fond_Sub_active.Visibility = Visibility.Visible;
                    break;

                case "Сервисы ЕИС 2.0":
                    Category_Fond_Sub_active.Visibility = Visibility.Collapsed;
                    Category_Fond_Sub_active = Sub_Fond_EIS;
                    Category_Fond_Sub_active.Visibility = Visibility.Visible;
                    break;

                case "Порталы":
                    Category_Fond_Sub_active.Visibility = Visibility.Collapsed;
                    Category_Fond_Sub_active = Sub_Fond_services;
                    Category_Fond_Sub_active.Visibility = Visibility.Visible;
                    break;

                case "Внешние сервисы":
                    Category_Fond_Sub_active.Visibility = Visibility.Collapsed;
                    Category_Fond_Sub_active = Sub_Fond_outside_services;
                    Category_Fond_Sub_active.Visibility = Visibility.Visible;
                    break;

                case "Прочее":
                    Category_Fond_Sub_active.Visibility = Visibility.Collapsed;
                    Category_Fond_Sub_active = Sub_Fond_other;
                    break;

                default:
                    break;
            }
        }

        #endregion Подменю категорий для ФЦИИТ

        #region Настройка поведения различных элементов (потеря фокус, изменение текста, переход на страницу, окончание загрузки)

        // Потеря фокуса для поля config_mail
        private void Config_mail_LostFocus(object sender, RoutedEventArgs e)
        {
            if (AreInSettings)
            {
                if (Mx.GetMXRecords(Host)[0] != "Ошибка")
                {
                    string[] email_adr = Mx.GetMXRecords(Host)[0].Split('.');
                    int count = email_adr.Length;
                    string email_domain = email_adr[count - 2];
                    string email_ext = email_adr[count - 1];
                    if (email_domain != "google")
                        Properties.Settings.Default.Server = "smtp." + email_domain + "." + email_ext;
                }
            }
        }

        // Проверяем, что вводят в поле config_mail
        private void Config_mail_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (AreInSettings)
            {
                TextBox curTextBox = sender as TextBox;
                Properties.Settings.Default.User = curTextBox.Text;
                if (Properties.Settings.Default.Mail != null)
                {
                    try
                    {
                        System.Net.Mail.MailAddress address = new System.Net.Mail.MailAddress(curTextBox.Text);
                        Host = address.Host;
                        Properties.Settings.Default.Server = "smtp." + Host;
                    }
                    catch (Exception)
                    {
                        Properties.Settings.Default.Server = "Сервер SMTP не найден, введите его вручную.";
                    }
                }
            }
        }

        // Проверяем, что вводят в поле config_password
        private void Config_password_PasswordChanged(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.Password = Config_password.Password;
        }

        // Переход на страницу в браузере
        private void WebBrowser_Navigating(object sender, System.Windows.Navigation.NavigatingCancelEventArgs e)
        {
            Nav_counter++;

            dynamic activeX = webBrowser1.GetType().InvokeMember("ActiveXInstance",
                BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.NonPublic,
                null, webBrowser1, new object[] { });

            activeX.Silent = true;

            WebBrowser wb = (WebBrowser)sender;

            if (Nav_counter <= 1)
            {
                wb.Width = 0;
                wb.Height = 0;
            }
        }

        // Браузер - > загрузка окончена, что делаем?
        private void WebBrowser1_LoadCompleted(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            string Browser_Zoom;
            double dpi;

            WebBrowser wb = (WebBrowser)sender;

            string script = "document.body.style.overflowX ='hidden'";
            wb.InvokeScript("execScript", new Object[] { script, "JavaScript" });

            PresentationSource source = PresentationSource.FromVisual(this);
            dpi = 96.0 * source.CompositionTarget.TransformToDevice.M11;

            if ((dpi >= 96) && (dpi <= 119)) { Browser_Zoom = "0.8"; }
            else if ((dpi >= 120) && (dpi <= 143)) { Browser_Zoom = "1.01"; }
            else if ((dpi >= 144) && (dpi <= 167)) { Browser_Zoom = "1.2"; }
            else { Browser_Zoom = "1.3"; }

            script = "document.body.style.zoom = " + Browser_Zoom;
            wb.InvokeScript("execScript", new Object[] { script, "JavaScript" });

            var document = (IHTMLDocument3)webBrowser1.Document;

            if (!(sender is WebBrowser browser) || browser.Document == null)
                return;

            dynamic doc = browser.Document;

            if (doc.readyState != "complete")
                return;

            doc.oncontextmenu += new Func<bool>(() => false);

            var head = document.getElementsByTagName("header"); //.OfType<mshtml.HTMLHeadElement>().FirstOrDefault();
            var foot = document.getElementsByTagName("footer");

            foreach (mshtml.IHTMLElement element in head)
            {
                element.style.display = "none";
            }

            foreach (mshtml.IHTMLElement element in foot)
            {
                element.style.display = "none";
            }

            wb.Width = 628;
            wb.Height = 700;

            //document.getElementById("test_screen").style.display = "none";

            // The first parameter is the url, the second is the index of the added style sheet.
            //IHTMLStyleSheet ss = doc.createStyleSheet("", 0);
        }

        #endregion Настройка поведения различных элементов (потеря фокус, изменение текста, переход на страницу, окончание загрузки)

        #region Настройка Webbrowser для правильной эмуляции IE на клиентских машинах

        private UInt32 GetBrowserEmulationMode()
        {
            int browserVersion = 7;
            using (var ieKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Internet Explorer",
                RegistryKeyPermissionCheck.ReadSubTree,
                System.Security.AccessControl.RegistryRights.QueryValues))
            {
                var version = ieKey.GetValue("svcVersion");
                if (null == version)
                {
                    version = ieKey.GetValue("Version");
                    if (null == version)
                        throw new ApplicationException("Microsoft Internet Explorer is required!");
                }
                int.TryParse(version.ToString().Split('.')[0], out browserVersion);
            }

            UInt32 mode = 11001; // Internet Explorer 11. Webpages containing standards-based !DOCTYPE directives are displayed in IE11 Standards mode. Default value for Internet Explorer 11.
            switch (browserVersion)
            {
                case 7:
                    mode = 7000; // Webpages containing standards-based !DOCTYPE directives are displayed in IE7 Standards mode. Default value for applications hosting the WebBrowser Control.
                    break;

                case 8:
                    mode = 8888; // Webpages containing standards-based !DOCTYPE directives are displayed in IE8 mode. Default value for Internet Explorer 8
                    break;

                case 9:
                    mode = 9999; // Internet Explorer 9. Webpages containing standards-based !DOCTYPE directives are displayed in IE9 mode. Default value for Internet Explorer 9.
                    break;

                case 10:
                    mode = 10001; // Internet Explorer 10. Webpages containing standards-based !DOCTYPE directives are displayed in IE10 mode. Default value for Internet Explorer 10.
                    break;

                default:
                    // use IE11 mode by default
                    break;
            }

            return mode;
        }

        private void SetBrowserFeatureControl()
        {
            // http://msdn.microsoft.com/en-us/library/ee330720(v=vs.85).aspx

            // FeatureControl settings are per-process
            var fileName = System.IO.Path.GetFileName(Process.GetCurrentProcess().MainModule.FileName);

            // make the control is not running inside Visual Studio Designer
            if (String.Compare(fileName, "devenv.exe", true) == 0 || String.Compare(fileName, "XDesProc.exe", true) == 0)
                return;

            SetBrowserFeatureControlKey("FEATURE_ADDON_MANAGEMENT", fileName, 0);
            SetBrowserFeatureControlKey("FEATURE_AJAX_CONNECTIONEVENTS", fileName, 1);
            SetBrowserFeatureControlKey("FEATURE_BLOCK_LMZ_OBJECT", fileName, 0);
            SetBrowserFeatureControlKey("FEATURE_BLOCK_LMZ_SCRIPT", fileName, 0);
            SetBrowserFeatureControlKey("FEATURE_BROWSER_EMULATION", fileName, GetBrowserEmulationMode()); // Webpages containing standards-based !DOCTYPE directives are displayed in IE10 Standards mode.
            SetBrowserFeatureControlKey("FEATURE_DISABLE_LEGACY_COMPRESSION", fileName, 1);
            SetBrowserFeatureControlKey("FEATURE_DISABLE_NAVIGATION_SOUNDS", fileName, 1);
            SetBrowserFeatureControlKey("FEATURE_DOMSTORAGE ", fileName, 1);
            SetBrowserFeatureControlKey("FEATURE_ENABLE_CLIPCHILDREN_OPTIMIZATION", fileName, 1);
            SetBrowserFeatureControlKey("FEATURE_GPU_RENDERING ", fileName, 1);
            SetBrowserFeatureControlKey("FEATURE_IVIEWOBJECTDRAW_DMLT9_WITH_GDI  ", fileName, 0);
            SetBrowserFeatureControlKey("FEATURE_LOCALMACHINE_LOCKDOWN", fileName, 0);
            SetBrowserFeatureControlKey("FEATURE_MANAGE_SCRIPT_CIRCULAR_REFS", fileName, 1);
            SetBrowserFeatureControlKey("FEATURE_SCRIPTURL_MITIGATION", fileName, 1);
            SetBrowserFeatureControlKey("FEATURE_SPELLCHECKING", fileName, 0);
            SetBrowserFeatureControlKey("FEATURE_STATUS_BAR_THROTTLING", fileName, 1);
            SetBrowserFeatureControlKey("FEATURE_TABBED_BROWSING", fileName, 0);
            SetBrowserFeatureControlKey("FEATURE_VALIDATE_NAVIGATE_URL", fileName, 1);
            SetBrowserFeatureControlKey("FEATURE_WEBOC_DOCUMENT_ZOOM", fileName, 1);
            SetBrowserFeatureControlKey("FEATURE_WEBOC_MOVESIZECHILD", fileName, 1);
            SetBrowserFeatureControlKey("FEATURE_WEBOC_POPUPMANAGEMENT", fileName, 0);
            SetBrowserFeatureControlKey("FEATURE_WEBSOCKET", fileName, 1);
            SetBrowserFeatureControlKey("FEATURE_WINDOW_RESTRICTIONS ", fileName, 0);
            SetBrowserFeatureControlKey("FEATURE_XMLHTTP", fileName, 1);
        }

        private void SetBrowserFeatureControlKey(string feature, string appName, uint value)
        {
            using (var key = Registry.CurrentUser.CreateSubKey(
                String.Concat(@"Software\Microsoft\Internet Explorer\Main\FeatureControl\", feature),
                RegistryKeyPermissionCheck.ReadWriteSubTree))
            {
                key.SetValue(appName, (UInt32)value, RegistryValueKind.DWord);
            }
        }

        #endregion Настройка Webbrowser для правильной эмуляции IE на клиентских машинах
    }

    #region Соленье / приправы

    public static class StringSecurityHelper
    {
        private static readonly byte[] entropy = Encoding.Unicode.GetBytes("1AQ'&mc %;lq@31*@!)@asd@.,,z2389");

        public static string DecryptString(this string encryptedData)
        {
            if (encryptedData == null)
            {
                return null;
            }

            try
            {
                byte[] decryptedData = ProtectedData.Unprotect(Convert.FromBase64String(encryptedData), entropy, DataProtectionScope.CurrentUser);

                return Encoding.Unicode.GetString(decryptedData);
            }
            catch
            {
                return null;
            }
        }

        public static string EncryptString(this string input)
        {
            if (input == null)
            {
                return null;
            }

            byte[] encryptedData = ProtectedData.Protect(Encoding.Unicode.GetBytes(input), entropy, DataProtectionScope.CurrentUser);

            return Convert.ToBase64String(encryptedData);
        }
    }

    #endregion Соленье / приправы

    #region Настройка "привязок"

    public class SettingBindingExtension : Binding
    {
        #region Constructors

        public SettingBindingExtension()
        {
            Initialize();
        }

        public SettingBindingExtension(string path)
            : base(path)
        {
            Initialize();
        }

        #endregion Constructors

        #region Methods

        private void Initialize()
        {
            this.Source = Feedback.Properties.Settings.Default;
            this.Mode = BindingMode.TwoWay;
        }

        #endregion Methods
    }

    #endregion Настройка "привязок"

    #region Процедура проверки и отправки почты

    public static class SmtpHelper
    {
        /// <summary>
        /// Процедура отправки письма
        /// </summary>

        #region Properties

        public static int Counter { get; set; } = 0;
        private static System.Net.Mail.MailMessage Mail { get; set; }

        #endregion Properties

        #region Methods

        public static void SendMail(string body, string Fsubject, string Fto, string Flogin, string Fpass, string Ffrom, string Fserver, int Fport, string fattachment1, string fattachment2, string fattachment3)
        {
            try
            {
                Mail = new System.Net.Mail.MailMessage
                {
                    From = new System.Net.Mail.MailAddress(Ffrom),
                    Subject = Fsubject,
                    IsBodyHtml = false,
                    Body = body,
                };
                Mail.To.Add(new System.Net.Mail.MailAddress(Fto));

                // Добавляем вложения к письму
                if (fattachment1 != null)
                {
                    Attachment fattach1 = new Attachment(fattachment1);
                    Mail.Attachments.Add(fattach1);
                }

                if (fattachment2 != null)
                {
                    Attachment fattach2 = new Attachment(fattachment2);
                    Mail.Attachments.Add(fattach2);
                }

                if (fattachment3 != null)
                {
                    Attachment fattach3 = new Attachment(fattachment3);
                    Mail.Attachments.Add(fattach3);
                }

                System.Net.Mail.SmtpClient client = new System.Net.Mail.SmtpClient
                {
                    Host = Fserver,
                    Port = Fport,
                    EnableSsl = true,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(Flogin, Fpass),
                    DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.Network,
                    Timeout = 5000
                };

                client.Send(Mail);
                client.Dispose();
                Mail.Dispose();

                MessageBox.Show("Сообщение отправлено! В ближайшее время операторы тех. поддержки пришлют ответ на Ваш электронный почтовый адрес.");
                // MessageBox.Show(mail.From + " _ " + mail.To + " _ " + mail.Subject + " _ " + mail.Body);
            }
            catch (Exception ee)
            {
                // Проверяем на почту гугл и ее запрет на использование сторонних приложений
                if (ee.Message.Contains("5.5.1") && (Mail.From.Host == "gmail.com"))
                {
                    Counter++;
                    if (Counter <= 2)
                    {
                        if (Counter == 1)
                        {
                            MessageBoxResult messageBoxResult = MessageBox.Show("Для отправки сообщения с помощью gmail, необходимо включить небезопасные приложения.", "Внимание!", MessageBoxButton.OK);
                            if (messageBoxResult == MessageBoxResult.OK)
                                System.Diagnostics.Process.Start("https://myaccount.google.com/lesssecureapps");
                        }
                        // Ждем, пока включат небезопасные приложения
                        System.Threading.Thread.Sleep(13000);
                        SendMail(body, Fsubject, Fto, Flogin, Fpass, Ffrom, Fserver, Fport, fattachment1, fattachment2, fattachment3);
                    }
                    else { MessageBox.Show("Не удалось отправить сообщение, проверьте настройки почты или попробуйте повторить позже."); }
                }
                else { MessageBox.Show("Не удалось отправить сообщение, проверьте настройки почты или попробуйте повторить позже."); }
                Console.WriteLine(ee.Message);
            }
        }

        /// <summary>
        /// Процедура проверки соединения
        /// </summary>
        /// <param name="smtpServerAddress">  </param>
        /// <param name="port">  </param>
        public static bool TestConnection(string smtpServerAddress, int port)
        {
            try
            {
                IPHostEntry hostEntry = Dns.GetHostEntry(smtpServerAddress);
                IPEndPoint endPoint = new IPEndPoint(hostEntry.AddressList[0], port);
                try
                {
                    using (Socket tcpSocket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp))
                    {
                        //try to connect and test the rsponse for code 220 = success
                        tcpSocket.Connect(endPoint);
                        if (!CheckResponse(tcpSocket, 220))
                        {
                            return false;
                        }

                        // send HELO and test the response for code 250 = proper response
                        SendData(tcpSocket, string.Format("HELO {0}\r\n", Dns.GetHostName()));
                        if (!CheckResponse(tcpSocket, 250))
                        {
                            return false;
                        }

                        // if we got here it's that we can connect to the smtp server
                        return true;
                    }
                }
                catch (Exception)
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static bool CheckResponse(Socket socket, int expectedCode)
        {
            while (socket.Available == 0)
            {
                System.Threading.Thread.Sleep(100);
            }
            byte[] responseArray = new byte[1024];
            socket.Receive(responseArray, 0, socket.Available, SocketFlags.None);
            string responseData = Encoding.ASCII.GetString(responseArray);
            int responseCode = Convert.ToInt32(responseData.Substring(0, 3));
            if (responseCode == expectedCode)
            {
                return true;
            }
            return false;
        }

        private static void SendData(Socket socket, string data)
        {
            byte[] dataArray = Encoding.ASCII.GetBytes(data);
            socket.Send(dataArray, 0, dataArray.Length, SocketFlags.None);
        }

        #endregion Methods
    }

    public static class ValidatorExtensions
    {
        #region Methods

        public static bool IsValidEmailAddress(this string s)
        {
            Regex regex = new Regex(@"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$");
            return regex.IsMatch(s);
        }

        #endregion Methods
    }

    // Нахождение МХ - записей
    public class Mx
    {
        #region Constructors

        public Mx()
        {
        }

        #endregion Constructors

        #region Enums

        private enum QueryOptions
        {
            DNS_QUERY_ACCEPT_TRUNCATED_RESPONSE = 1,
            DNS_QUERY_BYPASS_CACHE = 8,
            DNS_QUERY_DONT_RESET_TTL_VALUES = 0x100000,
            DNS_QUERY_NO_HOSTS_FILE = 0x40,
            DNS_QUERY_NO_LOCAL_NAME = 0x20,
            DNS_QUERY_NO_NETBT = 0x80,
            DNS_QUERY_NO_RECURSION = 4,
            DNS_QUERY_NO_WIRE_QUERY = 0x10,
            DNS_QUERY_RESERVED = -16777216,
            DNS_QUERY_RETURN_MESSAGE = 0x200,
            DNS_QUERY_STANDARD = 0,
            DNS_QUERY_TREAT_AS_FQDN = 0x1000,
            DNS_QUERY_USE_TCP_ONLY = 2,
            DNS_QUERY_WIRE_ONLY = 0x100
        }

        private enum QueryTypes
        {
            DNS_TYPE_MX = 15
        }

        #endregion Enums

        #region Methods

        public static string[] GetMXRecords(string domain)
        {
            IntPtr ptr1 = IntPtr.Zero;
            IntPtr ptr2;
            MXRecord recMx;
            if (Environment.OSVersion.Platform != PlatformID.Win32NT)
            {
                throw new NotSupportedException();
            }
            ArrayList list1 = new ArrayList();
            int num1 = Mx.DnsQuery(ref domain, QueryTypes.DNS_TYPE_MX, QueryOptions.DNS_QUERY_BYPASS_CACHE, 0, ref ptr1, 0);
            if (num1 != 0)
            {
                //throw new Win32Exception(num1);
                list1.Add("Ошибка");
            }
            for (ptr2 = ptr1; !ptr2.Equals(IntPtr.Zero); ptr2 = recMx.pNext)
            {
                recMx = (MXRecord)Marshal.PtrToStructure(ptr2, typeof(MXRecord));
                if (recMx.wType == 15)
                {
                    string text1 = Marshal.PtrToStringAuto(recMx.pNameExchange);
                    list1.Add(text1);
                }
            }
            Mx.DnsRecordListFree(ptr2, 0);
            return (string[])list1.ToArray(typeof(string));
        }

        [DllImport("dnsapi", EntryPoint = "DnsQuery_W", CharSet = CharSet.Unicode, SetLastError = true, ExactSpelling = true)]
        private static extern int DnsQuery([MarshalAs(UnmanagedType.VBByRefStr)]ref string pszName, QueryTypes wType, QueryOptions options, int aipServers, ref IntPtr ppQueryResults, int pReserved);

        [DllImport("dnsapi", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern void DnsRecordListFree(IntPtr pRecordList, int FreeType);

        #endregion Methods

        #region Structs

        [StructLayout(LayoutKind.Sequential)]
        private struct MXRecord
        {
            public IntPtr pNext;
            public string pName;
            public short wType;
            public short wDataLength;
            public int flags;
            public int dwTtl;
            public int dwReserved;
            public IntPtr pNameExchange;
            public short wPreference;
            public short Pad;
        }

        #endregion Structs
    }

    #endregion Процедура проверки и отправки почты

    #region Обводка текста

    public enum StrokePosition
    {
        Center,
        Outside,
        Inside
    }

    [ContentProperty("Text")]
    public class OutlinedTextBlock : FrameworkElement
    {
        #region Fields

        public static readonly DependencyProperty FillProperty = DependencyProperty.Register(
          "Fill",
          typeof(Brush),
          typeof(OutlinedTextBlock),
          new FrameworkPropertyMetadata(Brushes.Black, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty FontFamilyProperty = TextElement.FontFamilyProperty.AddOwner(
          typeof(OutlinedTextBlock),
          new FrameworkPropertyMetadata(OnFormattedTextUpdated));

        public static readonly DependencyProperty FontSizeProperty = TextElement.FontSizeProperty.AddOwner(
          typeof(OutlinedTextBlock),
          new FrameworkPropertyMetadata(OnFormattedTextUpdated));

        public static readonly DependencyProperty FontStretchProperty = TextElement.FontStretchProperty.AddOwner(
          typeof(OutlinedTextBlock),
          new FrameworkPropertyMetadata(OnFormattedTextUpdated));

        public static readonly DependencyProperty FontStyleProperty = TextElement.FontStyleProperty.AddOwner(
          typeof(OutlinedTextBlock),
          new FrameworkPropertyMetadata(OnFormattedTextUpdated));

        public static readonly DependencyProperty FontWeightProperty = TextElement.FontWeightProperty.AddOwner(
          typeof(OutlinedTextBlock),
          new FrameworkPropertyMetadata(OnFormattedTextUpdated));

        public static readonly DependencyProperty StrokePositionProperty =
            DependencyProperty.Register("StrokePosition",
                typeof(StrokePosition),
                typeof(OutlinedTextBlock),
                new FrameworkPropertyMetadata(StrokePosition.Outside, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty StrokeProperty = DependencyProperty.Register(
          "Stroke",
          typeof(Brush),
          typeof(OutlinedTextBlock),
          new FrameworkPropertyMetadata(Brushes.Black, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty StrokeThicknessProperty = DependencyProperty.Register(
          "StrokeThickness",
          typeof(double),
          typeof(OutlinedTextBlock),
          new FrameworkPropertyMetadata(1d, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty TextAlignmentProperty = DependencyProperty.Register(
          "TextAlignment",
          typeof(TextAlignment),
          typeof(OutlinedTextBlock),
          new FrameworkPropertyMetadata(OnFormattedTextUpdated));

        public static readonly DependencyProperty TextDecorationsProperty = DependencyProperty.Register(
          "TextDecorations",
          typeof(TextDecorationCollection),
          typeof(OutlinedTextBlock),
          new FrameworkPropertyMetadata(OnFormattedTextUpdated));

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
          "Text",
          typeof(string),
          typeof(OutlinedTextBlock),
          new FrameworkPropertyMetadata(OnFormattedTextInvalidated));

        public static readonly DependencyProperty TextTrimmingProperty = DependencyProperty.Register(
          "TextTrimming",
          typeof(TextTrimming),
          typeof(OutlinedTextBlock),
          new FrameworkPropertyMetadata(OnFormattedTextUpdated));

        public static readonly DependencyProperty TextWrappingProperty = DependencyProperty.Register(
          "TextWrapping",
          typeof(TextWrapping),
          typeof(OutlinedTextBlock),
          new FrameworkPropertyMetadata(TextWrapping.NoWrap, OnFormattedTextUpdated));

        private PathGeometry _clipGeometry;

        private FormattedText _FormattedText;

        private Pen _Pen;

        private Geometry _TextGeometry;

        #endregion Fields

        #region Constructors

        public OutlinedTextBlock()
        {
            UpdatePen();
            TextDecorations = new TextDecorationCollection();
        }

        #endregion Constructors

        #region Properties

        public Brush Fill
        {
            get { return (Brush)GetValue(FillProperty); }
            set { SetValue(FillProperty, value); }
        }

        public FontFamily FontFamily
        {
            get { return (FontFamily)GetValue(FontFamilyProperty); }
            set { SetValue(FontFamilyProperty, value); }
        }

        [TypeConverter(typeof(FontSizeConverter))]
        public double FontSize
        {
            get { return (double)GetValue(FontSizeProperty); }
            set { SetValue(FontSizeProperty, value); }
        }

        public FontStretch FontStretch
        {
            get { return (FontStretch)GetValue(FontStretchProperty); }
            set { SetValue(FontStretchProperty, value); }
        }

        public FontStyle FontStyle
        {
            get { return (FontStyle)GetValue(FontStyleProperty); }
            set { SetValue(FontStyleProperty, value); }
        }

        public FontWeight FontWeight
        {
            get { return (FontWeight)GetValue(FontWeightProperty); }
            set { SetValue(FontWeightProperty, value); }
        }

        public Brush Stroke
        {
            get { return (Brush)GetValue(StrokeProperty); }
            set { SetValue(StrokeProperty, value); }
        }

        public StrokePosition StrokePosition
        {
            get { return (StrokePosition)GetValue(StrokePositionProperty); }
            set { SetValue(StrokePositionProperty, value); }
        }

        public double StrokeThickness
        {
            get { return (double)GetValue(StrokeThicknessProperty); }
            set { SetValue(StrokeThicknessProperty, value); }
        }

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public TextAlignment TextAlignment
        {
            get { return (TextAlignment)GetValue(TextAlignmentProperty); }
            set { SetValue(TextAlignmentProperty, value); }
        }

        public TextDecorationCollection TextDecorations
        {
            get { return (TextDecorationCollection)GetValue(TextDecorationsProperty); }
            set { SetValue(TextDecorationsProperty, value); }
        }

        public TextTrimming TextTrimming
        {
            get { return (TextTrimming)GetValue(TextTrimmingProperty); }
            set { SetValue(TextTrimmingProperty, value); }
        }

        public TextWrapping TextWrapping
        {
            get { return (TextWrapping)GetValue(TextWrappingProperty); }
            set { SetValue(TextWrappingProperty, value); }
        }

        #endregion Properties

        #region Methods

        protected override Size ArrangeOverride(Size finalSize)
        {
            EnsureFormattedText();

            // update the formatted text with the final size
            _FormattedText.MaxTextWidth = finalSize.Width;
            _FormattedText.MaxTextHeight = Math.Max(0.0001d, finalSize.Height);

            // need to re-generate the geometry now that the dimensions have changed
            _TextGeometry = null;
            UpdatePen();

            return finalSize;
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            EnsureFormattedText();

            // constrain the formatted text according to the available size

            double w = availableSize.Width;
            double h = availableSize.Height;

            // the Math.Min call is important - without this constraint (which seems arbitrary, but
            // is the maximum allowable text width), things blow up when availableSize is infinite in
            // both directions the Math.Max call is to ensure we don't hit zero, which will cause
            // MaxTextHeight to throw
            _FormattedText.MaxTextWidth = Math.Min(3579139, w);
            _FormattedText.MaxTextHeight = Math.Max(0.0001d, h);

            // return the desired size
            return new Size(Math.Ceiling(_FormattedText.Width), Math.Ceiling(_FormattedText.Height));
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            EnsureGeometry();

            drawingContext.DrawGeometry(Fill, null, _TextGeometry);

            if (StrokePosition == StrokePosition.Outside)
            {
                drawingContext.PushClip(_clipGeometry);
            }
            else if (StrokePosition == StrokePosition.Inside)
            {
                drawingContext.PushClip(_TextGeometry);
            }

            drawingContext.DrawGeometry(null, _Pen, _TextGeometry);

            if (StrokePosition == StrokePosition.Outside || StrokePosition == StrokePosition.Inside)
            {
                drawingContext.Pop();
            }
        }

        private static void OnFormattedTextInvalidated(DependencyObject dependencyObject,
          DependencyPropertyChangedEventArgs e)
        {
            var outlinedTextBlock = (OutlinedTextBlock)dependencyObject;
            outlinedTextBlock._FormattedText = null;
            outlinedTextBlock._TextGeometry = null;

            outlinedTextBlock.InvalidateMeasure();
            outlinedTextBlock.InvalidateVisual();
        }

        private static void OnFormattedTextUpdated(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            var outlinedTextBlock = (OutlinedTextBlock)dependencyObject;
            outlinedTextBlock.UpdateFormattedText();
            outlinedTextBlock._TextGeometry = null;

            outlinedTextBlock.InvalidateMeasure();
            outlinedTextBlock.InvalidateVisual();
        }

        private void EnsureFormattedText()
        {
            if (_FormattedText != null)
            {
                return;
            }

            _FormattedText = new FormattedText(
              Text ?? "",
              CultureInfo.CurrentUICulture,
              FlowDirection,
              new Typeface(FontFamily, FontStyle, FontWeight, FontStretch),
              FontSize,
              Brushes.Black);

            UpdateFormattedText();
        }

        private void EnsureGeometry()
        {
            if (_TextGeometry != null)
            {
                return;
            }

            EnsureFormattedText();
            _TextGeometry = _FormattedText.BuildGeometry(new Point(0, 0));

            if (StrokePosition == StrokePosition.Outside)
            {
                var boundsGeo = new RectangleGeometry(new Rect(0, 0, ActualWidth, ActualHeight));
                _clipGeometry = Geometry.Combine(boundsGeo, _TextGeometry, GeometryCombineMode.Exclude, null);
            }
        }

        private void UpdateFormattedText()
        {
            if (_FormattedText == null)
            {
                return;
            }

            _FormattedText.MaxLineCount = TextWrapping == TextWrapping.NoWrap ? 1 : int.MaxValue;
            _FormattedText.TextAlignment = TextAlignment;
            _FormattedText.Trimming = TextTrimming;

            _FormattedText.SetFontSize(FontSize);
            _FormattedText.SetFontStyle(FontStyle);
            _FormattedText.SetFontWeight(FontWeight);
            _FormattedText.SetFontFamily(FontFamily);
            _FormattedText.SetFontStretch(FontStretch);
            _FormattedText.SetTextDecorations(TextDecorations);
        }

        private void UpdatePen()
        {
            _Pen = new Pen(Stroke, StrokeThickness)
            {
                DashCap = PenLineCap.Round,
                EndLineCap = PenLineCap.Round,
                LineJoin = PenLineJoin.Round,
                StartLineCap = PenLineCap.Round
            };

            if (StrokePosition == StrokePosition.Outside || StrokePosition == StrokePosition.Inside)
            {
                _Pen.Thickness = StrokeThickness * 2;
            }

            InvalidateVisual();
        }

        #endregion Methods
    }

    #endregion Обводка текста
}