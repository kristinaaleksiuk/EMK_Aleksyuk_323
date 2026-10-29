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
    /// <summary>
    /// Логика взаимодействия для VisitPage.xaml
    /// </summary>
    public partial class VisitPage : Page
    {
        private readonly appointment _appointment;
        private visit _visit;
        private bool _loading;

        public VisitPage(appointment selected)
        {
            InitializeComponent();
            _appointment = selected;

            _loading = true;
            TbPatient.Text = "Пациент: " + selected.patient.fio;
            TbDate.Text = "Приём: " + DateText(selected) + "   |   Статус: " + selected.status;
            DpTestDate.SelectedDate = DateTime.Today;

            // справочник диагнозов
            CbDiagnosis.ItemsSource = Core.DB.diagnosis.ToList()
                .OrderBy(d => d.icd10_code)
                .Select(d => new DiagnosisItem { Diagnosis = d })
                .ToList();

            try
            {
                _visit = GetOrCreateVisit();
                TbComplaints.Text = _visit.complaints;
                RefreshAll();
            }
            catch (Exception ex)
            {
                MessageBox.Show(Inner(ex));
            }
            _loading = false;
        }

        /// <summary>Строка выпадающего списка: «A00 — Название».</summary>
        private class DiagnosisItem
        {
            public diagnosis Diagnosis { get; set; }
            public string DisplayName => Diagnosis.icd10_code + " — " + Diagnosis.name;
        }

        private static string DateText(appointment a)
        {
            return a.schedule == null ? "" :
                a.schedule.visit_date.ToString("dd.MM.yyyy") + " " + a.schedule.time_start.ToString(@"hh\:mm");
        }

        private static string Inner(Exception ex)
        {
            while (ex.InnerException != null) ex = ex.InnerException;
            return ex.Message;
        }

        /// <summary>Карточка приёма (visit) + медицинская карта пациента (создаётся при первом приёме).</summary>
        private visit GetOrCreateVisit()
        {
            var ctx = Core.DB;
            var v = ctx.visit.FirstOrDefault(x => x.appointment_id == _appointment.id);
            if (v != null) return v;

            var record = ctx.medical_record.FirstOrDefault(r => r.patient_id == _appointment.patient_id);
            if (record == null)
            {
                record = new medical_record { patient_id = _appointment.patient_id, created_at = DateTime.Now };
                ctx.medical_record.Add(record);
            }

            v = new visit
            {
                appointment_id = _appointment.id,
                medical_record = record,
                visit_date = DateTime.Now
            };
            ctx.visit.Add(v);
            ctx.SaveChanges();
            return v;
        }

        private void RefreshAll()
        {
            DgTests.ItemsSource = null;
            DgTests.ItemsSource = _visit.lab_test.OrderBy(t => t.test_date).ToList();
            DgDiagnoses.ItemsSource = null;
            DgDiagnoses.ItemsSource = _visit.diagnosis.OrderBy(d => d.icd10_code).ToList();
            DgPrescriptions.ItemsSource = null;
            DgPrescriptions.ItemsSource = _visit.prescription.ToList();
            BuildReport();
        }

        private bool IsLocked()
        {
            if (_appointment.status == "Завершён")
            {
                MessageBox.Show("Приём завершён - данные ЭМК изменить нельзя.");
                return true;
            }
            return false;
        }

        private void TbComplaints_LostFocus(object sender, RoutedEventArgs e)
        {
            if (_loading || _visit == null || _appointment.status == "Завершён") return;
            try
            {
                _visit.complaints = TbComplaints.Text.Trim();
                Core.DB.SaveChanges();
            }
            catch (Exception ex) { MessageBox.Show(Inner(ex)); }
        }

        // ---------- 1. Анализы ----------
        private void BtnAddTest_Click(object sender, RoutedEventArgs e)
        {
            if (IsLocked()) return;
            string name = TbTestName.Text.Trim();
            if (name == "") { MessageBox.Show("Введите название анализа."); return; }
            if (DpTestDate.SelectedDate == null) { MessageBox.Show("Выберите дату анализа."); return; }

            try
            {
                var t = new lab_test
                {
                    visit_id = _visit.id,
                    name = name,
                    result = string.IsNullOrWhiteSpace(TbTestResult.Text) ? null : TbTestResult.Text.Trim(),
                    test_date = DpTestDate.SelectedDate.Value
                };
                Core.DB.lab_test.Add(t);
                Core.DB.SaveChanges();
                TbTestName.Clear(); TbTestResult.Clear();
                RefreshAll();
            }
            catch (Exception ex) { MessageBox.Show(Inner(ex)); }
        }

        private void BtnDelTest_Click(object sender, RoutedEventArgs e)
        {
            if (IsLocked()) return;
            var t = DgTests.SelectedItem as lab_test;
            if (t == null) { MessageBox.Show("Выберите анализ в таблице."); return; }
            try
            {
                Core.DB.lab_test.Remove(t);
                Core.DB.SaveChanges();
                RefreshAll();
            }
            catch (Exception ex) { MessageBox.Show(Inner(ex)); }
        }

        // ---------- 2. Диагноз ----------
        private void BtnAddDiagnosis_Click(object sender, RoutedEventArgs e)
        {
            if (IsLocked()) return;
            var item = CbDiagnosis.SelectedItem as DiagnosisItem;
            if (item == null) { MessageBox.Show("Выберите диагноз."); return; }
            diagnosis d = item.Diagnosis;
            if (_visit.diagnosis.Any(x => x.id == d.id)) { MessageBox.Show("Этот диагноз уже добавлен."); return; }

            try
            {
                _visit.diagnosis.Add(d);
                Core.DB.SaveChanges();
                RefreshAll();
            }
            catch (Exception ex) { MessageBox.Show(Inner(ex)); }
        }

        private void BtnDelDiagnosis_Click(object sender, RoutedEventArgs e)
        {
            if (IsLocked()) return;
            var d = DgDiagnoses.SelectedItem as diagnosis;
            if (d == null) { MessageBox.Show("Выберите диагноз в таблице."); return; }
            try
            {
                _visit.diagnosis.Remove(d);
                Core.DB.SaveChanges();
                RefreshAll();
            }
            catch (Exception ex) { MessageBox.Show(Inner(ex)); }
        }

        // ---------- 3. Лечение ----------
        private void BtnAddPrescription_Click(object sender, RoutedEventArgs e)
        {
            if (IsLocked()) return;
            string medicine = TbMedicine.Text.Trim();
            if (medicine == "") { MessageBox.Show("Введите название препарата."); return; }
            if (_visit.diagnosis.Count == 0) { MessageBox.Show("Сначала поставьте диагноз (шаг 2)."); return; }

            try
            {
                var p = new prescription
                {
                    visit_id = _visit.id,
                    medicine = medicine,
                    dosage = string.IsNullOrWhiteSpace(TbDosage.Text) ? null : TbDosage.Text.Trim(),
                    duration = string.IsNullOrWhiteSpace(TbDuration.Text) ? null : TbDuration.Text.Trim()
                };
                Core.DB.prescription.Add(p);
                Core.DB.SaveChanges();
                TbMedicine.Clear(); TbDosage.Clear(); TbDuration.Clear();
                RefreshAll();
            }
            catch (Exception ex) { MessageBox.Show(Inner(ex)); }
        }

        private void BtnDelPrescription_Click(object sender, RoutedEventArgs e)
        {
            if (IsLocked()) return;
            var p = DgPrescriptions.SelectedItem as prescription;
            if (p == null) { MessageBox.Show("Выберите назначение в таблице."); return; }
            try
            {
                Core.DB.prescription.Remove(p);
                Core.DB.SaveChanges();
                RefreshAll();
            }
            catch (Exception ex) { MessageBox.Show(Inner(ex)); }
        }

        // ---------- 4. Отчёт ----------
        private void BuildReport()
        {
            var sb = new StringBuilder();
            sb.AppendLine("ОТЧЁТ ПО ПРИЁМУ");
            sb.AppendLine("Пациент: " + _appointment.patient.fio);
            sb.AppendLine("Дата приёма: " + DateText(_appointment));
            sb.AppendLine("Номер карты: " + _visit.medical_record_id);
            sb.AppendLine();
            sb.AppendLine("Жалобы: " + (string.IsNullOrWhiteSpace(_visit.complaints) ? "не указаны" : _visit.complaints));
            sb.AppendLine();

            sb.AppendLine("Анализы:");
            if (_visit.lab_test.Count == 0) sb.AppendLine("  — нет");
            foreach (var t in _visit.lab_test.OrderBy(x => x.test_date))
                sb.AppendLine("  — " + t.name + ": " + (t.result ?? "результат не указан") + " (" + t.test_date.ToString("dd.MM.yyyy") + ")");
            sb.AppendLine();

            sb.AppendLine("Диагноз:");
            if (_visit.diagnosis.Count == 0) sb.AppendLine("  — не поставлен");
            foreach (var d in _visit.diagnosis.OrderBy(x => x.icd10_code))
                sb.AppendLine("  — " + d.icd10_code + " " + d.name);
            sb.AppendLine();

            sb.AppendLine("Назначенное лечение:");
            if (_visit.prescription.Count == 0) sb.AppendLine("  — не назначено");
            foreach (var p in _visit.prescription)
                sb.AppendLine("  — " + p.medicine + (p.dosage != null ? ", " + p.dosage : "") + (p.duration != null ? ", " + p.duration : ""));

            TbReport.Text = sb.ToString();
        }

        private void BtnRefreshReport_Click(object sender, RoutedEventArgs e)
        {
            RefreshAll();
        }

        private void BtnFinish_Click(object sender, RoutedEventArgs e)
        {
            if (IsLocked()) return;
            if (_visit.diagnosis.Count == 0)
            {
                MessageBox.Show("Для внесения в ЭМК необходимо поставить диагноз.");
                return;
            }

            if (MessageBox.Show("Внести данные приёма в ЭМК и завершить приём?", "Подтверждение",
                    MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                return;

            try
            {
                _visit.complaints = TbComplaints.Text.Trim();
                _appointment.status = "Завершён";
                Core.DB.SaveChanges();
                BuildReport();
                MessageBox.Show("Лечение внесено в ЭМК, приём завершён.");
                NavigationService?.GoBack();
            }
            catch (Exception ex) { MessageBox.Show(Inner(ex)); }
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.GoBack();
        }
    }
}
