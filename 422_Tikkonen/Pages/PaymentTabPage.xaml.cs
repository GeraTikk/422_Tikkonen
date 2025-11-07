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
    /// Логика взаимодействия для PaymentTabPage.xaml
    /// </summary>
    public partial class PaymentTabPage : Page
    {
        public PaymentTabPage()
        {
            InitializeComponent();
            DataGridPayment.ItemsSource = Tikkonen_DB_PaymentEntities.GetContext().Payment.ToList();
            this.IsVisibleChanged += Page_IsVisibleChanged;
        }

        private void Page_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (Visibility == Visibility.Visible)
            {
                try
                {
                    var context = Tikkonen_DB_PaymentEntities.GetContext();

                    // Фильтруем записи - Reload только для существующих в БД сущностей
                    var entries = context.ChangeTracker.Entries()
                        .Where(x => x.State != System.Data.Entity.EntityState.Added &&
                                   x.State != System.Data.Entity.EntityState.Detached)
                        .ToList();

                    entries.ForEach(x => x.Reload());

                    DataGridPayment.ItemsSource = context.Payment.ToList();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка обновления данных: {ex.Message}");
                }
            }
        }
        private void ButtonAdd_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new AddPaymentPage(null));
        }

        private void ButtonDel_Click(object sender, RoutedEventArgs e)
        {
            var paymentForRemoving = DataGridPayment.SelectedItems.Cast<Payment>().ToList();

            if (MessageBox.Show($"Вы точно хотите удалить записи в количестве {paymentForRemoving.Count()} элементов?",
                "Внимание", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    Tikkonen_DB_PaymentEntities.GetContext().Payment.RemoveRange(paymentForRemoving);
                    Tikkonen_DB_PaymentEntities.GetContext().SaveChanges();
                    MessageBox.Show("Данные успешно удалены!");
                    DataGridPayment.ItemsSource = Tikkonen_DB_PaymentEntities.GetContext().Payment.ToList();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message.ToString());
                }
            }
        }

        private void ButtonEdit_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new AddPaymentPage((sender as Button).DataContext as Payment));
        }
    }
}