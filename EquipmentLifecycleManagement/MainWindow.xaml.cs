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
        private List<Equipment> equipmentList;
        private List<Maintenance> maintenanceList;
        private List<Equipment> filteredEquipmentList;
        private AppDbContext dbContext;

        public MainWindow()
        {
            InitializeComponent();

            equipmentList = new List<Equipment>();
            maintenanceList = new List<Maintenance>();
            filteredEquipmentList = new List<Equipment>();

            InitializeData();
            InitializeDatabase();
            LoadEquipmentData();
            UpdateStatistics();
        }

        private void InitializeDatabase()
        {
            try
            {
                dbContext = new AppDbContext();

                // Проверяем подключение к БД
                bool canConnect = dbContext.Database.CanConnect();

                if (canConnect)
                {
                    // Создаем таблицы если их нет
                    dbContext.Database.EnsureCreated();
                    txtStatus.Text = "База данных подключена успешно";

                    // Добавляем тестовые данные если таблицы пустые
                    if (!dbContext.Equipment.Any())
                    {
                        AddTestData();
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

        private void AddTestData()
        {
            var testEquipment = new List<Equipment>
    {
        new Equipment { Name = "Токарный станок CNC-500", InventoryNumber = "INV001",
                       Model = "CNC-500", Status = "В работе", CommissionDate = new DateTime(2023, 1, 15) },
        new Equipment { Name = "Фрезерный станок FZ-200", InventoryNumber = "INV002",
                       Model = "FZ-200", Status = "На обслуживании", CommissionDate = new DateTime(2023, 3, 20) },
        new Equipment { Name = "Пресс гидравлический PH-100", InventoryNumber = "INV003",
                       Model = "PH-100", Status = "В работе", CommissionDate = new DateTime(2023, 5, 10) },
        new Equipment { Name = "Сверлильный станок SD-50", InventoryNumber = "INV004",
                       Model = "SD-50", Status = "Списан", CommissionDate = new DateTime(2022, 8, 1) }
    };

            dbContext.Equipment.AddRange(testEquipment);
            dbContext.SaveChanges();

            var equipment = dbContext.Equipment.First();
            var testMaintenance = new List<Maintenance>
    {
        new Maintenance { EquipmentName = "Токарный станок CNC-500", EquipmentId = equipment.Id,
                        PlannedDate = DateTime.Now.AddDays(7), Status = "Запланировано" },
        new Maintenance { EquipmentName = "Фрезерный станок FZ-200", EquipmentId = equipment.Id,
                        PlannedDate = DateTime.Now.AddDays(2), Status = "В работе" },
        new Maintenance { EquipmentName = "Пресс гидравлический PH-100", EquipmentId = equipment.Id,
                        PlannedDate = DateTime.Now.AddDays(14), Status = "Запланировано" }
    };

            dbContext.Maintenance.AddRange(testMaintenance);
            dbContext.SaveChanges();
        }

        private void InitializeData()
        {
            try
            {
                // Пытаемся загрузить данные из БД
                if (dbContext?.Database.CanConnect() == true)
                {
                    equipmentList = dbContext.Equipment.ToList();
                    maintenanceList = dbContext.Maintenance.ToList();
                    txtStatus.Text = " Днные загружены из базы данных";
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

            filteredEquipmentList = new List<Equipment>(equipmentList);
        }


        private void UseInMemoryData()
        {
            equipmentList = new List<Equipment>
        {
            new Equipment { Id = 1, Name = "Токарный станок CNC-500", InventoryNumber = "INV001",
                       Model = "CNC-500", Status = "В работе", CommissionDate = new DateTime(2023, 1, 15) },
            new Equipment { Id = 2, Name = "Фрезерный станок FZ-200", InventoryNumber = "INV002",
                       Model = "FZ-200", Status = "На обслуживании", CommissionDate = new DateTime(2023, 3, 20) }
        };

            maintenanceList = new List<Maintenance>
            {       
                new Maintenance { Id = 1, EquipmentName = "Токарный станок CNC-500",
                        PlannedDate = DateTime.Now.AddDays(7), Status = "Запланировано" }
            };
        }

        private void LoadEquipmentData()
        {
            if (filteredEquipmentList == null || maintenanceList == null) return;
            dgEquipment.ItemsSource = null;
            dgEquipment.ItemsSource = filteredEquipmentList;

            lbMaintenance.ItemsSource = null;
            lbMaintenance.ItemsSource = maintenanceList;

            txtUpcomingCount.Text = maintenanceList.Count(m => m.Status != "Выполнено").ToString();
        }

        private void UpdateStatistics()
        {
            if (equipmentList == null) return;

            txtTotalEquipment.Text = equipmentList.Count.ToString();
            txtActiveEquipment.Text = equipmentList.Count(e => e.Status == "В работе").ToString();
            txtMaintenanceEquipment.Text = equipmentList.Count(e => e.Status == "На обслуживании").ToString();
            txtRetiredEquipment.Text = equipmentList.Count(e => e.Status == "Списан").ToString();
        }

        // Обработчики событий для placeholder текста
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

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (equipmentList == null) return;

            if (txtSearch.Text != "Поиск оборудования..." && !string.IsNullOrWhiteSpace(txtSearch.Text))
            {
                var searchText = txtSearch.Text.ToLower();
                filteredEquipmentList = equipmentList
                    .Where(equipment => equipment.Name.ToLower().Contains(searchText) ||
                                       equipment.InventoryNumber.ToLower().Contains(searchText) ||
                                       equipment.Model.ToLower().Contains(searchText))
                    .ToList();
                txtStatus.Text = $"Найдено: {filteredEquipmentList.Count} оборудования";
            }
            else
            {
                filteredEquipmentList = new List<Equipment>(equipmentList);
                txtStatus.Text = "Готов к работе";
            }

            LoadEquipmentData();
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
        private void BtnAddEquipment_Click(object sender, RoutedEventArgs args)
        {
            try
            {
                var newEquipment = new Equipment
                {
                    Name = "Новое оборудование",
                    InventoryNumber = $"INV{Guid.NewGuid().ToString().Substring(0, 8)}",
                    Model = "Модель",
                    Status = "В работе",
                    CommissionDate = DateTime.Now
                };

                if (dbContext?.Database.CanConnect() == true)
                {
                    // Сохраняем в БД
                    dbContext.Equipment.Add(newEquipment);
                    dbContext.SaveChanges();

                    // Обновляем список из БД
                    equipmentList = dbContext.Equipment.ToList();
                }
                else
                {
                    // Сохраняем в памяти
                    newEquipment.Id = equipmentList.Count > 0 ? equipmentList.Max(e => e.Id) + 1 : 1;
                    equipmentList.Add(newEquipment);
                }

                filteredEquipmentList = new List<Equipment>(equipmentList);
                LoadEquipmentData();
                UpdateStatistics();
                txtStatus.Text = "Оборудование добавлено";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка добавления: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnEditEquipment_Click(object sender, RoutedEventArgs e)
        {
            if (dgEquipment.SelectedItem is Equipment selectedEquipment)
            {
                selectedEquipment.Name = $"{selectedEquipment.Name} (изм.)";
                LoadEquipmentData();
                txtStatus.Text = "Оборудование отредактировано";
            }
            else
            {
                MessageBox.Show("Выберите оборудование для редактирования", "Внимание",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnDeleteEquipment_Click(object sender, RoutedEventArgs e)
        {
            if (dgEquipment.SelectedItem is Equipment selectedEquipment)
            {
                var result = MessageBox.Show("Удалить выбранное оборудование?", "Подтверждение",
                                           MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    equipmentList.Remove(selectedEquipment);
                    filteredEquipmentList.Remove(selectedEquipment);
                    LoadEquipmentData();
                    UpdateStatistics();
                    txtStatus.Text = "Оборудование удалено";
                }
            }
        }

        // Обработчики техобслуживания
        private void BtnAddMaintenance_Click(object sender, RoutedEventArgs e)
        {
            var newId = maintenanceList.Count > 0 ? maintenanceList.Max(m => m.Id) + 1 : 1;
            var newMaintenance = new Maintenance
            {
                Id = newId,
                EquipmentName = "Новое оборудование",
                PlannedDate = DateTime.Now.AddDays(30),
                Status = "Запланировано"
            };

            maintenanceList.Add(newMaintenance);
            LoadEquipmentData();
            txtStatus.Text = "Техобслуживание добавлено";
        }

        private void BtnCompleteMaintenance_Click(object sender, RoutedEventArgs e)
        {
            if (lbMaintenance.SelectedItem is Maintenance selectedMaintenance)
            {
                selectedMaintenance.Status = "Выполнено";
                LoadEquipmentData();
                txtStatus.Text = "Техобслуживание выполнено";
            }
        }

        // Обработчики отчетов
        private void BtnEquipmentReport_Click(object sender, RoutedEventArgs args)
        {
            var report = $"ОТЧЕТ ПО ОБОРУДОВАНИЮ\n" +
                        $"Дата формирования: {DateTime.Now:dd.MM.yyyy HH:mm}\n\n" +
                        $"Всего оборудования: {equipmentList.Count}\n" +
                        $"В работе: {equipmentList.Count(e => e.Status == "В работе")}\n" +
                        $"На обслуживании: {equipmentList.Count(e => e.Status == "На обслуживании")}\n" +
                        $"Списано: {equipmentList.Count(e => e.Status == "Списан")}\n\n" +
                        $"СПИСОК ОБОРУДОВАНИЯ:\n";

            foreach (var equipment in equipmentList)
            {
                report += $"- {equipment.Name} ({equipment.InventoryNumber}) - {equipment.Status}\n";
            }

            txtReport.Text = report;
            txtStatus.Text = "Сформирован отчет по оборудованию";
        }

        private void BtnMaintenanceReport_Click(object sender, RoutedEventArgs e)
        {
            var report = $"ОТЧЕТ ПО ТЕХОБСЛУЖИВАНИЮ\n" +
                        $"Дата формирования: {DateTime.Now:dd.MM.yyyy HH:mm}\n\n" +
                        $"Всего записей ТО: {maintenanceList.Count}\n" +
                        $"Запланировано: {maintenanceList.Count(m => m.Status == "Запланировано")}\n" +
                        $"В работе: {maintenanceList.Count(m => m.Status == "В работе")}\n" +
                        $"Выполнено: {maintenanceList.Count(m => m.Status == "Выполнено")}\n\n" +
                        $"ПРЕДСТОЯЩИЕ РАБОТЫ:\n";

            var upcoming = maintenanceList.Where(m => m.Status != "Выполнено")
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
        protected override void OnClosed(EventArgs e)
        {
            dbContext?.Dispose();
            base.OnClosed(e);
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
    }
}