using EquipmentLifecycleManager.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace EquipmentLifecycleManager
{
    public partial class LoginWindow : Window
    {
        private AppDbContext dbContext;

        public LoginWindow()
        {
            InitializeComponent();
            cmbRole.SelectedIndex = 0; // Выбираем первую роль по умолчанию

            // Инициализируем БД и добавляем тестовых пользователей
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            try
            {
                dbContext = new AppDbContext();
                dbContext.Database.EnsureCreated();

                // Добавляем тестовых пользователей если их нет
                if (!dbContext.Users.Any())
                {
                    var testUsers = new[]
                    {
                        new User { Username = "admin", Password = "admin123", Role = "Admin", FullName = "Администратор системы" },
                        new User { Username = "engineer", Password = "engineer123", Role = "Engineer", FullName = "Инженер Иванов И.И." },
                        new User { Username = "technician", Password = "tech123", Role = "Technician", FullName = "Техник Петров П.П." },
                        new User { Username = "manager", Password = "manager123", Role = "Manager", FullName = "Менеджер Сидоров С.С." }
                    };

                    dbContext.Users.AddRange(testUsers);
                    dbContext.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка инициализации БД: {ex.Message}\nИспользуется демо-режим.",
                              "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Password;
            string selectedRole = (cmbRole.SelectedItem as ComboBoxItem)?.Content.ToString();

            // Валидация
            if (string.IsNullOrWhiteSpace(username))
            {
                ShowError("Введите логин");
                txtUsername.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                ShowError("Введите пароль");
                txtPassword.Focus();
                return;
            }

            try
            {
                User user = null;


                if (dbContext?.Database.CanConnect() == true)
                {
                    user = dbContext.Users
                        .FirstOrDefault(u => u.Username == username &&
                                           u.Password == password &&
                                           u.IsActive);
                }

                if (user == null)
                {
                    // Демо-пользователи (для тестирования без БД)
                    var demoUsers = new[]
                    {
                        new { Username = "admin", Password = "admin123", Role = "Admin", FullName = "Администратор" },
                        new { Username = "engineer", Password = "engineer123", Role = "Engineer", FullName = "Инженер" }, 
                        new { Username = "technician", Password = "tech123", Role = "Technician", FullName = "Техник" },
                        new { Username = "manager", Password = "manager123", Role = "Manager", FullName = "Менеджер" }
                    };

                    var demoUser = demoUsers.FirstOrDefault(u =>
                        u.Username == username && u.Password == password);

                    if (demoUser != null)
                    {
                        user = new User
                        {
                            Username = demoUser.Username,
                            Role = demoUser.Role,
                            FullName = demoUser.FullName
                        };
                    }
                }

                if (user != null)
                {
                    // Проверяем соответствие роли (если выбрана конкретная роль)
                    if (selectedRole != null)
                    {
                        string mappedRole;
                            switch (selectedRole)
                        {
                            case "Администратор":
                                mappedRole = "Admin";
                                break;
                            case "Инженер":
                                mappedRole = "Engineer";
                                break;
                            case "Техник":
                                mappedRole = "Technician";
                                break;
                            case "Менеджер":
                                mappedRole = "Manager";
                                break;
                            default:
                                mappedRole = selectedRole;
                                break;
                        }

                        if (user.Role != mappedRole)
                        {
                            ShowError($"У пользователя роль: {GetRoleDisplayName(user.Role)}\nВы выбрали: {selectedRole}");
                            return;
                        }
                    }

                    // Авторизация успешна
                    CurrentUser.User = user;
                    DialogResult = true;
                    Close();
                }
                else
                {
                    ShowError("Неверный логин или пароль");
                    txtPassword.Password = "";
                    txtPassword.Focus();
                }
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка авторизации: {ex.Message}");
            }
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

        private string GetRoleDisplayName(string role)
        {
            switch (role)
            {
                case "Admin":
                    return "Администратор";
                case "Engineer":
                    return "Инженер";
                case "Technician":
                    return "Техник";
                case "Manager":
                    return "Менеджер";
                default:
                    return role;
            }
            
        }
    }

    // Статический класс для хранения текущего пользователя
    public static class CurrentUser
    {
        public static User User { get; set; }

        public static bool IsAdmin => User?.Role == "Admin";
        public static bool IsEngineer => User?.Role == "Engineer";
        public static bool IsTechnician => User?.Role == "Technician";
        public static bool IsManager => User?.Role == "Manager";

        public static string DisplayName => User?.FullName ?? User?.Username ?? "Гость";
    }
}