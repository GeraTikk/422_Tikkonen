using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace _422_Tikkonen.Pages
{
    public partial class RegPage : Page
    {
        public RegPage()
        {
            InitializeComponent();
            comboBxRole.SelectedIndex = 0;
        }

        // Обработчик изменения выбора в ComboBox
        private void comboBxRole_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Можно добавить дополнительную логику при изменении роли
        }

        // Метод для хеширования пароля (используем тот же, что и в AuthPage)
        public static string GetHash(string password)
        {
            using (var hash = SHA1.Create())
            {
                return string.Concat(hash.ComputeHash(Encoding.UTF8.GetBytes(password))
                    .Select(x => x.ToString("X2")));
            }
        }

        // Обработчики для placeholder'ов
        private void lblLogHitn_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            txtbxLog.Focus();
        }

        private void lblPassHitn_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            passBxFrst.Focus();
        }

        private void lblPassSecHitn_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            passBxScnd.Focus();
        }

        private void lblFioHitn_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            txtbxFIO.Focus();
        }

        // Обработчики изменения текста (скрытие placeholder'ов)
        private void txtbxLog_TextChanged(object sender, TextChangedEventArgs e)
        {
            lblLogHitn.Visibility = Visibility.Visible;
            if (txtbxLog.Text.Length > 0)
            {
                lblLogHitn.Visibility = Visibility.Hidden;
            }
        }

        private void passBxFrst_PasswordChanged(object sender, RoutedEventArgs e)
        {
            lblPassHitn.Visibility = Visibility.Visible;
            if (passBxFrst.Password.Length > 0)
            {
                lblPassHitn.Visibility = Visibility.Hidden;
            }
        }

        private void passBxScnd_PasswordChanged(object sender, RoutedEventArgs e)
        {
            lblPassSecHitn.Visibility = Visibility.Visible;
            if (passBxScnd.Password.Length > 0)
            {
                lblPassSecHitn.Visibility = Visibility.Hidden;
            }
        }

        private void txtbxFIO_TextChanged(object sender, TextChangedEventArgs e)
        {
            lblFioHitn.Visibility = Visibility.Visible;
            if (txtbxFIO.Text.Length > 0)
            {
                lblFioHitn.Visibility = Visibility.Hidden;
            }
        }

        // Обработчик кнопки регистрации
        private void regButton_Click(object sender, RoutedEventArgs e)
        {
            // Проверка заполнения всех полей
            if (string.IsNullOrEmpty(txtbxLog.Text) ||
                string.IsNullOrEmpty(txtbxFIO.Text) ||
                string.IsNullOrEmpty(passBxFrst.Password) ||
                string.IsNullOrEmpty(passBxScnd.Password))
            {
                MessageBox.Show("Заполните все поля!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Временная заглушка для проверки логики (без БД)
            // TODO: Раскомментировать когда будет готова база данных

            
            using (var db = new Tikkonen_DB_PaymentEntities())
            {
                var user = db.User
                    .AsNoTracking()
                    .FirstOrDefault(u => u.Login == txtbxLog.Text);

                if (user != null)
                {
                    MessageBox.Show("Пользователь с таким логином уже существует!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
            

            // Проверка формата пароля
            if (passBxFrst.Password.Length < 6)
            {
                MessageBox.Show("Пароль слишком короткий, должно быть минимум 6 символов!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            bool en = true;
            bool number = false;

            for (int i = 0; i < passBxFrst.Password.Length; i++)
            {
                if (passBxFrst.Password[i] >= '0' && passBxFrst.Password[i] <= '9')
                    number = true;
                else if (!((passBxFrst.Password[i] >= 'A' && passBxFrst.Password[i] <= 'Z') ||
                          (passBxFrst.Password[i] >= 'a' && passBxFrst.Password[i] <= 'z')))
                    en = false;
            }

            if (!en)
            {
                MessageBox.Show("Используйте только английскую раскладку!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            else if (!number)
            {
                MessageBox.Show("Добавьте хотя бы одну цифру!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Проверка совпадения паролей
            if (passBxFrst.Password != passBxScnd.Password)
            {
                MessageBox.Show("Пароли не совпадают!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Если все проверки прошли успешно
            // TODO: Раскомментировать когда будет готова база данных

            
            using (var db = new Tikkonen_DB_PaymentEntities())
            {
                string hashedPassword = GetHash(passBxFrst.Password);

                User userObject = new User
                {
                    FIO = txtbxFIO.Text,
                    Login = txtbxLog.Text,
                    Password = hashedPassword,
                    Role = comboBxRole.Text
                };
                
                db.User.Add(userObject);
                db.SaveChanges();
            }
            

            MessageBox.Show("Пользователь успешно зарегистрирован!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

            // Очистка полей
            txtbxLog.Clear();
            passBxFrst.Clear();
            passBxScnd.Clear();
            comboBxRole.SelectedIndex = 0;
            txtbxFIO.Clear();

            // Возврат на страницу авторизации
            NavigationService?.Navigate(new AuthPage());
        }
    }
}