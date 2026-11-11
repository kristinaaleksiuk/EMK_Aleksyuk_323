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
    /// <summary>Администратор: список записей на приём, изменение, отмена и печать талона.</summary>
    public partial class AppointmentsPage : Page
    {
        public AppointmentsPage()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateList();
        }

        private void Filter_Changed(object sender, EventArgs e) { UpdateList(); }

        private void UpdateList()
        {
            if (!IsInitialized) return;

            var list = Core.DB.appointment.ToList()
                .Where(a => a.schedule != null && a.patient != null);

            string q = TbSearch.Text.Trim().ToLower();
            if (q.Length > 0)
                list = list.Where(a => a.patient.fio.ToLower().Contains(q) ||
                                       (a.schedule.doctor != null && a.schedule.doctor.fio.ToLower().Contains(q)));

            var status = (CbStatus.SelectedItem as ComboBoxItem)?.Content.ToString();
            if (!string.IsNullOrEmpty(status) && status != "Все")
                list = list.Where(a => a.status == status);

            DgAppointments.ItemsSource = list
                .OrderByDescending(a => a.schedule.visit_date)
                .ThenByDescending(a => a.schedule.time_start)
                .ToList();
        }

        private void BtnReset_Click(object sender, RoutedEventArgs e)
        {
            TbSearch.Text = "";
            CbStatus.SelectedIndex = 0;
        }

        private appointment Selected()
        {
            var a = DgAppointments.SelectedItem as appointment;
            if (a == null) MessageBox.Show("Выберите запись в таблице.");
            return a;
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new AddAppointmentPage(null));
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            var a = Selected();
            if (a == null) return;
            if (a.status != "Запланирован")
            {
                MessageBox.Show("Изменить можно только запланированный приём.");
                return;
            }
            NavigationService?.Navigate(new AddAppointmentPage(a));
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            var a = Selected();
            if (a == null) return;
            if (a.status != "Запланирован")
            {
                MessageBox.Show("Отменить можно только запланированный приём.");
                return;
            }

            if (MessageBox.Show("Отменить запись пациента «" + a.patient.fio + "» на " +
                    a.schedule.visit_date.ToString("dd.MM.yyyy") + " " + a.schedule.time_start.ToString(@"hh\:mm") + "?",
                    "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                return;

            try
            {
                Core.CancelAppointment(a);
                UpdateList();
            }
            catch (Exception ex)
            {
                Core.DB.Entry(a).Reload();
                MessageBox.Show(ex.InnerException?.InnerException?.Message ?? ex.Message);
            }
        }

        private void BtnTicket_Click(object sender, RoutedEventArgs e)
        {
            var a = Selected();
            if (a == null) return;
            NavigationService?.Navigate(new PrintPage("Талон на приём", DocBuilder.BuildTicket(a), () => new AppointmentsPage()));
        }
    }
}
