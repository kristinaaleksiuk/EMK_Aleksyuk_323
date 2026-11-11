using System;
using System.Collections.Generic;
using System.Data.Entity;
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
    /// <summary>Администратор: просмотр, создание и удаление времени приёма врачей (регламент расписания).</summary>
    public partial class SchedulePage : Page
    {
        public SchedulePage()
        {
            InitializeComponent();

            var filterDoctors = new List<doctor> { new doctor { id = 0, fio = "Все врачи" } };
            filterDoctors.AddRange(Core.DB.doctor.ToList().OrderBy(d => d.fio));
            CbDoctor.ItemsSource = filterDoctors;
            CbDoctor.SelectedIndex = 0;

            var genDoctors = new List<doctor> { new doctor { id = 0, fio = "Все врачи" } };
            genDoctors.AddRange(Core.DB.doctor.ToList().OrderBy(d => d.fio));
            CbGenDoctor.ItemsSource = genDoctors;
            CbGenDoctor.SelectedIndex = 0;

            DpFrom.SelectedDate = DateTime.Today.AddDays(1);
            DpTo.SelectedDate = DateTime.Today.AddDays(7);
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateList();
        }

        private void Filter_Changed(object sender, EventArgs e) { UpdateList(); }

        private void UpdateList()
        {
            if (!IsInitialized || DgSlots == null) return;

            var list = Core.DB.schedule.ToList().AsEnumerable();

            var doc = CbDoctor.SelectedItem as doctor;
            if (doc != null && doc.id != 0)
                list = list.Where(s => s.doctor_id == doc.id);

            if (DpDate.SelectedDate.HasValue)
                list = list.Where(s => s.visit_date.Date == DpDate.SelectedDate.Value.Date);

            if (ChkUpcoming.IsChecked == true)
                list = list.Where(s => s.visit_date.Date >= DateTime.Today);

            var status = (CbStatus.SelectedItem as ComboBoxItem)?.Content.ToString();
            if (!string.IsNullOrEmpty(status) && status != "Все")
                list = list.Where(s => s.status == status);

            DgSlots.ItemsSource = list
                .OrderBy(s => s.visit_date).ThenBy(s => s.time_start).ThenBy(s => s.doctor_id)
                .ToList();
        }

        private void BtnReset_Click(object sender, RoutedEventArgs e)
        {
            CbDoctor.SelectedIndex = 0;
            DpDate.SelectedDate = null;
            CbStatus.SelectedIndex = 0;
            ChkUpcoming.IsChecked = true;
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            var s = DgSlots.SelectedItem as schedule;
            if (s == null) { MessageBox.Show("Выберите время в таблице."); return; }
            if (s.status != "Свободно")
            {
                MessageBox.Show("Удалить можно только свободное время.");
                return;
            }
            if (s.appointment != null && s.appointment.Count > 0)
            {
                MessageBox.Show("С этим временем связаны записи (в том числе отменённые). Удалять его нельзя, чтобы сохранить историю приёмов.");
                return;
            }

            if (MessageBox.Show("Удалить время " + s.visit_date.ToString("dd.MM.yyyy") + " " + s.time_start.ToString(@"hh\:mm") + "?",
                    "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                return;

            try
            {
                Core.DB.schedule.Remove(s);
                Core.DB.SaveChanges();
                UpdateList();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.InnerException?.InnerException?.Message ?? ex.Message);
            }
        }

        // создание времени приёма на период по регламенту (шаг, часы работы, выходные)
        private void BtnGenerate_Click(object sender, RoutedEventArgs e)
        {
            ErrorText.Text = "";

            if (!DpFrom.SelectedDate.HasValue || !DpTo.SelectedDate.HasValue)
            { ErrorText.Text = "Укажите период: с даты и по дату."; return; }
            DateTime dateFrom = DpFrom.SelectedDate.Value.Date;
            DateTime dateTo = DpTo.SelectedDate.Value.Date;
            if (dateTo < dateFrom) { ErrorText.Text = "Дата окончания раньше даты начала."; return; }
            if ((dateTo - dateFrom).TotalDays > 92) { ErrorText.Text = "Период не должен превышать 3 месяцев."; return; }

            TimeSpan start, end;
            if (!TimeSpan.TryParse(TbStart.Text.Trim(), out start) || !TimeSpan.TryParse(TbEnd.Text.Trim(), out end))
            { ErrorText.Text = "Введите время в формате ЧЧ:ММ, например 09:00."; return; }
            if (start < TimeSpan.Zero || end > TimeSpan.FromHours(24) || end <= start)
            { ErrorText.Text = "Конец приёма должен быть позже начала."; return; }

            int len;
            if (!int.TryParse(TbLen.Text.Trim(), out len) || len < 10 || len > 120)
            { ErrorText.Text = "Длительность приёма: от 10 до 120 минут."; return; }

            var selected = CbGenDoctor.SelectedItem as doctor;
            if (selected == null) { ErrorText.Text = "Выберите врача."; return; }
            var doctors = selected.id == 0 ? Core.DB.doctor.ToList() : new List<doctor> { selected };

            var step = TimeSpan.FromMinutes(len);
            int created = 0;

            try
            {
                foreach (var d in doctors)
                {
                    int did = d.id;
                    var existing = Core.DB.schedule
                        .Where(s => s.doctor_id == did && s.visit_date >= dateFrom && s.visit_date <= dateTo)
                        .ToList();

                    for (DateTime day = dateFrom; day <= dateTo; day = day.AddDays(1))
                    {
                        if (ChkNoWeekends.IsChecked == true &&
                            (day.DayOfWeek == DayOfWeek.Saturday || day.DayOfWeek == DayOfWeek.Sunday))
                            continue;

                        for (TimeSpan t = start; t + step <= end; t += step)
                        {
                            TimeSpan tEnd = t + step;
                            DateTime currentDay = day;
                            TimeSpan currentStart = t;

                            // не создаём время, пересекающееся с уже существующим
                            if (existing.Any(s => s.visit_date.Date == currentDay &&
                                                  s.time_start < tEnd && s.time_end > currentStart))
                                continue;

                            var slot = new schedule
                            {
                                doctor_id = did,
                                visit_date = day,
                                time_start = t,
                                time_end = tEnd,
                                status = "Свободно"
                            };
                            Core.DB.schedule.Add(slot);
                            existing.Add(slot);
                            created++;
                        }
                    }
                }

                Core.DB.SaveChanges();
            }
            catch (Exception ex)
            {
                foreach (var entry in Core.DB.ChangeTracker.Entries<schedule>()
                             .Where(x => x.State == EntityState.Added).ToList())
                    entry.State = EntityState.Detached;
                ErrorText.Text = "Не удалось создать время: " + (ex.InnerException?.InnerException?.Message ?? ex.Message);
                return;
            }

            UpdateList();
            MessageBox.Show(created == 0
                ? "Новых записей не создано: на выбранный период время уже есть."
                : "Создано записей времени приёма: " + created);
        }
    }
}