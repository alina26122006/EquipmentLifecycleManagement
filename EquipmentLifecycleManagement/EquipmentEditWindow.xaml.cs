using EquipmentLifecycleManager.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;

namespace EquipmentLifecycleManager
{
    public partial class EquipmentEditWindow : Window, INotifyPropertyChanged
    {
        private string _equipmentName;
        private string _inventoryNumber;
        private string _model;
        private string _status;
        private DateTime _commissionDate;
        private string _windowTitle;
        private int? _selectedDepartmentId;
        private List<Data.Department> _departments;

        public EquipmentEditWindow(Data.Equipment equipment = null)
        {
            InitializeComponent();
            DataContext = this;

            // Загружаем список отделений
            LoadDepartments();

            if (equipment != null)
            {
                // Режим редактирования
                EquipmentName = equipment.Name;
                InventoryNumber = equipment.InventoryNumber;
                Model = equipment.Model;
                Status = equipment.Status;
                CommissionDate = equipment.CommissionDate;
                SelectedDepartmentId = equipment.DepartmentId;
                WindowTitle = "Редактирование оборудования";
            }
            else
            {
                // Режим добавления
                EquipmentName = "";
                InventoryNumber = $"INV{DateTime.Now:yyyyMMddHHmmss}";
                Model = "";
                Status = "В работе";
                CommissionDate = DateTime.Now;
                WindowTitle = "Добавление оборудования";
            }
        }

        private void LoadDepartments()
        {
            try
            {
                using (var context = new AppDbContext())
                {
                    if (context.Database.CanConnect())
                    {
                        _departments = context.Departments.Where(d => d.IsActive).ToList();
                        
                        // Если отделений нет, создаем несколько тестовых
                        if (!_departments.Any())
                        {
                            CreateDefaultDepartments(context);
                            _departments = context.Departments.Where(d => d.IsActive).ToList();
                        }
                    }
                    else
                    {
                        // Данные в памяти
                        _departments = GetDefaultDepartments();
                    }
                }
                
                cmbDepartment.ItemsSource = _departments;
            }
            catch
            {
                _departments = GetDefaultDepartments();
                cmbDepartment.ItemsSource = _departments;
            }
        }

        private void CreateDefaultDepartments(AppDbContext context)
        {
            var defaultDepartments = new List<Data.Department>
            {
                new Data.Department { Name = "Информационные технологии", Code = "ИТ", Description = "IT отдел" },
                new Data.Department { Name = "Бухгалтерия", Code = "БУХ", Description = "Бухгалтерский отдел" },
                new Data.Department { Name = "Производство", Code = "ПРОИЗВ", Description = "Производственный отдел" },
                new Data.Department { Name = "Склад", Code = "СКЛАД", Description = "Складское хозяйство" },
                new Data.Department { Name = "Отдел кадров", Code = "ОК", Description = "Отдел кадров" },
                new Data.Department { Name = "Администрация", Code = "АДМ", Description = "Административный отдел" }
            };

            context.Departments.AddRange(defaultDepartments);
            context.SaveChanges();
        }

        private List<Data.Department> GetDefaultDepartments()
        {
            return new List<Data.Department>
            {
                new Data.Department { Id = 1, Name = "Информационные технологии", Code = "ИТ", IsActive = true },
                new Data.Department { Id = 2, Name = "Бухгалтерия", Code = "БУХ", IsActive = true },
                new Data.Department { Id = 3, Name = "Производство", Code = "ПРОИЗВ", IsActive = true },
                new Data.Department { Id = 4, Name = "Склад", Code = "СКЛАД", IsActive = true }
            };
        }

        // Свойства с уведомлениями об изменении
        public string EquipmentName
        {
            get => _equipmentName;
            set { _equipmentName = value; OnPropertyChanged(nameof(EquipmentName)); }
        }

        public string InventoryNumber
        {
            get => _inventoryNumber;
            set { _inventoryNumber = value; OnPropertyChanged(nameof(InventoryNumber)); }
        }

        public string Model
        {
            get => _model;
            set { _model = value; OnPropertyChanged(nameof(Model)); }
        }

        public string Status
        {
            get => _status;
            set { _status = value; OnPropertyChanged(nameof(Status)); }
        }

        public DateTime CommissionDate
        {
            get => _commissionDate;
            set { _commissionDate = value; OnPropertyChanged(nameof(CommissionDate)); }
        }

        public string WindowTitle
        {
            get => _windowTitle;
            set { _windowTitle = value; OnPropertyChanged(nameof(WindowTitle)); }
        }

        public int? SelectedDepartmentId
        {
            get => _selectedDepartmentId;
            set { _selectedDepartmentId = value; OnPropertyChanged(nameof(SelectedDepartmentId)); }
        }

        // Результат - созданное/отредактированное оборудование
        public Data.Equipment ResultEquipment { get; private set; }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            // Валидация
            if (string.IsNullOrWhiteSpace(EquipmentName))
            {
                MessageBox.Show("Введите наименование оборудования", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
                txtName.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(InventoryNumber))
            {
                MessageBox.Show("Введите инвентарный номер", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
                txtInventoryNumber.Focus();
                return;
            }

            // Создаем объект оборудования
            ResultEquipment = new Data.Equipment
            {
                Name = EquipmentName.Trim(),
                InventoryNumber = InventoryNumber.Trim(),
                Model = Model?.Trim(),
                Status = Status,
                CommissionDate = CommissionDate,
                DepartmentId = SelectedDepartmentId
            };

            DialogResult = true;
            Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}