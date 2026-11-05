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
        private List<doctor> _allDoctors;              // все врачи (для фильтра по специализации)     

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
                // пользователь записывает себя: строка Пациент скрыта
                patients.Add(Core.EnsurePatient(Core.CurrentUser));
                PatientPanel.Visibility = Visibility.Collapsed;
                CbStatus.IsEnabled = false;
            }
            CbPatient.ItemsSource = patients;
            if (patients.Count == 1) CbPatient.SelectedIndex = 0;

            _allDoctors = Core.DB.doctor.ToList().OrderBy(d => d.fio).ToList();
            CbDoctor.ItemsSource = _allDoctors;

            // фильтр: «Все специализации» + справочник специализаций
            var specs = new List<specialization> { new specialization { id = 0, name = "Все специализации" } };
            specs.AddRange(Core.DB.specialization.ToList().OrderBy(x => x.name));
            CbSpec.ItemsSource = specs;
            CbSpec.SelectedIndex = 0;

            if (_current != null)
            {
                TbTitle.Text = "Изменение приёма";
                CbPatient.SelectedItem = patients.FirstOrDefault(p => p.id == _current.patient_id);
                var curDoctor = _allDoctors.FirstOrDefault(d => d.id == _current.schedule.doctor_id);
                if (curDoctor != null)
                    CbSpec.SelectedItem = specs.FirstOrDefault(x => x.id == curDoctor.specialization_id);
                CbDoctor.SelectedItem = curDoctor;
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

        // поиск врача по специализации
        private void CbSpec_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_allDoctors == null) return;
            var sp = CbSpec.SelectedItem as specialization;
            CbDoctor.ItemsSource = (sp == null || sp.id == 0)
                ? _allDoctors
                : _allDoctors.Where(d => d.specialization_id == sp.id).ToList();
            CbSlot.ItemsSource = null;
        }

        private void CbDoctor_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadSlots();
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            patient pat;
            if (Core.CurrentUser.role == "Admin")
                pat = CbPatient.SelectedItem as patient;          // админ выбирает пациента из списка
            else
                pat = Core.EnsurePatient(Core.CurrentUser);       // пациент записывает сам себя
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