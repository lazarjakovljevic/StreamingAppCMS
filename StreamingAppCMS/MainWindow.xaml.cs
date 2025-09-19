using Notification.Wpf;
using StreamingAppCMS.Helpers;
using System.Windows;
using System.Windows.Input;

namespace StreamingAppCMS
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private NotificationManager notificationManager;
        public MainWindow()
        {
            InitializeComponent();
            LoadStreamingApps();

            notificationManager = new NotificationManager();
        }

        private void LoadStreamingApps()
        {
            StreamingAppDataStorage.EnsureDataExists();
        }

        private void Toolbar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        public void ShowToastNotification(ToastNotification toastNotification)
        {
            notificationManager.Show(toastNotification.Title, toastNotification.Message, toastNotification.Type, "WindowNotificationArea");
        }
    }
}
