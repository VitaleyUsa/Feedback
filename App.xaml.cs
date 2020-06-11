using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Feedback
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public const int HT_CAPTION = 0x2;
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public StackPanel Category_global_active { get; internal set; }

        public TextBlock Contact_active { get; internal set; }

        public string Mailto { get; set; }

        public static T FindChild<T>(DependencyObject parent, string childName) where T : DependencyObject
        {
            // Confirm parent and childName are valid.
            if (parent == null) return null;
            T foundChild = null;
            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                // If the child is not of the request child type child
                if (!(child is T))
                {
                    // recursively drill down the tree
                    foundChild = FindChild<T>(child, childName);
                    // If the child is found, break so we do not overwrite the found child.
                    if (foundChild != null) break;
                }
                else if (!string.IsNullOrEmpty(childName))
                {
                    // If the child's name is set for search
                    if (child is FrameworkElement frameworkElement && frameworkElement.Name == childName)
                    {
                        // if the child's name is of the request name
                        foundChild = (T)child;
                        break;
                    }
                }
                else
                {
                    // child element found.
                    foundChild = (T)child;
                    break;
                }
            }
            return foundChild;
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        private void Caption_MouseDown(object sender, MouseButtonEventArgs e)
        {
            TextBlock thisTextBlock = (TextBlock)sender;
            TextBlock phoneTextBlock = FindChild<TextBlock>(thisTextBlock, "");
            Run header = thisTextBlock.Inlines.FirstInline.NextInline as Run;
            StackPanel Category_global_prev = Category_global_active;
            MainWindow Xaml = MainWindow as MainWindow;

            if (header.Text.Contains("Росреестр"))
            {
                Xaml.close_min.IsOpen = true;
                Xaml.webBrowser1.Navigate("https://rosreestr.ru/wps/portal/cc_faq_query");
                Xaml.Header_configform.Visibility = Visibility.Collapsed;
                Xaml.Header_mailform.Visibility = Visibility.Collapsed;
                Xaml.MailForm.Visibility = Visibility.Collapsed;
                Xaml.ConfigForm.Visibility = Visibility.Collapsed;
                Xaml.RosreestrURL.Visibility = Visibility.Visible;
            }
            else
            {
                Category_global_prev.Visibility = Visibility.Collapsed;

                Xaml.close_min.IsOpen = false;
                Xaml.Header_configform.Visibility = Visibility.Collapsed;
                Xaml.Header_mailform.Visibility = Visibility.Visible;
                Xaml.MailForm.Visibility = Visibility.Visible;
                Xaml.ConfigForm.Visibility = Visibility.Collapsed;
                Xaml.RosreestrURL.Visibility = Visibility.Collapsed;

                if (header.Text.Contains("ФЦИИТ"))
                    Category_global_active = Xaml.Category_Fond_global;
                else if (header.Text.Contains("Триасофт"))
                    Category_global_active = Xaml.Category_Triasoft_global;
                else if (header.Text.Contains("Табеллион"))
                    Category_global_active = Xaml.Category_Tabellion_global;
                else if (header.Text.Contains("Нотариат"))
                    Category_global_active = Xaml.Category_Notariat_global;
                else if (header.Text.Contains("Региональная"))
                    Category_global_active = Xaml.Category_Palata_global;

                Category_global_active.Visibility = Visibility.Visible;
            }

            TextBlock mailTextBlock = FindChild<TextBlock>(phoneTextBlock, "");
            if (mailTextBlock != null)
                Mailto = mailTextBlock.Text;
            else
                Mailto = "Rosreestr";

            Image img_mail = FindChild<Image>(thisTextBlock, "");
            img_mail.RenderTransform = new TranslateTransform(0, 2.6); ;
            img_mail.Source = new BitmapImage(new Uri("pack://application:,,,/Feedback;component/Resources/tick.png"));

            if (Contact_active != null && Contact_active != thisTextBlock)
            {
                Contact_active.Background = Brushes.Transparent;

                Image img_mail_prev = FindChild<Image>(Contact_active, "");
                img_mail_prev.RenderTransform = new TranslateTransform(0, 5);
                img_mail_prev.Source = new BitmapImage(new Uri("pack://application:,,,/Feedback;component/Resources/email.png"));
            }

            Contact_active = thisTextBlock;
        }

        private void Caption_MouseEnter(object sender, MouseEventArgs e)
        {
            TextBlock thisTextBlock = (TextBlock)sender;

            SolidColorBrush whiteBrush = new SolidColorBrush(Colors.White)
            {
                Opacity = 0.5
            };
            thisTextBlock.Background = whiteBrush;

            Image img_mail = FindChild<Image>(thisTextBlock, "");
            img_mail.RenderTransform = new TranslateTransform(0, 2.6);
            img_mail.Source = new BitmapImage(new Uri("pack://application:,,,/Feedback;component/Resources/tick.png"));
        }

        private void Caption_MouseLeave(object sender, MouseEventArgs e)
        {
            TextBlock thisTextBlock = (TextBlock)sender;
            if (thisTextBlock != Contact_active)
            {
                thisTextBlock.Background = Brushes.Transparent;
                Image img_mail = FindChild<Image>(thisTextBlock, "");
                img_mail.RenderTransform = new TranslateTransform(0, 5);
                img_mail.Source = new BitmapImage(new Uri("pack://application:,,,/Feedback;component/Resources/email.png"));
            }
        }

        private void EatEventsHandler(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        private void Image_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            IntPtr windowHandle = new WindowInteropHelper(Application.Current.MainWindow).Handle;
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                ReleaseCapture();
                SendMessage(windowHandle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        private void Image_MouseDown_off(object sender, MouseEventArgs e)
        {
            System.Diagnostics.Process.Start("https://rosreestr.ru/wps/portal/cc_faq_query");
        }
    }
}