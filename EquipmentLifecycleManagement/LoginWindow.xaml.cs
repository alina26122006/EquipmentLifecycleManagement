using System;
using System.Linq;
using System.Windows;

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

            // Простая проверка
            if (username == "1" && password == "1")
            {
                CurrentUser.User = new User
                {
                    Username = "1",
                    FullName = "Администратор",
                    Role = "Admin"
                };
                DialogResult = true;
                Close();
                return;
            }

            // Проверка других пользователей
            var user = CheckUser(username, password);
            if (user != null)
            {
                CurrentUser.User = user;
                DialogResult = true;
                Close();
            }
            else
            {
                ShowError("Неверный логин или пароль");
            }
        }

        private User CheckUser(string username, string password)
        {
            // Стандартные пользователи
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
                return new User
                {
                    Username = foundUser.Username,
                    FullName = foundUser.FullName,
                    Role = foundUser.Role
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

    // Класс User (если нет в Data)
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
        public string FullName { get; set; }
        public bool IsActive { get; set; } = true;
    }

    // Статический класс для хранения текущего пользователя
    public static class CurrentUser
    {
        public static User User { get; set; }
    }
}