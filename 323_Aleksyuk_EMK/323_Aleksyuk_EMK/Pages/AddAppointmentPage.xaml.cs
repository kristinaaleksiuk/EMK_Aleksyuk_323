using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace _323_Aleksyuk_EMK.Pages
{
    public partial class AddAppointmentPage : Page
    {
        private readonly appointment _current;     

        public AddAppointmentPage(appointment selected)
        {
            InitializeComponent();
            _current = selected;

            var patients = new List<patient>();
            if (Core.CurrentUser.role == "Admin")
            {
                patients = Core.DB.patient.ToList().OrderBy(p => p.fio).ToList();
            }
            else
            {
                // пользователь записывает себя: строка «Пациент» скрыта
                patients.Add(Core.EnsurePatient(Core.CurrentUser));
                PatientPanel.Visibility = Visibility.Collapsed;
                CbStatus.IsEnabled = false;
            }
            CbPatient.ItemsSource = patients;
            if (patients.Count == 1) CbPatient.SelectedIndex = 0;

            var doctors = Core.DB.doctor.ToList().OrderBy(d => d.fio).ToList();
            CbDoctor.ItemsSource = doctors;

            if (_current != null)
            {
                TbTitle.Text = "Изменение приёма";
                CbPatient.SelectedItem = patients.FirstOrDefault(p => p.id == _current.patient_id);
                CbDoctor.SelectedItem = doctors.FirstOrDefault(d => d.id == _current.schedule.doctor_id);
                LoadSlots();
                CbSlot.SelectedItem = ((IEnumerable<schedule>)CbSlot.ItemsSource)
                    .FirstOrDefault(s => s.id == _current.schedule_id);
                foreach (ComboBoxItem item in CbStatus.Items)
                    if (item.Content.ToString() == _current.status) CbStatus.SelectedItem = item;
            }
            else
            {
                CbStatus.SelectedIndex = 0;
            }
        }

        // свободное время только у выбранного врача
        private void LoadSlots()
        {
            var selDoctor = CbDoctor.SelectedItem as doctor;
            if (selDoctor == null) { CbSlot.ItemsSource = null; return; }

            int currentSlotId = _current != null ? _current.schedule_id : 0;
            var slots = Core.DB.schedule.ToList()
                .Where(s => s.doctor_id == selDoctor.id &&
                            ((s.status == "Свободно" && s.visit_date.Date >= DateTime.Today) || s.id == currentSlotId))
                .OrderBy(s => s.visit_date).ThenBy(s => s.time_start)
                .ToList();

            CbSlot.ItemsSource = slots;
            ErrorText.Text = slots.Count == 0 ? "У выбранного врача нет свободного времени" : "";
        }

        private void CbDoctor_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadSlots();
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            var pat = CbPatient.SelectedItem as patient;
            var slot = CbSlot.SelectedItem as schedule;

            if (pat == null) { ErrorText.Text = "Выберите пациента"; return; }
            if (CbDoctor.SelectedItem == null) { ErrorText.Text = "Выберите врача"; return; }
            if (slot == null) { ErrorText.Text = "Выберите время приёма"; return; }

            string status = (CbStatus.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Запланирован";

            try
            {
                if (_current != null && _current.schedule_id != slot.id)
                {
                    // перенос приёма: прежний слот освобождается
                    var oldSlot = Core.DB.schedule.Find(_current.schedule_id);
                    if (oldSlot != null) oldSlot.status = "Свободно";
                }

                appointment appointment = _current;
                if (appointment == null)
                {
                    appointment = new appointment();
                    appointment.created_at = DateTime.Now;
                    Core.DB.appointment.Add(appointment);
                }

                appointment.patient = pat;
                appointment.schedule = slot;
                appointment.status = status;
                slot.status = status == "Отменён" ? "Свободно" : "Занято";

                Core.DB.SaveChanges();
                MessageBox.Show("Данные успешно сохранены!");
                NavigationService?.GoBack();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.InnerException?.InnerException?.Message ?? ex.Message);
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.GoBack();
        }
    }
}