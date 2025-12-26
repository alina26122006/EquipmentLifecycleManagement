using EquipmentLifecycleManager.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace EquipmentLifecycleManager
{
    public partial class MaintenanceAddWindow : Window
    {
        private List<Equipment> equipmentList;
        private Maintenance resultMaintenance;

        public MaintenanceAddWindow()
        {
            InitializeComponent();
            dpPlannedDate.SelectedDate = DateTime.Now.AddDays(7);
            cmbMaintenanceStatus.SelectedIndex = 0;
        }

        public void LoadEquipmentList(List<Equipment> equipment)
        {
            equipmentList = equipment;
            cmbEquipment.ItemsSource = equipmentList;
            if (equipmentList.Any())
            {
                cmbEquipment.SelectedIndex = 0;
            }
        }

        public Maintenance ResultMaintenance => resultMaintenance;

        private void CmbEquipment_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbEquipment.SelectedItem is Equipment selectedEquipment)
            {
                txtInventoryNumber.Text = selectedEquipment.InventoryNumber;
                txtCurrentStatus.Text = selectedEquipment.Status;
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            // Валидация
            if (cmbEquipment.SelectedItem == null)
            {
                MessageBox.Show("Выберите оборудование", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
                cmbEquipment.Focus();
                return;
            }

            if (dpPlannedDate.SelectedDate == null)
            {
                MessageBox.Show("Выберите планируемую дату", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
                dpPlannedDate.Focus();
                return;
            }

            if (cmbMaintenanceStatus.SelectedItem == null)
            {
                MessageBox.Show("Выберите статус техобслуживания", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
                cmbMaintenanceStatus.Focus();
                return;
            }

            var selectedEquipment = cmbEquipment.SelectedItem as Equipment;
            var status = (cmbMaintenanceStatus.SelectedItem as ComboBoxItem)?.Content.ToString();

            // Создаем объект техобслуживания
            resultMaintenance = new Maintenance
            {
                EquipmentId = selectedEquipment.Id,
                EquipmentName = selectedEquipment.Name,
                PlannedDate = dpPlannedDate.SelectedDate.Value,
                Status = status
            };

            DialogResult = true;
            Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}