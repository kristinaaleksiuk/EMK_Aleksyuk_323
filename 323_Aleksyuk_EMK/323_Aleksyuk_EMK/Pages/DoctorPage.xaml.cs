using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Providers.Entities;
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
    /// <summary>
    /// Логика взаимодействия для DoctorPage.xaml
    /// </summary>
    public partial class DoctorPage : Page
    {
        public DoctorPage()
        {
            InitializeComponent();
        }
    
     private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            var currentUser = Core.CurrentUser;
            if (currentUser == null) { NavigationService?.Navigate(new AuthPage()); return; }

            //var ctx = EMKEntities.GetContext();
            //ctx.Refresh();

            TbName.Text = currentUser.fio;
            TbInfo.Text = "Роль: " + currentUser.role + "  |  Логин: " + currentUser.login;
            //PhotoHelper.Apply(PhotoEllipse, PhotoHint, currentUser.photo);

            //_doctorId = currentUser.doctor_id;
            //if (_doctorId > 0)
            //{
            //    int did = _doctorId.Value;
            //    var card = ctx.doctor.FirstOrDefault(d => d.id == did);
            //    TbDoctorInfo.Text = card != null
            //        ? card.Specialty + "  |  Кабинет " + card.cabinet + "  |  Стаж: " + card.experience + " лет"
            //        : "";
            //}
            //else
            //{
            //    TbDoctorInfo.Text = "Учётная запись не привязана к карточке врача (обратитесь к администратору)";
            //}

            //UpdateList();
        }

        private void Filter_Changed(object sender, EventArgs e) { UpdateList(); }

        private void UpdateList()
        {
        //    if (!IsInitialized) return;

            //if (!_doctorId.HasValue)
            //{
            //    DgAppointments.ItemsSource = null;
            //    return;
            //}

            //int did = _doctorId.Value;
            //var list = Core.DB.appointment.ToList()
            //    .Where(a => a.schedule != null && a.schedule.doctor_id == did);

            // фильтр по статусу
        //    var status = ((ComboBoxItem)CbStatus.SelectedItem)?.Content.ToString();
        //    if (!string.IsNullOrEmpty(status) && status != "Все")
        //        list = list.Where(a => a.status == status);

        //    // фильтр по дате
        //    var period = ((ComboBoxItem)CbPeriod.SelectedItem)?.Content.ToString();
        //    if (period == "Сегодня")
        //        list = list.Where(a => a.Date.Date == DateTime.Today);
        //    else if (period == "Предстоящие")
        //        list = list.Where(a => a.Date >= DateTime.Now);

        //    DgAppointments.ItemsSource = list.OrderBy(a => a.Date).ToList();
        }

        private void BtnReset_Click(object sender, RoutedEventArgs e)
        {
            CbStatus.SelectedIndex = 0;
            CbPeriod.SelectedIndex = 0;
        }

        private void BtnVisit_Click(object sender, RoutedEventArgs e)
        {
            var selected = DgAppointments.SelectedItem as appointment;
            if (selected == null) { MessageBox.Show("Выберите приём в таблице."); return; }
            if (selected.status == "Отменён")
            {
                MessageBox.Show("Приём отменён - вести по нему ЭМК нельзя.");
                return;
            }
            // задачи врача: 1. Анализы -> 2. Диагноз -> 3. Лечение -> 4. Отчёт
            //NavigationService?.Navigate(new VisitPage(selected));
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            var selected = DgAppointments.SelectedItem as appointment;
            if (selected == null) { MessageBox.Show("Выберите приём в таблице."); return; }
            if (selected.status != "Запланирован")
            {
                MessageBox.Show("Отменить можно только запланированный приём.");
                return;
            }

            //if (MessageBox.Show("Отменить приём пациента «" + selected.PatientShort + "» (" + selected.DateText + ")?",
            //        "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
            //    return;

            try
            {
                var ctx = Core.DB;
                selected.status = "Отменён";
                var slot = ctx.schedule.Find(selected.schedule_id);
                if (slot != null) slot.status = "Свободно";      // время снова доступно для записи
                ctx.SaveChanges();
                UpdateList();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
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
