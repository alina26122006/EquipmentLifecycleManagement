using EquipmentLifecycleManager.Data;
using System;
using System.ComponentModel;
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

        public EquipmentEditWindow(Equipment equipment = null)
        {
            InitializeComponent();
            DataContext = this;

            if (equipment != null)
            {
                // Режим редактирования
                EquipmentName = equipment.Name;
                InventoryNumber = equipment.InventoryNumber;
                Model = equipment.Model;
                Status = equipment.Status;
                CommissionDate = equipment.CommissionDate;
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

        // Результат - созданное/отредактированное оборудование
        public Equipment ResultEquipment { get; private set; }

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
            ResultEquipment = new Equipment
            {
                Name = EquipmentName.Trim(),
                InventoryNumber = InventoryNumber.Trim(),
                Model = Model?.Trim(),
                Status = Status,
                CommissionDate = CommissionDate
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