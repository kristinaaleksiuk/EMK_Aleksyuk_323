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
using static System.Collections.Specialized.BitVector32;

namespace _323_Aleksyuk_EMK.Pages
{
    /// <summary>Шаг 10: форма создания и изменения приёма.
    /// Пациент - из таблицы Patient, время - строки Schedule со статусом «Свободно».</summary>
    public partial class AddAppointmentPage : Page
    {
        private readonly appointment _current;     // null - новый приём

        public AddAppointmentPage(appointment selected)
        {
            InitializeComponent();
            _current = selected;
            var ctx = Entities.GetContext();

            // Пациенты: администратор видит всех, пользователь - только себя
            if (session.IsAdmin)
            {
                CbPatient.ItemsSource = ctx.patient.ToList().OrderBy(p => p.fio).ToList();
            }
            else
            {
                CbPatient.ItemsSource = new List<patient> { Session.GetOrCreatePatient() };
                CbPatient.SelectedIndex = 0;
                CbPatient.IsEnabled = false;
                CbStatus.IsEnabled = false;          // обычный пользователь только записывается
            }

            // Свободные слоты (+ текущий слот редактируемого приёма)
            var now = DateTime.Now;
            int currentSlotId = _current != null ? _current.schedule_id : 0;
            CbSlot.ItemsSource = ctx.Schedules.ToList()
                .Where(s => (s.Status == "Свободно" && s.SlotTime >= now) || s.id == currentSlotId)
                .OrderBy(s => s.SlotTime)
                .ToList();

            CbStatus.SelectedIndex = 0;

            if (_current != null)
            {
                TbTitle.Text = "Изменение приёма";
                CbPatient.SelectedItem = ((IEnumerable<patient>)CbPatient.ItemsSource).FirstOrDefault(p => p.id == _current.patient);
                CbSlot.SelectedItem = ((IEnumerable<schedule>)CbSlot.ItemsSource).FirstOrDefault(s => s.id == _current.schedule_id);
                foreach (ComboBoxItem item in CbStatus.Items)
                    if (item.Content.ToString() == _current.status) CbStatus.SelectedItem = item;
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            var patient = CbPatient.SelectedItem as patient;
            var slot = CbSlot.SelectedItem as schedule;
            var statusItem = CbStatus.SelectedItem as ComboBoxItem;

            if (patient == null || slot == null || statusItem == null)
            {
                ErrorText.Text = "Выберите пациента и время приёма";
                return;
            }

            string status = statusItem.Content.ToString();
            var ctx = Entities.GetContext();

            try
            {
                if (_current != null && _current.schedule_id != slot.id)
                {
                    // перенос приёма: прежний слот освобождается
                    var oldSlot = ctx.Schedules.Find(_current.schedule_id);
                    if (oldSlot != null) oldSlot.Status = "Свободно";
                }

                appointment appointment = _current;
                if (appointment == null)
                {
                    appointment = new appointment();
                    ctx.appointment.Add(appointment);
                }

                appointment.patient = patient;
                appointment.schedule = slot;
                appointment.status = status;
                slot.status = status == "Отменён" ? "Свободно" : "Занято";

                ctx.SaveChanges();
                MessageBox.Show("Данные успешно сохранены!");
                NavigationService?.GoBack();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.GoBack();
        }
    }
}