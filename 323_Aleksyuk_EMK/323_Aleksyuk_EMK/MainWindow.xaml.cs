using _323_Aleksyuk_EMK.Pages;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Threading;

namespace _323_Aleksyuk_EMK
{
    public partial class MainWindow : Window
    {
        private bool _alternativeTheme;

        public MainWindow()
        {
            InitializeComponent();
            MainFrame.Navigated += MainFrame_Navigated;
            MainFrame.Navigate(new AuthPage());          // первая страница приложения
        }

        // Заголовок окна меняется в зависимости от открытой страницы (Page.Title)
        private void MainFrame_Navigated(object sender, NavigationEventArgs e)
        {
            // Вход в систему - начало сеанса: журнал навигации очищаем
            if (e.Content is AuthPage)
                while (MainFrame.CanGoBack)
                    MainFrame.RemoveBackEntry();

            BackButton.Visibility = MainFrame.CanGoBack ? Visibility.Visible : Visibility.Hidden;

            if (e.Content is Page page)
                TitleText.Text = page.Title;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (MainFrame.CanGoBack)
                MainFrame.GoBack();
        }

        // Шаг 2, п. 5: текущие дата и время в нижней панели
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DateTimeNow.Text = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss");
            var timer = new System.Windows.Threading.DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, 1);
            timer.Tick += (o, t) => { DateTimeNow.Text = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"); };
            timer.Start();
        }

        // Шаг 2, п. 8: выход из приложения с подтверждением
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (MessageBox.Show("Вы уверены, что хотите закрыть окно?", "Message",
                    MessageBoxButton.YesNo) == MessageBoxResult.No)
                e.Cancel = true;
            else
                e.Cancel = false;
        }

        // Шаг 3, п. 5: переключение двух словарей стилей
        private void ThemeButton_Click(object sender, RoutedEventArgs e)
        {
            _alternativeTheme = !_alternativeTheme;

            // определяем путь к файлу ресурсов
            var uri = new Uri(_alternativeTheme ? "Dictionary2.xaml" : "Dictionary.xaml", UriKind.Relative);
            // загружаем словарь ресурсов
            ResourceDictionary resourceDict = Application.LoadComponent(uri) as ResourceDictionary;
            // очищаем коллекцию подключённых словарей и добавляем выбранный
            Application.Current.Resources.MergedDictionaries.Clear();
            Application.Current.Resources.MergedDictionaries.Add(resourceDict);
        }
    }
}