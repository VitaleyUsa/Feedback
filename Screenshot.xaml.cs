using System;
using System.Windows;

namespace Feedback
{
    /// <summary>
    /// Interaction logic for Screenshot.xaml
    /// </summary>
    public partial class Screenshot : Window
    {
        public readonly System.Windows.Threading.DispatcherTimer timer = new System.Windows.Threading.DispatcherTimer();
        public readonly System.Windows.Threading.DispatcherTimer timer2 = new System.Windows.Threading.DispatcherTimer();

        public Screenshot()
        {
            InitializeComponent();
            var desktopWorkingArea = System.Windows.SystemParameters.WorkArea;
            this.Left = (desktopWorkingArea.Right - this.Width) / 2;
            this.Top = 0;

            timer.Tick += new EventHandler(TimerTick);
            timer.Interval = new TimeSpan(0, 0, 2);

            timer2.Tick += new EventHandler(TimerTick2);
            timer2.Interval = new TimeSpan(0, 0, 1);
        }

        private void ButtonInnerScreenshot_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            // Добавляем скриншоты
            MainWindow mainWindow = Application.Current.MainWindow as MainWindow;
            string fname = mainWindow.TakeScreenShot();

            if (mainWindow.FileAttachment_1 == null)
                mainWindow.FileAttachment_1 = fname;
            else if (mainWindow.FileAttachment_2 == null)
                mainWindow.FileAttachment_2 = fname;
            else if (mainWindow.FileAttachment_3 == null)
                mainWindow.FileAttachment_3 = fname;
            else
                mainWindow.FileAttachment_1 = fname;

            // Выводим окно с сообщением об успешном выполнении

            timer2.Start();
            timer.Start();
        }

        private void TimerTick(object sender, EventArgs e)
        {
            popup.IsOpen = false;
            this.Close();
            App.Current.MainWindow.Visibility = Visibility.Visible;
            timer.Stop();
        }

        private void TimerTick2(object sender, EventArgs e)
        {
            popup.VerticalOffset = 0;
            popup.HorizontalOffset = (SystemParameters.PrimaryScreenWidth - this.Width - 300) / 2;
            popup.IsOpen = true;
            timer2.Stop();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (this.Visibility != Visibility.Collapsed)
            {
                App.Current.MainWindow.Visibility = Visibility.Visible;
                this.Close();
            }
        }
    }
}