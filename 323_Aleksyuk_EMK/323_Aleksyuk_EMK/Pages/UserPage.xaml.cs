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
            ShowReminder();
        }

        // напоминание о предстоящих приёмах (ближайшие 48 часов)
        private void ShowReminder()
        {
            int? pid = Core.CurrentUser.patient_id;
            var now = DateTime.Now;

            var soon = Core.DB.appointment.ToList()
                .Where(a => pid != null && a.patient_id == pid && a.status == "Запланирован" && a.schedule != null)
                .Select(a => new { A = a, At = a.schedule.visit_date.Date + a.schedule.time_start })
                .Where(x => x.At >= now && x.At <= now.AddHours(48))
                .OrderBy(x => x.At)
                .ToList();

            if (soon.Count == 0)
            {
                ReminderBorder.Visibility = Visibility.Collapsed;
                return;
            }

            var lines = soon.Select(x =>
            {
                string day = x.At.Date == DateTime.Today ? "Сегодня"
                           : x.At.Date == DateTime.Today.AddDays(1) ? "Завтра"
                           : x.At.ToString("dd.MM.yyyy");
                var doc = x.A.schedule.doctor;
                return day + " в " + x.At.ToString("HH:mm") + " — " + (doc != null ? doc.fio + ", каб. " + doc.cabinet : "врач не указан");
            });
            TbReminder.Text = "🔔 Напоминание о приёме:\n" + string.Join("\n", lines);
            ReminderBorder.Visibility = Visibility.Visible;
        }

        private void BtnMedCard_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new MedicalCardPage());
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
        private appointment SelectedAppointment()
        {
            var a = DgAppointments.SelectedItem as appointment;
            if (a == null) MessageBox.Show("Выберите приём в таблице.");
            return a;
        }

        private static DateTime StartOf(appointment a)
        {
            return a.schedule.visit_date.Date + a.schedule.time_start;
        }

        // отмена записи пациентом: время снова становится свободным
        private void BtnCancelApp_Click(object sender, RoutedEventArgs e)
        {
            var a = SelectedAppointment();
            if (a == null) return;
            if (a.status != "Запланирован") { MessageBox.Show("Отменить можно только запланированный приём."); return; }
            if (StartOf(a) < DateTime.Now) { MessageBox.Show("Этот приём уже прошёл."); return; }

            if (MessageBox.Show("Отменить запись к врачу «" + a.schedule.doctor.fio + "» на " +
                    StartOf(a).ToString("dd.MM.yyyy HH:mm") + "?", "Подтверждение",
                    MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                return;

            try
            {
                Core.CancelAppointment(a);
                UpdateList();
                ShowReminder();
            }
            catch (Exception ex)
            {
                Core.DB.Entry(a).Reload();
                MessageBox.Show(ex.InnerException?.InnerException?.Message ?? ex.Message);
            }
        }

        private void BtnReschedule_Click(object sender, RoutedEventArgs e)
        {
            var a = SelectedAppointment();
            if (a == null) return;
            if (a.status != "Запланирован") { MessageBox.Show("Перенести можно только запланированный приём."); return; }
            if (StartOf(a) < DateTime.Now) { MessageBox.Show("Этот приём уже прошёл."); return; }
            NavigationService?.Navigate(new AddAppointmentPage(a));
        }

        private void BtnTicket_Click(object sender, RoutedEventArgs e)
        {
            var a = SelectedAppointment();
            if (a == null) return;
            NavigationService?.Navigate(new PrintPage("Талон на приём", DocBuilder.BuildTicket(a), () => new UserPage()));
        }

    }

}

