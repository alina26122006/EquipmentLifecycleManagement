using EquipmentLifecycleManager.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.EntityFrameworkCore;

namespace EquipmentLifecycleManager
{
    public partial class MainWindow : Window
    {
        private List<Data.Equipment> _equipmentList;
        private List<Data.Maintenance> _maintenanceList;
        private List<Data.Equipment> _filteredEquipmentList;
        private List<Data.Department> _departments;
        private AppDbContext _dbContext;
        private int _selectedDepartmentId = 0; // 0 = все отделения

        public MainWindow()
        {
            InitializeComponent();

            // Проверяем и инициализируем пользователя
            InitializeUser();

            _equipmentList = new List<Data.Equipment>();
            _maintenanceList = new List<Data.Maintenance>();
            _filteredEquipmentList = new List<Data.Equipment>();
            _departments = new List<Data.Department>();

            SetupDatabase();
            LoadAllData();
            LoadDepartments();
            UpdateStatistics();

            // Инициализируем отображение техобслуживания
            UpdateMaintenanceDisplay();
        }

        // Инициализация пользователя
        private void InitializeUser()
        {
            if (CurrentUser.User != null)
            {
                txtCurrentUser.Text = $"Пользователь: {CurrentUser.User.FullName} ({GetRoleDisplayName(CurrentUser.User.Role)})";
                SetupRoleBasedAccess();
            }
            else
            {
                // Создаем тестового пользователя для разработки
                CurrentUser.User = new Data.User
                {
                    Username = "admin",
                    FullName = "Администратор (тестовый)",
                    Role = "Admin"
                };
                txtCurrentUser.Text = $"Пользователь: {CurrentUser.User.FullName} ({GetRoleDisplayName(CurrentUser.User.Role)})";
                SetupRoleBasedAccess();
            }
        }

        // Настройка базы данных
        private void SetupDatabase()
        {
            try
            {
                _dbContext = new AppDbContext();

                // Проверяем подключение к БД
                bool canConnect = _dbContext.Database.CanConnect();

                if (canConnect)
                {
                    // Создаем таблицы если их нет
                    _dbContext.Database.EnsureCreated();
                    txtStatus.Text = "База данных подключена успешно";

                    // Добавляем тестовые данные если таблицы пустые
                    if (!_dbContext.Equipment.Any())
                    {
                        AddTestDataToDatabase();
                    }

                    if (!_dbContext.Users.Any())
                    {
                        AddTestUsersToDatabase();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка подключения к БД: {ex.Message}\n\nПриложение будет работать с данными в памяти.",
                              "Ошибка БД",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                txtStatus.Text = "⚠️ Режим работы: данные в памяти (БД не доступна)";
            }
        }

        // Метод для настройки прав доступа по роли
        private void SetupRoleBasedAccess()
        {
            string role = CurrentUser.User.Role;

            if (role == "Technician")
            {
                btnAddEquipment.IsEnabled = false;
                btnEditEquipment.IsEnabled = false;
                btnDeleteEquipment.IsEnabled = false;
                btnAddMaintenance.IsEnabled = false;
                // Техник может только выполнять ТО
            }
            else if (role == "Manager")
            {
                btnAddEquipment.IsEnabled = false;
                btnEditEquipment.IsEnabled = false;
                btnDeleteEquipment.IsEnabled = false;
                btnAddMaintenance.IsEnabled = false;
                btnCompleteMaintenance.IsEnabled = false;
                // Менеджер может только смотреть отчеты
            }
            // Администратор и Инженер - полный доступ
        }

        // Метод для перевода названия роли
        private string GetRoleDisplayName(string role)
        {
            switch (role)
            {
                case "Admin": return "Администратор";
                case "Engineer": return "Инженер";
                case "Technician": return "Техник";
                case "Manager": return "Менеджер";
                default: return role;
            }
        }

        // Метод для загрузки всех данных
        private void LoadAllData()
        {
            try
            {
                if (_dbContext?.Database.CanConnect() == true)
                {
                    // Загружаем оборудование с связанными отделениями
                    _equipmentList = _dbContext.Equipment
                        .Include(e => e.Maintenances)
                        .Include(e => e.Department) // Загружаем связанное отделение
                        .ToList();

                    _maintenanceList = _dbContext.Maintenance.ToList();
                    txtStatus.Text = "Данные загружены из базы данных";
                }
                else
                {
                    // Если БД не доступна, используем данные в памяти
                    UseInMemoryData();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки из БД: {ex.Message}\nИспользуются данные в памяти.",
                              "Внимание", MessageBoxButton.OK, MessageBoxImage.Information);
                UseInMemoryData();
            }

            ApplyFilters(); // Используем новый метод для применения фильтров
            UpdateMaintenanceDisplay(); // Обновляем отображение техобслуживания
        }

        // Метод для обновления отображения техобслуживания
        private void UpdateMaintenanceDisplay()
        {
            if (_maintenanceList == null) return;

            // Обновляем ListBox
            lbMaintenance.ItemsSource = null;
            lbMaintenance.ItemsSource = _maintenanceList;

            // Обновляем счетчик предстоящих обслуживаний
            var upcomingCount = _maintenanceList.Count(m => m.Status != "Выполнено");
            txtUpcomingCount.Text = upcomingCount.ToString();
        }

        private void AddTestUsersToDatabase()
        {
            var testUsers = new List<Data.User>
            {
                new Data.User { Username = "admin", Password = "admin123", Role = "Admin", FullName = "Администратор системы", IsActive = true },
                new Data.User { Username = "engineer", Password = "engineer123", Role = "Engineer", FullName = "Инженер Иванов И.И.", IsActive = true },
                new Data.User { Username = "technician", Password = "tech123", Role = "Technician", FullName = "Техник Петров П.П.", IsActive = true },
                new Data.User { Username = "manager", Password = "manager123", Role = "Manager", FullName = "Менеджер Сидоров С.С.", IsActive = true }
            };

            _dbContext.Users.AddRange(testUsers);
            _dbContext.SaveChanges();
        }

        private void AddTestDataToDatabase()
        {
            // Сначала создаем отделения
            var departments = new List<Data.Department>
            {
                new Data.Department { Name = "Информационные технологии", Code = "ИТ", Description = "IT отдел" },
                new Data.Department { Name = "Бухгалтерия", Code = "БУХ", Description = "Бухгалтерский отдел" },
                new Data.Department { Name = "Производство", Code = "ПРОИЗВ", Description = "Производственный отдел" },
                new Data.Department { Name = "Склад", Code = "СКЛАД", Description = "Складское хозяйство" }
            };

            _dbContext.Departments.AddRange(departments);
            _dbContext.SaveChanges();

            // Получаем созданные отделения с ID
            var createdDepartments = _dbContext.Departments.ToList();

            var testEquipment = new List<Data.Equipment>
            {
                new Data.Equipment { Name = "Токарный станок CNC-500", InventoryNumber = "INV001",
                    Model = "CNC-500", Status = "В работе", CommissionDate = new DateTime(2023, 1, 15),
                    DepartmentId = createdDepartments.First(d => d.Code == "ПРОИЗВ").Id },
                new Data.Equipment { Name = "Фрезерный станок FZ-200", InventoryNumber = "INV002",
                    Model = "FZ-200", Status = "На обслуживании", CommissionDate = new DateTime(2023, 3, 20),
                    DepartmentId = createdDepartments.First(d => d.Code == "ПРОИЗВ").Id },
                new Data.Equipment { Name = "Пресс гидравлический PH-100", InventoryNumber = "INV003",
                    Model = "PH-100", Status = "В работе", CommissionDate = new DateTime(2023, 5, 10),
                    DepartmentId = createdDepartments.First(d => d.Code == "ПРОИЗВ").Id },
                new Data.Equipment { Name = "Сверлильный станок SD-50", InventoryNumber = "INV004",
                    Model = "SD-50", Status = "Списан", CommissionDate = new DateTime(2022, 8, 1),
                    DepartmentId = createdDepartments.First(d => d.Code == "ПРОИЗВ").Id },
                new Data.Equipment { Name = "Компьютер Dell", InventoryNumber = "INV005",
                    Model = "Optiplex 7070", Status = "В работе", CommissionDate = new DateTime(2023, 2, 10),
                    DepartmentId = createdDepartments.First(d => d.Code == "ИТ").Id }
            };

            _dbContext.Equipment.AddRange(testEquipment);
            _dbContext.SaveChanges();

            // Создаем обслуживание для оборудования
            var equipmentList = _dbContext.Equipment.ToList();
            var testMaintenance = new List<Data.Maintenance>
            {
                new Data.Maintenance { EquipmentName = "Токарный станок CNC-500", EquipmentId = equipmentList[0].Id,
                    PlannedDate = DateTime.Now.AddDays(7), Status = "Запланировано" },
                new Data.Maintenance { EquipmentName = "Фрезерный станок FZ-200", EquipmentId = equipmentList[1].Id,
                    PlannedDate = DateTime.Now.AddDays(2), Status = "В работе" }
            };

            _dbContext.Maintenance.AddRange(testMaintenance);
            _dbContext.SaveChanges();
        }

        // Загружаем список отделений
        private void LoadDepartments()
        {
            try
            {
                if (_dbContext?.Database.CanConnect() == true)
                {
                    _departments = _dbContext.Departments
                        .Where(d => d.IsActive)
                        .OrderBy(d => d.Name)
                        .ToList();
                }
                else
                {
                    // Данные в памяти
                    _departments = GetDefaultDepartments();
                }

                // Заполняем ComboBox для фильтрации
                cmbDepartmentFilter.Items.Clear();
                cmbDepartmentFilter.Items.Add(new ComboBoxItem { Content = "Все отделения", Tag = "0" });

                foreach (var department in _departments)
                {
                    cmbDepartmentFilter.Items.Add(new ComboBoxItem
                    {
                        Content = department.Name,
                        Tag = department.Id.ToString()
                    });
                }

                cmbDepartmentFilter.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки отделений: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private List<Data.Department> GetDefaultDepartments()
        {
            return new List<Data.Department>
            {
                new Data.Department { Id = 1, Name = "Информационные технологии", Code = "ИТ", IsActive = true },
                new Data.Department { Id = 2, Name = "Бухгалтерия", Code = "БУХ", IsActive = true },
                new Data.Department { Id = 3, Name = "Производство", Code = "ПРОИЗВ", IsActive = true },
                new Data.Department { Id = 4, Name = "Склад", Code = "СКЛАД", IsActive = true },
                new Data.Department { Id = 5, Name = "Отдел кадров", Code = "ОК", IsActive = true },
                new Data.Department { Id = 6, Name = "Администрация", Code = "АДМ", IsActive = true }
            };
        }

        private void UseInMemoryData()
        {
            // Сначала создаем отделения
            var departments = GetDefaultDepartments();

            _equipmentList = new List<Data.Equipment>
            {
                new Data.Equipment { Id = 1, Name = "Токарный станок CNC-500", InventoryNumber = "INV001",
                    Model = "CNC-500", Status = "В работе", CommissionDate = new DateTime(2023, 1, 15),
                    DepartmentId = 3, Department = departments.First(d => d.Id == 3) },
                new Data.Equipment { Id = 2, Name = "Фрезерный станок FZ-200", InventoryNumber = "INV002",
                    Model = "FZ-200", Status = "На обслуживании", CommissionDate = new DateTime(2023, 3, 20),
                    DepartmentId = 3, Department = departments.First(d => d.Id == 3) },
                new Data.Equipment { Id = 3, Name = "Пресс гидравлический PH-100", InventoryNumber = "INV003",
                    Model = "PH-100", Status = "В работе", CommissionDate = new DateTime(2023, 5, 10),
                    DepartmentId = 3, Department = departments.First(d => d.Id == 3) },
                new Data.Equipment { Id = 4, Name = "Сверлильный станок SD-50", InventoryNumber = "INV004",
                    Model = "SD-50", Status = "Списан", CommissionDate = new DateTime(2022, 8, 1),
                    DepartmentId = 3, Department = departments.First(d => d.Id == 3) },
                new Data.Equipment { Id = 5, Name = "Компьютер Dell Optiplex", InventoryNumber = "INV005",
                    Model = "Optiplex 7070", Status = "В работе", CommissionDate = new DateTime(2023, 2, 10),
                    DepartmentId = 1, Department = departments.First(d => d.Id == 1) },
                new Data.Equipment { Id = 6, Name = "Принтер HP LaserJet", InventoryNumber = "INV006",
                    Model = "LaserJet Pro", Status = "В работе", CommissionDate = new DateTime(2023, 4, 15),
                    DepartmentId = 2, Department = departments.First(d => d.Id == 2) },
                new Data.Equipment { Id = 7, Name = "Сканер штрих-кодов", InventoryNumber = "INV007",
                    Model = "BCR-2000", Status = "В работе", CommissionDate = new DateTime(2023, 6, 20),
                    DepartmentId = 4, Department = departments.First(d => d.Id == 4) }
            };

            _maintenanceList = new List<Data.Maintenance>
            {
                new Data.Maintenance { Id = 1, EquipmentName = "Токарный станок CNC-500", EquipmentId = 1,
                        PlannedDate = DateTime.Now.AddDays(7), Status = "Запланировано" },
                new Data.Maintenance { Id = 2, EquipmentName = "Фрезерный станок FZ-200", EquipmentId = 2,
                        PlannedDate = DateTime.Now.AddDays(2), Status = "В работе" }
            };
        }

        // Обработчик изменения фильтра по отделениям
        private void CmbDepartmentFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbDepartmentFilter.SelectedItem is ComboBoxItem selectedItem)
            {
                if (int.TryParse(selectedItem.Tag?.ToString(), out int departmentId))
                {
                    _selectedDepartmentId = departmentId;
                    ApplyFilters();
                }
            }
        }

        // Применяем все фильтры (поиск + отделение)
        private void ApplyFilters()
        {
            if (_equipmentList == null) return;

            // Начинаем с полного списка
            var filtered = new List<Data.Equipment>(_equipmentList);

            // Фильтр по отделению
            if (_selectedDepartmentId > 0)
            {
                filtered = filtered.Where(eq => eq.DepartmentId == _selectedDepartmentId).ToList();
            }

            // Фильтр по поисковому запросу
            if (txtSearch.Text != "Поиск оборудования..." && !string.IsNullOrWhiteSpace(txtSearch.Text))
            {
                var searchText = txtSearch.Text.ToLower();
                filtered = filtered
                    .Where(equipment => equipment.Name.ToLower().Contains(searchText) ||
                                       equipment.InventoryNumber.ToLower().Contains(searchText) ||
                                       (equipment.Model != null && equipment.Model.ToLower().Contains(searchText)))
                    .ToList();
            }

            _filteredEquipmentList = filtered;

            // Обновляем отображение
            dgEquipment.ItemsSource = null;
            dgEquipment.ItemsSource = _filteredEquipmentList;

            // Обновляем статистику
            UpdateStatistics();

            // Обновляем статус
            if (_selectedDepartmentId > 0)
            {
                var department = _departments.FirstOrDefault(d => d.Id == _selectedDepartmentId);
                var deptName = department?.Name ?? "Неизвестное отделение";
                txtStatus.Text = $"Отфильтровано: {_filteredEquipmentList.Count} оборудования в отделении '{deptName}'";
            }
            else
            {
                txtStatus.Text = $"Отфильтровано: {_filteredEquipmentList.Count} оборудования";
            }
        }

        // Обновляем метод TxtSearch_TextChanged
        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilters();
        }

        // Обновляем метод UpdateStatistics для отображения статистики по отделениям
        private void UpdateStatistics()
        {
            if (_equipmentList == null) return;

            // Общая статистика
            txtTotalEquipment.Text = _filteredEquipmentList.Count.ToString();
            txtActiveEquipment.Text = _filteredEquipmentList.Count(eq => eq.Status == "В работе").ToString();
            txtMaintenanceEquipment.Text = _filteredEquipmentList.Count(eq => eq.Status == "На обслуживании").ToString();
            txtRetiredEquipment.Text = _filteredEquipmentList.Count(eq => eq.Status == "Списан").ToString();

            // Статистика по отделениям
            var departmentStats = _equipmentList
                .Where(eq => eq.DepartmentId.HasValue)
                .GroupBy(eq => eq.DepartmentId)
                .Select(g => new
                {
                    DepartmentId = g.Key,
                    DepartmentName = g.First().Department?.Name ?? "Без отдела",
                    Count = g.Count()
                })
                .OrderByDescending(d => d.Count)
                .Take(5) // Показываем топ-5 отделений
                .ToList();

            if (departmentStats.Any())
            {
                var statsText = string.Join("\n", departmentStats.Select(d =>
                    $"{d.DepartmentName}: {d.Count} ед."));
                txtDepartmentStats.Text = statsText;
            }
            else
            {
                txtDepartmentStats.Text = "Нет данных";
            }
        }

        // Обработчики событий для поиска
        private void TxtSearch_GotFocus(object sender, RoutedEventArgs e)
        {
            if (txtSearch.Text == "Поиск оборудования...")
            {
                txtSearch.Text = "";
                txtSearch.Foreground = Brushes.Black;
            }
        }

        private void TxtSearch_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtSearch.Text))
            {
                txtSearch.Text = "Поиск оборудования...";
                txtSearch.Foreground = Brushes.Gray;
            }
        }

        // Обработчики событий навигации
        private void BtnEquipment_Click(object sender, RoutedEventArgs e)
        {
            ShowPanel(panelEquipment);
            txtStatus.Text = "Режим просмотра оборудования";
        }

        private void BtnMaintenance_Click(object sender, RoutedEventArgs e)
        {
            ShowPanel(panelMaintenance);
            txtStatus.Text = "Режим планирования ТО";

            // Обновляем отображение техобслуживания при переходе на вкладку
            UpdateMaintenanceDisplay();
        }

        private void BtnReports_Click(object sender, RoutedEventArgs e)
        {
            ShowPanel(panelReports);
            txtStatus.Text = "Режим отчетов";
            txtReport.Text = "";
        }

        private void ShowPanel(StackPanel panelToShow)
        {
            panelEquipment.Visibility = Visibility.Collapsed;
            panelMaintenance.Visibility = Visibility.Collapsed;
            panelReports.Visibility = Visibility.Collapsed;
            panelToShow.Visibility = Visibility.Visible;
        }

        // Обработчики кнопок оборудования
        private void BtnAddEquipment_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var editWindow = new EquipmentEditWindow();
                editWindow.Owner = this;

                if (editWindow.ShowDialog() == true)
                {
                    var newEquipment = editWindow.ResultEquipment;

                    if (_dbContext?.Database.CanConnect() == true)
                    {
                        // Сохраняем в БД
                        _dbContext.Equipment.Add(newEquipment);
                        _dbContext.SaveChanges();

                        // Обновляем списки из БД
                        LoadAllData();
                        LoadDepartments();
                        txtStatus.Text = "✅ Оборудование добавлено в базу данных";
                    }
                    else
                    {
                        // Сохраняем в памяти
                        newEquipment.Id = _equipmentList.Count > 0 ? _equipmentList.Max(eq => eq.Id) + 1 : 1;

                        // Находим отделение для нового оборудования
                        if (newEquipment.DepartmentId.HasValue)
                        {
                            var department = _departments.FirstOrDefault(d => d.Id == newEquipment.DepartmentId.Value);
                            newEquipment.Department = department;
                        }

                        _equipmentList.Add(newEquipment);
                        ApplyFilters();
                        txtStatus.Text = "✅ Оборудование добавлено (данные в памяти)";
                    }

                    UpdateStatistics();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка добавления: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnEditEquipment_Click(object sender, RoutedEventArgs e)
        {
            if (dgEquipment.SelectedItem is Data.Equipment selectedEquipment)
            {
                try
                {
                    var editWindow = new EquipmentEditWindow(selectedEquipment);
                    editWindow.Owner = this;

                    if (editWindow.ShowDialog() == true)
                    {
                        var updatedEquipment = editWindow.ResultEquipment;

                        // Обновляем данные
                        selectedEquipment.Name = updatedEquipment.Name;
                        selectedEquipment.InventoryNumber = updatedEquipment.InventoryNumber;
                        selectedEquipment.Model = updatedEquipment.Model;
                        selectedEquipment.Status = updatedEquipment.Status;
                        selectedEquipment.CommissionDate = updatedEquipment.CommissionDate;
                        selectedEquipment.DepartmentId = updatedEquipment.DepartmentId;

                        // Обновляем связанное отделение
                        if (updatedEquipment.DepartmentId.HasValue)
                        {
                            selectedEquipment.Department = _departments.FirstOrDefault(d => d.Id == updatedEquipment.DepartmentId.Value);
                        }
                        else
                        {
                            selectedEquipment.Department = null;
                        }

                        if (_dbContext?.Database.CanConnect() == true)
                        {
                            // Загружаем обновленную запись
                            var dbEquipment = _dbContext.Equipment.Find(selectedEquipment.Id);
                            if (dbEquipment != null)
                            {
                                dbEquipment.Name = updatedEquipment.Name;
                                dbEquipment.InventoryNumber = updatedEquipment.InventoryNumber;
                                dbEquipment.Model = updatedEquipment.Model;
                                dbEquipment.Status = updatedEquipment.Status;
                                dbEquipment.CommissionDate = updatedEquipment.CommissionDate;
                                dbEquipment.DepartmentId = updatedEquipment.DepartmentId;
                                _dbContext.SaveChanges();
                            }
                            txtStatus.Text = "Изменения сохранены в базе данных";
                        }
                        else
                        {
                            txtStatus.Text = "Изменения сохранены (данные в памяти)";
                        }

                        ApplyFilters(); // Обновляем фильтры
                        UpdateStatistics();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка редактирования: {ex.Message}", "Ошибка",
                                  MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Выберите оборудование для редактирования", "Внимание",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnDeleteEquipment_Click(object sender, RoutedEventArgs e)
        {
            if (dgEquipment.SelectedItem is Data.Equipment selectedEquipment)
            {
                var result = MessageBox.Show($"Удалить оборудование '{selectedEquipment.Name}'?\n" +
                                           $"Инвентарный номер: {selectedEquipment.InventoryNumber}",
                                           "Подтверждение удаления",
                                           MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        if (_dbContext?.Database.CanConnect() == true)
                        {
                            // Удаляем из БД
                            var dbEquipment = _dbContext.Equipment.Find(selectedEquipment.Id);
                            if (dbEquipment != null)
                            {
                                _dbContext.Equipment.Remove(dbEquipment);
                                _dbContext.SaveChanges();

                                // Удаляем связанные обслуживания
                                var relatedMaintenances = _dbContext.Maintenance
                                    .Where(m => m.EquipmentId == selectedEquipment.Id)
                                    .ToList();
                                _dbContext.Maintenance.RemoveRange(relatedMaintenances);
                                _dbContext.SaveChanges();
                            }
                        }

                        // Удаляем из списков
                        _equipmentList.Remove(selectedEquipment);
                        _maintenanceList.RemoveAll(m => m.EquipmentId == selectedEquipment.Id);
                        _filteredEquipmentList.Remove(selectedEquipment);

                        ApplyFilters();
                        UpdateStatistics();
                        UpdateMaintenanceDisplay(); // Обновляем отображение техобслуживания
                        txtStatus.Text = "Оборудование удалено";
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка",
                                      MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите оборудование для удаления", "Внимание",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        // Новый метод для списания оборудования
        private void BtnRetireEquipment_Click(object sender, RoutedEventArgs e)
        {
            if (dgEquipment.SelectedItem is Data.Equipment selectedEquipment)
            {
                var result = MessageBox.Show($"Списать оборудование '{selectedEquipment.Name}'?\n" +
                                           $"Инвентарный номер: {selectedEquipment.InventoryNumber}",
                                           "Подтверждение списания",
                                           MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        selectedEquipment.Status = "Списан";

                        if (_dbContext?.Database.CanConnect() == true)
                        {
                            // Обновляем статус в БД
                            var dbEquipment = _dbContext.Equipment.Find(selectedEquipment.Id);
                            if (dbEquipment != null)
                            {
                                dbEquipment.Status = "Списан";
                                _dbContext.SaveChanges();
                            }
                            txtStatus.Text = "Оборудование списано (изменения в БД)";
                        }
                        else
                        {
                            txtStatus.Text = "Оборудование списано (данные в памяти)";
                        }

                        ApplyFilters();
                        UpdateStatistics();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при списании: {ex.Message}", "Ошибка",
                                      MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите оборудование для списания", "Внимание",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        // Обработчики техобслуживания
        private void BtnAddMaintenance_Click(object sender, RoutedEventArgs e)
        {
            // Создаем окно для добавления техобслуживания
            var dialog = new MaintenanceAddWindow();
            dialog.Owner = this;

            // Загружаем список оборудования для выбора
            var availableEquipment = _equipmentList.Where(eq => eq.Status != "Списан").ToList();
            dialog.LoadEquipmentList(availableEquipment);

            if (dialog.ShowDialog() == true)
            {
                var newMaintenance = dialog.ResultMaintenance;

                if (newMaintenance != null)
                {
                    try
                    {
                        // Находим выбранное оборудование
                        var selectedEquipment = _equipmentList.FirstOrDefault(eq => eq.Id == newMaintenance.EquipmentId);
                        if (selectedEquipment != null)
                        {
                            // Меняем статус оборудования на "На обслуживании"
                            selectedEquipment.Status = "На обслуживании";

                            if (_dbContext?.Database.CanConnect() == true)
                            {
                                // Сохраняем в БД
                                _dbContext.Maintenance.Add(newMaintenance);

                                // Обновляем статус оборудования в БД
                                var dbEquipment = _dbContext.Equipment.Find(selectedEquipment.Id);
                                if (dbEquipment != null)
                                {
                                    dbEquipment.Status = "На обслуживании";
                                }
                                _dbContext.SaveChanges();

                                // Обновляем список из БД
                                _maintenanceList = _dbContext.Maintenance.ToList();
                                txtStatus.Text = "Техобслуживание добавлено (изменения в БД)";
                            }
                            else
                            {
                                // Сохраняем в памяти
                                newMaintenance.Id = _maintenanceList.Count > 0 ? _maintenanceList.Max(m => m.Id) + 1 : 1;
                                _maintenanceList.Add(newMaintenance);
                                txtStatus.Text = "Техобслуживание добавлено (данные в памяти)";
                            }

                            ApplyFilters();
                            UpdateStatistics();
                            UpdateMaintenanceDisplay(); // Обновляем отображение техобслуживания
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка добавления техобслуживания: {ex.Message}", "Ошибка",
                                      MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void BtnCompleteMaintenance_Click(object sender, RoutedEventArgs e)
        {
            if (lbMaintenance.SelectedItem is Data.Maintenance selectedMaintenance)
            {
                var result = MessageBox.Show($"Отметить техобслуживание для '{selectedMaintenance.EquipmentName}' как выполненное?",
                                           "Подтверждение",
                                           MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        selectedMaintenance.Status = "Выполнено";

                        // Находим оборудование и меняем его статус обратно на "В работе"
                        var equipment = _equipmentList.FirstOrDefault(eq => eq.Id == selectedMaintenance.EquipmentId);
                        if (equipment != null && equipment.Status == "На обслуживании")
                        {
                            // Проверяем, есть ли другие незавершенные обслуживания для этого оборудования
                            var otherActiveMaintenance = _maintenanceList
                                .Where(m => m.EquipmentId == equipment.Id &&
                                           m.Id != selectedMaintenance.Id &&
                                           m.Status != "Выполнено")
                                .Any();

                            if (!otherActiveMaintenance)
                            {
                                equipment.Status = "В работе";

                                if (_dbContext?.Database.CanConnect() == true)
                                {
                                    // Обновляем в БД
                                    var dbEquipment = _dbContext.Equipment.Find(equipment.Id);
                                    if (dbEquipment != null)
                                    {
                                        dbEquipment.Status = "В работе";
                                    }
                                    _dbContext.SaveChanges();
                                }
                            }
                        }

                        if (_dbContext?.Database.CanConnect() == true)
                        {
                            // Обновляем статус в БД
                            var dbMaintenance = _dbContext.Maintenance.Find(selectedMaintenance.Id);
                            if (dbMaintenance != null)
                            {
                                dbMaintenance.Status = "Выполнено";
                                _dbContext.SaveChanges();
                            }
                        }

                        ApplyFilters();
                        UpdateStatistics();
                        UpdateMaintenanceDisplay(); // Обновляем отображение техобслуживания
                        txtStatus.Text = "Техобслуживание выполнено";
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                                      MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите техобслуживание для отметки о выполнении", "Внимание",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        // Обработчики отчетов
        private void BtnEquipmentReport_Click(object sender, RoutedEventArgs e)
        {
            var equipmentToReport = _selectedDepartmentId > 0
                ? _equipmentList.Where(eq => eq.DepartmentId == _selectedDepartmentId).ToList()
                : _equipmentList;

            var departmentName = _selectedDepartmentId > 0
                ? _departments.FirstOrDefault(d => d.Id == _selectedDepartmentId)?.Name ?? "Неизвестное отделение"
                : "Все отделения";

            var report = $"ОТЧЕТ ПО ОБОРУДОВАНИЮ\n" +
                        $"Отделение: {departmentName}\n" +
                        $"Дата формирования: {DateTime.Now:dd.MM.yyyy HH:mm}\n" +
                        $"Пользователь: {CurrentUser.User.FullName}\n\n" +
                        $"Всего оборудования: {equipmentToReport.Count}\n" +
                        $"В работе: {equipmentToReport.Count(eq => eq.Status == "В работе")}\n" +
                        $"На обслуживании: {equipmentToReport.Count(eq => eq.Status == "На обслуживании")}\n" +
                        $"Списано: {equipmentToReport.Count(eq => eq.Status == "Списан")}\n\n" +
                        $"РАСПРЕДЕЛЕНИЕ ПО ОТДЕЛЕНИЯМ:\n";

            var departmentStats = equipmentToReport
                .GroupBy(eq => eq.Department?.Name ?? "Без отдела")
                .OrderByDescending(g => g.Count())
                .ToList();

            foreach (var group in departmentStats)
            {
                report += $"- {group.Key}: {group.Count()} ед.\n";
            }

            report += $"\nСПИСОК ОБОРУДОВАНИЯ:\n";

            foreach (var equipment in equipmentToReport.OrderBy(eq => eq.Department?.Name).ThenBy(eq => eq.Name))
            {
                var deptName = equipment.Department?.Name ?? "Без отдела";
                report += $"- {equipment.Name} ({equipment.InventoryNumber}) - {deptName} - {equipment.Status}\n";
            }

            txtReport.Text = report;
            txtStatus.Text = $"Сформирован отчет по оборудованию ({departmentName})";
        }

        private void BtnMaintenanceReport_Click(object sender, RoutedEventArgs e)
        {
            var report = $"ОТЧЕТ ПО ТЕХОБСЛУЖИВАНИЮ\n" +
                        $"Дата формирования: {DateTime.Now:dd.MM.yyyy HH:mm}\n" +
                        $"Пользователь: {CurrentUser.User.FullName}\n\n" +
                        $"Всего записей ТО: {_maintenanceList.Count}\n" +
                        $"Запланировано: {_maintenanceList.Count(m => m.Status == "Запланировано")}\n" +
                        $"В работе: {_maintenanceList.Count(m => m.Status == "В работе")}\n" +
                        $"Выполнено: {_maintenanceList.Count(m => m.Status == "Выполнено")}\n\n" +
                        $"ПРЕДСТОЯЩИЕ РАБОТЫ:\n";

            var upcoming = _maintenanceList.Where(m => m.Status != "Выполнено")
                                         .OrderBy(m => m.PlannedDate);

            foreach (var maintenance in upcoming)
            {
                report += $"- {maintenance.EquipmentName} - {maintenance.PlannedDate:dd.MM.yyyy} ({maintenance.Status})\n";
            }

            txtReport.Text = report;
            txtStatus.Text = "Сформирован отчет по техобслуживанию";
        }

        private void DgEquipment_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgEquipment.SelectedItem != null)
            {
                txtStatus.Text = "Выбрано оборудование для действий";
            }
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Вы уверены, что хотите выйти?", "Подтверждение",
                                        MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                CurrentUser.User = null;
                var loginWindow = new LoginWindow();
                loginWindow.Show();
                Close();
            }
        }

        private void BtnTestDB_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var testContext = new AppDbContext())
                {
                    bool canConnect = testContext.Database.CanConnect();

                    if (canConnect)
                    {
                        int equipmentCount = testContext.Equipment.Count();
                        int maintenanceCount = testContext.Maintenance.Count();

                        MessageBox.Show($"База данных подключена успешно!\n\n" +
                                      $"Оборудование в БД: {equipmentCount} записей\n" +
                                      $"Техобслуживание в БД: {maintenanceCount} записей",
                                      "Проверка подключения",
                                      MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Ошибка подключения: {ex.Message}",
                              "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnManageDepartments_Click(object sender, RoutedEventArgs e)
        {
            var departmentWindow = new DepartmentWindow();
            departmentWindow.Owner = this;

            if (departmentWindow.ShowDialog() == true)
            {
                // Обновляем данные после закрытия окна управления отделениями
                LoadDepartments();
                LoadAllData();
                UpdateStatistics();
                txtStatus.Text = "Список отделений обновлен";
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            _dbContext?.Dispose();
            base.OnClosed(e);
        }

        private void BtnDepartments_Click(object sender, RoutedEventArgs e)
        {
            var departmentWindow = new DepartmentWindow();
            departmentWindow.Owner = this;

            if (departmentWindow.ShowDialog() == true)
            {
                // Обновляем данные после закрытия окна управления отделениями
                LoadDepartments();
                LoadAllData();
                UpdateStatistics();
                txtStatus.Text = "Список отделений обновлен";
            }
        }

        private void BtnAbout_Click(object sender, RoutedEventArgs e)
        {
            var aboutWindow = new AboutWindow();
            aboutWindow.Owner = this;
            aboutWindow.ShowDialog();

            txtStatus.Text = "Окно 'О проекте' закрыто";
        }
    }
}