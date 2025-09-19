using Notification.Wpf;
using StreamingAppCMS.Helpers;
using StreamingAppCMS.Models;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace StreamingAppCMS.Pages
{
    /// <summary>
    /// Interaction logic for StreamingAppDescriptionPage.xaml
    /// </summary>
    public partial class StreamingAppDescriptionPage : Page
    {
        MainWindow mainWindow;

        public StreamingAppDescriptionPage(StreamingApp streamingApp)
        {
            DataContext = streamingApp;
            mainWindow = (MainWindow)Application.Current.MainWindow;

            InitializeComponent();
            LoadRtfContent(streamingApp.DescriptionPath);
        }

        private void LoadRtfContent(string rtfPath)
        {
            try
            {
                string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, rtfPath);

                if (File.Exists(fullPath))
                {
                    string rtfContent = File.ReadAllText(fullPath);

                    TextRange textRange = new TextRange(richTextBoxDescription.Document.ContentStart, richTextBoxDescription.Document.ContentEnd);
                    using (MemoryStream ms = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(rtfContent)))
                    {
                        textRange.Load(ms, DataFormats.Rtf);
                    }
                }
                else
                {
                    string message = $"File not found";
                    mainWindow.ShowToastNotification(new ToastNotification("Erorr", message, NotificationType.Error));
                }
            }
            catch (Exception ex)
            {
                string message = $"Error loading RTF file: {ex.Message}";
                mainWindow.ShowToastNotification(new ToastNotification("Erorr", message, NotificationType.Error));
            }
        }

        private void btnPageBack_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
        }
    }
}
