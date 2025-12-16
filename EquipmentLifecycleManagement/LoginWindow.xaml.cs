using System;
using System.Linq;
using System.Windows;
using EquipmentLifecycleManager.Data;

namespace EquipmentLifecycleManager
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
            cmbRole.SelectedIndex = 0;
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Password;

            MessageBox.Show($"Пытаюсь войти: {username}"); // Тест

            if (username == "1" && password == "1")
            {
                CurrentUser.User = new EquipmentLifecycleManager.Data.User
                {
                    Username = "1",
                    FullName = "Администратор",
                    Role = "Admin",
                    IsActive = true
                };
                var mainWindow = new MainWindow();
                mainWindow.Show();

                this.Close();
                return;
            }

            var user = CheckUser(username, password);
            if (user != null)
            {
                CurrentUser.User = user;

                var mainWindow = new MainWindow();
                mainWindow.Show();

                this.Close();
            }
            else
            {
                ShowError("Неверный логин или пароль");
            }
        }
        private EquipmentLifecycleManager.Data.User CheckUser(string username, string password) // ИЗМЕНИТЕ
        {
            var users = new[]
            {
                new { Username = "admin", Password = "admin123", Role = "Admin", FullName = "Администратор" },
                new { Username = "engineer", Password = "engineer123", Role = "Engineer", FullName = "Инженер" },
                new { Username = "technician", Password = "tech123", Role = "Technician", FullName = "Техник" },
                new { Username = "manager", Password = "manager123", Role = "Manager", FullName = "Менеджер" }
            };

            var foundUser = users.FirstOrDefault(u =>
                u.Username == username && u.Password == password);

            if (foundUser != null)
            {
                return new EquipmentLifecycleManager.Data.User // ИЗМЕНИТЕ
                {
                    Username = foundUser.Username,
                    FullName = foundUser.FullName,
                    Role = foundUser.Role,
                    IsActive = true
                };
            }

            return null;
        }

        private void BtnExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void ShowError(string message)
        {
            txtErrorMessage.Text = message;
            txtErrorMessage.Visibility = Visibility.Visible;
        }

        private void TxtUsername_GotFocus(object sender, RoutedEventArgs e)
        {
            txtErrorMessage.Visibility = Visibility.Collapsed;
        }

        private void TxtPassword_GotFocus(object sender, RoutedEventArgs e)
        {
            txtErrorMessage.Visibility = Visibility.Collapsed;
        }
    }



    // Статический класс для хранения текущего пользователя
    public static class CurrentUser
    {
        public static EquipmentLifecycleManager.Data.User User { get; set; } // ИЗМЕНИТЕ
    }
}