using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace _422_Tikkonen.Pages
{
    /// <summary>
    /// Логика взаимодействия для AddUserPage.xaml
    /// </summary>
    public partial class AddUserPage : Page
    {
        private User _currentUser = new User();

        public AddUserPage(User selectedUser)
        {
            InitializeComponent();

            if (selectedUser != null)
                _currentUser = selectedUser;

            DataContext = _currentUser;

            if (!string.IsNullOrEmpty(_currentUser.Role))
            {
                cmbRole.Text = _currentUser.Role;
            }
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder errors = new StringBuilder();

            if (string.IsNullOrWhiteSpace(_currentUser.Login))
                errors.AppendLine("Укажите логин!");
            if (string.IsNullOrWhiteSpace(_currentUser.Password))
                errors.AppendLine("Укажите пароль!");
            if ((_currentUser.Role == null) || (cmbRole.Text == ""))
                errors.AppendLine("Выберите роль!");
            else
                _currentUser.Role = cmbRole.Text;
            if (string.IsNullOrWhiteSpace(_currentUser.FIO))
                errors.AppendLine("Укажите ФИО");

            if (errors.Length > 0)
            {
                MessageBox.Show(errors.ToString());
                return;
            }

            if (_currentUser.ID == 0)
                Tikkonen_DB_PaymentEntities.GetContext().User.Add(_currentUser);

            try
            {
                Tikkonen_DB_PaymentEntities.GetContext().SaveChanges();
                MessageBox.Show("Данные успешно сохранены!");
                NavigationService?.GoBack();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }

        private void ButtonClean_Click(object sender, RoutedEventArgs e)
        {
            TBLogin.Text = "";
            TBPass.Text = "";
            cmbRole.SelectedIndex = -1;
            TBFio.Text = "";
            TBPhoto.Text = "";
            TBEmail.Text = "";
        }
    }
}