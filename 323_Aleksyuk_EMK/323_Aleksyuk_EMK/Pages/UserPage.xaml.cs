using DurableTask.Core.Common;
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

            UpdateList();
        }

        private void Filter_Changed(object sender, EventArgs e) { UpdateList(); }

        private void UpdateList()
        {
            
            if (!IsInitialized || Core.CurrentUser == null) return;

            int? patientId = Core.CurrentUser.patient_id;

           
            var list = Core.DB.appointment.ToList()
                .Where(a => a.patient_id == patientId && a.schedule != null);

           
            string q = TbSearch.Text.Trim().ToLower();
            if (q.Length > 0)
                list = list.Where(a =>
                    (a.schedule.doctor != null && (a.schedule.doctor.fio ?? "").ToLower().Contains(q)) ||
                    (a.schedule.doctor != null && (a.schedule.doctor.cabinet ?? "").ToLower().Contains(q)) ||
                    a.schedule.visit_date.ToString("dd.MM.yyyy").Contains(q));

            
            var status = (CbStatus.SelectedItem as ComboBoxItem)?.Content.ToString();
            if (!string.IsNullOrEmpty(status) && status != "Все")
                list = list.Where(a => a.status == status);

            
            switch (CbSort.SelectedIndex)
            {
                case 1:  // дата: сначала старые
                    list = list.OrderBy(a => a.schedule.visit_date).ThenBy(a => a.schedule.time_start);
                    break;
                case 2:  // врач А-Я
                    list = list.OrderBy(a => a.schedule.doctor?.fio);
                    break;
                case 3:  // врач Я-А
                    list = list.OrderByDescending(a => a.schedule.doctor?.fio);
                    break;
                case 4:  // статус
                    list = list.OrderBy(a => a.status).ThenByDescending(a => a.schedule.visit_date);
                    break;
                default: // дата: сначала новые
                    list = list.OrderByDescending(a => a.schedule.visit_date).ThenByDescending(a => a.schedule.time_start);
                    break;
            }

            DgAppointments.ItemsSource = list.ToList();
        }

        private void BtnReset_Click(object sender, RoutedEventArgs e)
        {
            TbSearch.Text = "";
            CbStatus.SelectedIndex = 0;
            CbSort.SelectedIndex = 0;
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
