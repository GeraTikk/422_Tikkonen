using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace _422_Tikkonen
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Загружаем страницу авторизации при запуске
            MainFrame.Navigate(new Pages.AuthPage());
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Таймер для обновления времени каждую секунду
            var timer = new System.Windows.Threading.DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, 1);
            timer.IsEnabled = true;
            timer.Tick += (o, t) => { DateTimeNow.Text = DateTime.Now.ToString(); };
            timer.Start();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (MainFrame.CanGoBack)
            {
                MainFrame.GoBack();
            }
            else
            {
                MessageBoxResult result = MessageBox.Show("Вы действительно хотите выйти?",
                    "Подтверждение выхода",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    Application.Current.Shutdown();
                }
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Вы действительно хотите выйти?",
                "Подтверждение выхода",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);
            if (result == MessageBoxResult.No)
            {
                e.Cancel = true;
            }
        }

        private void ThemeSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ThemeSelector.SelectedItem is ComboBoxItem selectedItem && selectedItem.Tag != null)
            {
                string dictionaryPath = selectedItem.Tag.ToString();
                if (!string.IsNullOrEmpty(dictionaryPath))
                {
                    try
                    {
                        // Загружаем и применяем словарь ресурсов
                        var uri = new Uri(dictionaryPath, UriKind.Relative);
                        ResourceDictionary resourceDict = Application.LoadComponent(uri) as ResourceDictionary;
                        Application.Current.Resources.Clear();
                        Application.Current.Resources.MergedDictionaries.Add(resourceDict);

                        // Принудительно устанавливаем цвета основываясь на выбранной теме
                        if (dictionaryPath.Contains("DictionaryGreen.xaml")) // Тёмная тема
                        {
                            this.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#1A1F2B");
                        }
                        else // Светлая тема
                        {
                            this.Background = Brushes.AliceBlue;
                        
                        }

                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка загрузки темы: {ex.Message}");
                    }
                }
            }
        }
    }
}