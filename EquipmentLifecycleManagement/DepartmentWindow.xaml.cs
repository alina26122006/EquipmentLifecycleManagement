using EquipmentLifecycleManager.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace EquipmentLifecycleManager
{
    public partial class DepartmentWindow : Window
    {
        private List<Data.Department> _departments;
        private Data.Department _selectedDepartment;
        private AppDbContext _dbContext;

        public DepartmentWindow()
        {
            InitializeComponent();
            _dbContext = new AppDbContext();
            LoadDepartments();
        }

        private void LoadDepartments()
        {
            try
            {
                if (_dbContext.Database.CanConnect())
                {
                    _departments = _dbContext.Departments
                        .OrderBy(d => d.Name)
                        .ToList();
                }
                else
                {
                    // Данные в памяти
                    _departments = GetDefaultDepartments();
                }

                lbDepartments.ItemsSource = null;
                lbDepartments.ItemsSource = _departments;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки отделений: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
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

        private void LbDepartments_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            _selectedDepartment = lbDepartments.SelectedItem as Data.Department;

            if (_selectedDepartment != null)
            {
                txtDepartmentName.Text = _selectedDepartment.Name;
                txtDepartmentCode.Text = _selectedDepartment.Code;
                txtDepartmentDescription.Text = _selectedDepartment.Description;
                chkIsActive.IsChecked = _selectedDepartment.IsActive;

                btnDeleteDepartment.IsEnabled = true;
                btnSaveDepartment.Content = "💾 Обновить";
            }
            else
            {
                ClearForm();
                btnDeleteDepartment.IsEnabled = false;
                btnSaveDepartment.Content = "💾 Сохранить";
            }
        }

        private void ClearForm()
        {
            txtDepartmentName.Text = "";
            txtDepartmentCode.Text = "";
            txtDepartmentDescription.Text = "";
            chkIsActive.IsChecked = true;
        }

        private void BtnAddDepartment_Click(object sender, RoutedEventArgs e)
        {
            _selectedDepartment = null;
            ClearForm();
            lbDepartments.SelectedItem = null;
            txtDepartmentName.Focus();
        }

        private void BtnSaveDepartment_Click(object sender, RoutedEventArgs e)
        {
            // Валидация
            if (string.IsNullOrWhiteSpace(txtDepartmentName.Text))
            {
                MessageBox.Show("Введите название отделения", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
                txtDepartmentName.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtDepartmentCode.Text))
            {
                MessageBox.Show("Введите код отделения", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
                txtDepartmentCode.Focus();
                return;
            }

            try
            {
                if (_selectedDepartment == null)
                {
                    // Добавление нового отделения
                    var newDepartment = new Data.Department
                    {
                        Name = txtDepartmentName.Text.Trim(),
                        Code = txtDepartmentCode.Text.Trim().ToUpper(),
                        Description = txtDepartmentDescription.Text?.Trim(),
                        IsActive = chkIsActive.IsChecked == true
                    };

                    if (_dbContext.Database.CanConnect())
                    {
                        _dbContext.Departments.Add(newDepartment);
                        _dbContext.SaveChanges();
                    }
                    else
                    {
                        // В памяти
                        newDepartment.Id = _departments.Count > 0 ? _departments.Max(d => d.Id) + 1 : 1;
                        _departments.Add(newDepartment);
                    }

                    MessageBox.Show("Отделение добавлено", "Успех",
                                  MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    // Обновление существующего отделения
                    _selectedDepartment.Name = txtDepartmentName.Text.Trim();
                    _selectedDepartment.Code = txtDepartmentCode.Text.Trim().ToUpper();
                    _selectedDepartment.Description = txtDepartmentDescription.Text?.Trim();
                    _selectedDepartment.IsActive = chkIsActive.IsChecked == true;

                    if (_dbContext.Database.CanConnect())
                    {
                        _dbContext.SaveChanges();
                    }

                    MessageBox.Show("Отделение обновлено", "Успех",
                                  MessageBoxButton.OK, MessageBoxImage.Information);
                }

                LoadDepartments();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnDeleteDepartment_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedDepartment == null) return;

            var result = MessageBox.Show($"Удалить отделение '{_selectedDepartment.Name}'?\n" +
                                       "Примечание: оборудование этого отделения будет перемещено в 'Без отдела'.",
                                       "Подтверждение удаления",
                                       MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    if (_dbContext.Database.CanConnect())
                    {
                        // Помечаем как неактивное вместо удаления
                        _selectedDepartment.IsActive = false;
                        _dbContext.SaveChanges();
                    }
                    else
                    {
                        // В памяти
                        _departments.Remove(_selectedDepartment);
                    }

                    MessageBox.Show("Отделение удалено", "Успех",
                                  MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadDepartments();
                    ClearForm();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка удаления: {ex.Message}", "Ошибка",
                                  MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}