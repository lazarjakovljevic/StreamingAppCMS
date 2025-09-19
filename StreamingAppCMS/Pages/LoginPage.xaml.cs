using StreamingAppCMS.Helpers;
using StreamingAppCMS.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Notification.Wpf;


namespace StreamingAppCMS.Pages
{
    public partial class LoginPage : Page
    {
        MainWindow mainWindow;
        string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "users.xml");
        private List<User> usersList { get; set; }
        
        public LoginPage()
        {
            InitializeComponent();
            mainWindow = (MainWindow)Application.Current.MainWindow;

            LoadUsers();

            txtUsername.KeyDown += Input_KeyDown;
            txtPassword.KeyDown += Input_KeyDown;

            usersList = UserDataStorage.LoadUsers();
        }

        private void LoadUsers()
        {
            UserDataStorage.EnsureDataExists(filePath);
        }

        private void Input_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                btnLogin_Click(btnLogin, new RoutedEventArgs());
            }
        }

        private void ValidateFormData(object sender, RoutedEventArgs e)
        {
            txtUsernameError.Text = "";
            txtPasswordError.Text = "";
            txtUsername.BorderBrush = Brushes.Gray;
            txtPassword.BorderBrush = Brushes.Gray;

            string username = txtUsername.Text.Trim();
            string password = txtPassword.Password;

            bool isValid = true;

            if (string.IsNullOrEmpty(username))
            {
                txtUsernameError.Text = "Username is required.";
                txtUsername.BorderBrush = Brushes.Red;
                isValid = false;
            }

            if (string.IsNullOrEmpty(password))
            {
                txtPasswordError.Text = "Password is required.";
                txtPassword.BorderBrush = Brushes.Red;
                isValid = false;
            }

            if (!isValid)
                return;

            User user = usersList.Find(x => x.Username == username);

            if (user == null)
            {
                txtUsernameError.Text = "Username does not exist.";
                txtUsername.BorderBrush = Brushes.Red;
                txtUsername.Focus();
                return;
            }

            if (user.Password != password)
            {
                txtPasswordError.Text = "Incorrect password.";
                txtPassword.BorderBrush = Brushes.Red;
                txtPassword.Clear();
                return;
            }

            if (user.Role == UserRole.Admin)
            {
                this.NavigationService.Navigate(new Uri("Pages/AdminPage.xaml", UriKind.Relative));
            }
            else if (user.Role == UserRole.Visitor)
            {
                this.NavigationService.Navigate(new Uri("Pages/VisitorPage.xaml", UriKind.Relative));
            }

            ShowWelcomeToastNotification(sender, e);
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            ValidateFormData(sender, e);
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void ShowWelcomeToastNotification(object sender, RoutedEventArgs e)
        {
            string message = $"Welcome, {txtUsername.Text}!";

            mainWindow.ShowToastNotification(new ToastNotification("Login Successful", message, NotificationType.Success));
        }
    }
}