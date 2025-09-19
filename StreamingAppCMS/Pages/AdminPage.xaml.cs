using Notification.Wpf;
using StreamingAppCMS.Helpers;
using StreamingAppCMS.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace StreamingAppCMS.Pages
{
    /// <summary>
    /// Interaction logic for AdminPage.xaml
    /// </summary>
    public partial class AdminPage : Page
    {
        private HashSet<StreamingApp> selectedApps;
        private ObservableCollection<StreamingApp> streamingApps { get; set; }
        MainWindow mainWindow;
        public AdminPage()
        {
            InitializeComponent();
            LoadStreamingApps();

            mainWindow = (MainWindow)Application.Current.MainWindow;
        }

        private void LoadStreamingApps()
        {
            string filePath = "streamingApps.xml";
            List<StreamingApp> appsList = StreamingAppDataStorage.LoadApps(filePath);
            streamingApps = new ObservableCollection<StreamingApp>(appsList);
            selectedApps = new HashSet<StreamingApp>();

            StreamingAppDataGrid.ItemsSource = streamingApps;
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("Pages/LoginPage.xaml", UriKind.Relative));
        }

        private void IndividualCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox && checkBox.Tag is StreamingApp app)
            {
                selectedApps.Add(app);
            }
        }

        private void IndividualCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox && checkBox.Tag is StreamingApp app)
            {
                selectedApps.Remove(app);
            }
        }

        private void SelectAll_Checked(object sender, RoutedEventArgs e)
        {
            selectedApps.Clear();
            foreach (var app in streamingApps)
            {
                selectedApps.Add(app);
            }

            SetAllIndividualCheckboxes(true);
        }

        private void SelectAll_Unchecked(object sender, RoutedEventArgs e)
        {
            selectedApps.Clear();

            SetAllIndividualCheckboxes(false);
        }

        private void SetAllIndividualCheckboxes(bool isChecked)
        {

            for (int i = 0; i < StreamingAppDataGrid.Items.Count; i++)
            {
                DataGridRow row = (DataGridRow)StreamingAppDataGrid.ItemContainerGenerator.ContainerFromIndex(i);
                if (row != null)
                {
                    CheckBox checkBox = FindVisualChild<CheckBox>(row);
                    if (checkBox != null)
                    {
                        checkBox.IsChecked = isChecked;
                    }
                }
            }
        }

        private static T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child != null && child is T)
                    return (T)child;

                var childOfChild = FindVisualChild<T>(child);
                if (childOfChild != null)
                    return childOfChild;
            }
            return null;
        }

        private void DeleteSelected_Click(object sender, RoutedEventArgs e)
        {
            if (streamingApps.Count == 0)
            {
                mainWindow.ShowToastNotification(new ToastNotification("Error", "Cannot delete from an empty table!", NotificationType.Error));
                return;
            }

            if (selectedApps.Count == 0)
            {
                mainWindow.ShowToastNotification(new ToastNotification("Error", "Please select item(s) first!", NotificationType.Error));
                return;
            }

            List<StreamingApp> appsToDelete = new List<StreamingApp>(selectedApps);
            foreach (var app in appsToDelete)
            {
                streamingApps.Remove(app);
            }

            selectedApps.Clear();
            if (chkSelectAll != null)
            {
                chkSelectAll.IsChecked = false;
            }

            mainWindow.ShowToastNotification(new ToastNotification("Success", $"{appsToDelete.Count} application(s) successfully removed!", NotificationType.Success));
            StreamingAppDataStorage.SaveApps(streamingApps.ToList());
        }

        private void AddNewApp_Click(object sender, RoutedEventArgs e)
        {
            var addNewAppPage = new AddNewAppPage(streamingApps, null);
            this.NavigationService.Navigate(addNewAppPage);
        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Hyperlink hyperlink && hyperlink.Tag is StreamingApp selectedApp)
            {
                var detailsPage = new AddNewAppPage(streamingApps, selectedApp);
                this.NavigationService.Navigate(detailsPage);
            }

            StreamingAppDataGrid.Items.Refresh();
        }
    }
}
