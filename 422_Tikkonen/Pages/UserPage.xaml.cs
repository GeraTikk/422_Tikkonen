using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace _422_Tikkonen.Pages
{
    public partial class UserPage : Page
    {
        public UserPage()
        {
            InitializeComponent();
            LoadUsers();
        }

        private void LoadUsers()
        {
            try
            {
                using (var db = new Tikkonen_DB_PaymentEntities())
                {
                    var currentUsers = db.User.ToList();
                    ListUser.ItemsSource = currentUsers;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки пользователей: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void clearFiltersButton_Click_1(object sender, RoutedEventArgs e)
        {
            fioFilterTextBox.Text = "";
            sortComboBox.SelectedIndex = 0;
            onlyAdminCheckBox.IsChecked = false;
        }

        private void fioFilterTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateUsers();
        }

        private void sortComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateUsers();
        }

        private void onlyAdminCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            UpdateUsers();
        }

        private void onlyAdminCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            UpdateUsers();
        }

        private void UpdateUsers()
        {
            if (!IsInitialized)
            {
                return;
            }

            try
            {
                using (var db = new Tikkonen_DB_PaymentEntities())
                {
                    List<User> currentUsers = db.User.ToList();

                    // Фильтрация по ФИО
                    if (!string.IsNullOrWhiteSpace(fioFilterTextBox.Text))
                    {
                        currentUsers = currentUsers.Where(x => x.FIO.ToLower().Contains(fioFilterTextBox.Text.ToLower())).ToList();
                    }

                    // Фильтрация по роли
                    if (onlyAdminCheckBox.IsChecked.Value)
                    {
                        currentUsers = currentUsers.Where(x => x.Role == "Admin").ToList();
                    }

                    // Сортировка по убыванию/возрастанию
                    if (sortComboBox.SelectedIndex == 0)
                    {
                        ListUser.ItemsSource = currentUsers.OrderBy(x => x.FIO).ToList();
                    }
                    else
                    {
                        ListUser.ItemsSource = currentUsers.OrderByDescending(x => x.FIO).ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обновлении списка пользователей: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}