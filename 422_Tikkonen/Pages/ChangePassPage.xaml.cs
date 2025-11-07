using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
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
    /// Логика взаимодействия для ChangePassPage.xaml
    /// </summary>
    public partial class ChangePassPage : Page
    {
        public ChangePassPage()
        {
            InitializeComponent();
        }

        public static string GetHash(string password)
        {
            using (var hash = SHA1.Create())
            {
                return string.Concat(hash.ComputeHash(Encoding.UTF8.GetBytes(password)).Select(x => x.ToString("X2")));
            }
        }

        private void ButtonSaveChangePassword_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(PBOldPass.Password) || string.IsNullOrEmpty(PBNewPass.Password) || string.IsNullOrEmpty(PBNewTwoPass.Password) || string.IsNullOrEmpty(TextBoxLogin.Text))
            {
                MessageBox.Show("Все поля обязательны к заполнению!");
                return;
            }

            string hashedPass = GetHash(PBOldPass.Password);
            var user = Tikkonen_DB_PaymentEntities.GetContext().User.FirstOrDefault(u => u.Login == TextBoxLogin.Text && u.Password == hashedPass);

            if (user == null)
            {
                MessageBox.Show("Текущий пароль/Логин неверный!");
                return;
            }

            if (PBNewPass.Password.Length >= 6)
            {
                bool en = true;
                bool number = false;

                for (int i = 0; i < PBNewPass.Password.Length; i++)
                {
                    if (PBNewPass.Password[i] >= '0' && PBNewPass.Password[i] <= '9') number = true;
                    else if (!((PBNewPass.Password[i] >= 'A' && PBNewPass.Password[i] <= 'Z') || (PBNewPass.Password[i] >= 'a' && PBNewPass.Password[i] <= 'z'))) en = false;
                }

                if (!en)
                {
                    MessageBox.Show("Используйте только английскую раскладку!");
                    return;
                }
                else if (!number)
                {
                    MessageBox.Show("Добавьте хотя бы одну цифру!");
                    return;
                }

                if (en && number)
                {
                    if (PBNewPass.Password != PBNewTwoPass.Password)
                    {
                        MessageBox.Show("Пароли не совпадают!");
                    }
                    else
                    {
                        user.Password = GetHash(PBNewPass.Password);
                        Tikkonen_DB_PaymentEntities.GetContext().SaveChanges();
                        MessageBox.Show("Пароль успешно изменен!");
                        NavigationService?.Navigate(new AuthPage());
                    }
                }
            }
            else
            {
                MessageBox.Show("Пароль слишком короткий, должно быть минимум 6 символов!");
            }
        }
    }
}