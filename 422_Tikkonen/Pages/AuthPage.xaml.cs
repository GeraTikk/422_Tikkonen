using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace _422_Tikkonen.Pages
{
    public partial class AuthPage : Page
    {
        private int failedAttempts = 0;

        public AuthPage()
        {
            InitializeComponent();
        }

        // Метод для хеширования пароля
        public static string GetHash(string password)
        {
            using (var hash = SHA1.Create())
            {
                return string.Concat(hash.ComputeHash(Encoding.UTF8.GetBytes(password))
                    .Select(x => x.ToString("X2")));
            }
        }

        private void ButtonEnter_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(TextBoxLogin.Text) || string.IsNullOrEmpty(PasswordBox.Password))
            {
                MessageBox.Show("Введите логин и пароль");
                return;
            }

            string hashedPassword = GetHash(PasswordBox.Password);

            using (var db = new Tikkonen_DB_PaymentEntities())
            {
                var user = db.User.AsNoTracking().FirstOrDefault(u => u.Login == TextBoxLogin.Text && u.Password == hashedPassword);

                if (user == null)
                {
                    MessageBox.Show("Пользователь с такими данными не найден!");
                    failedAttempts++;

                    if (failedAttempts >= 3)
                    {
                        if (captchaBlock.Visibility != Visibility.Visible)
                        {
                            CaptchaSwitch();
                        }
                        CaptchaChange();
                    }
                    return;
                }
                else
                {
                    MessageBox.Show("Пользователь успешно авторизован!");

                    // Сохраняем информацию о пользователе
                    Application.Current.Properties["CurrentUser"] = user;
                    Application.Current.Properties["UserId"] = user.ID;
                    Application.Current.Properties["UserLogin"] = user.Login;
                    Application.Current.Properties["UserRole"] = user.Role;

                    // Навигация в зависимости от роли
                    switch (user.Role)
                    {
                        case "User":
                            NavigationService?.Navigate(new UserPage());
                            break;
                        case "Admin":
                            NavigationService?.Navigate(new AdminPage());
                            break;
                        default:
                            NavigationService?.Navigate(new UserPage());
                            break;
                    }
                }
            }
        }

        private void ButtonReg_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new Pages.RegPage());
        }

        private void ButtonChangePassword_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new ChangePassPage());
        }

        // Метод включения/отключения капчи
        public void CaptchaSwitch()
        {
            switch (captchaBlock.Visibility)
            {
                case Visibility.Visible:
                    // Скрываем капчу, показываем форму авторизации
                    TextBoxLogin.Clear();
                    PasswordBox.Clear();
                    captchaInput.Clear();

                    captchaBlock.Visibility = Visibility.Hidden;
                    captchaInput.Visibility = Visibility.Hidden;
                    submitCaptcha.Visibility = Visibility.Hidden;

                    TextBoxLogin.Visibility = Visibility.Visible;
                    PasswordBox.Visibility = Visibility.Visible;
                    ButtonEnter.Visibility = Visibility.Visible;
                    ButtonReg.Visibility = Visibility.Visible;
                    ButtonChangePassword.Visibility = Visibility.Visible;
                    return;

                case Visibility.Hidden:
                    // Показываем капчу, скрываем форму авторизации
                    CaptchaChange();
                    captchaBlock.Visibility = Visibility.Visible;
                    captchaInput.Visibility = Visibility.Visible;
                    submitCaptcha.Visibility = Visibility.Visible;

                    TextBoxLogin.Visibility = Visibility.Hidden;
                    PasswordBox.Visibility = Visibility.Hidden;
                    ButtonEnter.Visibility = Visibility.Hidden;
                    ButtonReg.Visibility = Visibility.Hidden;
                    ButtonChangePassword.Visibility = Visibility.Hidden;
                    return;
            }
        }

        // Код обновления капчи
        public void CaptchaChange()
        {
            String allowchar = " ";
            allowchar = "A,B,C,D,E,F,G,H,I,J,K,L,M,N,O,P,Q,R,S,T,U,V,W,X,Y,Z";
            allowchar += "a,b,c,d,e,f,g,h,i,j,k,l,m,n,o,p,q,r,s,t,u,v,w,y,z";
            allowchar += "1,2,3,4,5,6,7,8,9,0";
            char[] a = { ',' };
            String[] ar = allowchar.Split(a);
            String pwd = "";
            string temp = "";
            Random r = new Random();

            for (int i = 0; i < 6; i++)
            {
                temp = ar[(r.Next(0, ar.Length))];
                pwd += temp;
            }
            captcha.Text = pwd;
        }

        // Обработка подтверждения ввода капчи
        private void submitCaptcha_Click(object sender, RoutedEventArgs e)
        {
            if (captchaInput.Text != captcha.Text)
            {
                MessageBox.Show("Неверно введена капча", "Ошибка");
                CaptchaChange();
            }
            else
            {
                MessageBox.Show("Капча введена успешно, можете продолжить авторизацию", "Успех");
                CaptchaSwitch();
                failedAttempts = 0;
            }
        }

        // Запрет Копирования/Вырезки/Вставки капчи
        private void textBox_PreviewExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Command == ApplicationCommands.Copy ||
                e.Command == ApplicationCommands.Cut ||
                e.Command == ApplicationCommands.Paste)
            {
                e.Handled = true;
            }
        }
    }
}