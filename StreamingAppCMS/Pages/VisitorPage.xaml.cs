using StreamingAppCMS.Helpers;
using StreamingAppCMS.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Navigation;

namespace StreamingAppCMS.Pages
{
    /// <summary>
    /// Interaction logic for VisitorPage.xaml
    /// </summary>
    public partial class VisitorPage : Page
    {
        public VisitorPage()
        {
            InitializeComponent();
            LoadStreamingApps();
        }

        private void LoadStreamingApps()
        {
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "streamingApps.xml");
            List<StreamingApp> apps = StreamingAppDataStorage.LoadApps(filePath);
            StreamingAppDataGrid.ItemsSource = apps;
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("Pages/LoginPage.xaml", UriKind.Relative));
        }

        private void NameHyperlink_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Hyperlink hyperlink && hyperlink.Tag is StreamingApp selectedApp)
            {
                var detailsPage = new StreamingAppDescriptionPage(selectedApp);
                this.NavigationService.Navigate(detailsPage);
            }
        }
    }
}
