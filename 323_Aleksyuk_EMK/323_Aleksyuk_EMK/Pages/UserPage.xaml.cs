//using DurableTask.Core.Common;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.Windows;
//using System.Windows.Controls;
//using System.Windows.Data;
//using System.Windows.Documents;
//using System.Windows.Input;
//using System.Windows.Media;
//using System.Windows.Media.Imaging;
//using System.Windows.Navigation;
//using System.Windows.Shapes;

//namespace _323_Aleksyuk_EMK.Pages
//{
//    public partial class UserPage : Page
//{
//    public UserPage()
//    {
//        InitializeComponent();
//    }

//    private void Page_Loaded(object sender, RoutedEventArgs e)
//    {
//        var user = Core.CurrentUser;
//        if (user == null) { NavigationService?.Navigate(new AuthPage()); return; }

//        var ctx = Core.DB;


//        TbName.Text = user.fio;
//        TbInfo.Text = "Роль: " + user.role + "  |  Логин: " + user.login;
//            //PhotoHelper.Apply(PhotoEllipse, PhotoHint, user.photo);

//            int? patientId = user.patient_id;
//            DgAppointments.ItemsSource = Core.DB.appointment.ToList()
//                .Where(a => a.patient_id == patientId)
//                .OrderByDescending(a => a.schedule.visit_date)
//                .ToList();
//        }

//    //private void BtnAddPreem_Click(object sender, RoutedEventArgs e)
//    //{
//    //    //NavigationService?.Navigate(new AddAppointmentPage(null));
//    //}




//    private void BtnChangePass_Click(object sender, RoutedEventArgs e)
//    {
//        NavigationService?.Navigate(new ChangePassPage());
//    }

//    private void BtnLogout_Click(object sender, RoutedEventArgs e)
//    {
//            if (MessageBox.Show("Вы уверены, что хотите выйти из системы?", "Выход",
//                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes) 
//                NavigationService?.Navigate(new AuthPage());
//    }
//        //private void BtnAdd_Click(object sender, RoutedEventArgs e)
//        //    {
//        //        try
//        //        {
//        //            NavigationService?.Navigate(new AddAppointmentPageUser());
//        //        }
//        //        catch (Exception ex)
//        //        {
//        //            MessageBox.Show(ex.InnerException?.Message ?? ex.Message);
//        //        }
//        //    }
//        private void BtnAdd_Click(object sender, RoutedEventArgs e)
//        {
//            MessageBox.Show("Кнопка нажата");          // временно, для проверки
//            try
//            {
//                var page = new AddAppointmentPage(null);
//                var frame = ((MainWindow)Application.Current.MainWindow).MainFrame;
//                frame.Navigate(page);                  // переход напрямую через Frame окна
//            }
//            catch (Exception ex)
//            {
//                MessageBox.Show(ex.ToString());
//            }
//        }
//    }
//}
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace _323_Aleksyuk_EMK.Pages
{
    public partial class UserPage : Page
    {
        public UserPage()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            var user = Core.CurrentUser;
            if (user == null) { NavigationService?.Navigate(new AuthPage()); return; }

            TbName.Text = user.fio;
            TbInfo.Text = "Роль: " + user.role + "  |  Логин: " + user.login;

            int? patientId = user.patient_id;
            DgAppointments.ItemsSource = Core.DB.appointment.ToList()
                .Where(a => a.patient_id == patientId)
                .OrderByDescending(a => a.schedule.visit_date)
                .ToList();
        }

        private void BtnAddPreem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var page = new AddAppointmentPage(null);
                var frame = ((MainWindow)Application.Current.MainWindow).MainFrame;
                frame.Navigate(page);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.InnerException?.Message ?? ex.Message);
            }
        }

        private void BtnChangePass_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new ChangePassPage());
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Вы уверены, что хотите выйти из системы?", "Выход",
                    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                NavigationService?.Navigate(new AuthPage());
        }
    }
}
