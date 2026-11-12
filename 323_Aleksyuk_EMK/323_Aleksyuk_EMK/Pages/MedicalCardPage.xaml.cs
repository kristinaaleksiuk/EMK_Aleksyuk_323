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
    /// Логика взаимодействия для MedicalCardPage.xaml
    /// </summary>
    public partial class MedicalCardPage : Page
    {
        public MedicalCardPage()
        {
            InitializeComponent();
        }
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            var u = Core.CurrentUser;
            if (u == null) { NavigationService?.Navigate(new AuthPage()); return; }

            if (u.patient_id == null)
            {
                TbPatient.Text = u.fio;
                TbCard.Text = "Медицинская карта пока пуста: вы ещё не были на приёме.";
                return;
            }

            try
            {
                var p = Core.DB.patient.Find(u.patient_id);
                TbPatient.Text = p == null ? u.fio
                    : p.fio + "   |   Дата рождения: " + p.birth_date.ToString("dd.MM.yyyy") +
                     "   |   Пол: " + p.gender + "   |   Полис: " + p.policy_number + (string.IsNullOrWhiteSpace(p.guardian_fio) 
                     ? "" : "   |   Законный представитель: " + p.guardian_fio);
                TbCard.Text = BuildHistory(u.patient_id.Value, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.InnerException?.Message ?? ex.Message);
            }
        }

        /// <summary>
        /// Текстовая история завершённых приёмов пациента (новые сверху).
        /// Используется и на странице пациента, и во вкладке «История» у врача.
        /// </summary>
        public static string BuildHistory(int patientId, int? excludeVisitId)
        {
            var visits = Core.DB.visit.ToList()
                .Where(v => v.medical_record != null && v.medical_record.patient_id == patientId
                            && v.appointment != null && v.appointment.status == "Завершён"
                            && v.appointment.schedule != null
                            && (excludeVisitId == null || v.id != excludeVisitId.Value))
                .OrderByDescending(v => v.appointment.schedule.visit_date)
                .ThenByDescending(v => v.appointment.schedule.time_start)
                .ToList();

            if (visits.Count == 0)
                return "Завершённых приёмов пока нет.";

            var sb = new StringBuilder();
            foreach (var v in visits)
            {
                var sch = v.appointment.schedule;
                var doc = sch.doctor;
                string spec = doc != null && doc.specialization != null ? doc.specialization.name : "";

                sb.AppendLine("━━ Приём " + sch.visit_date.ToString("dd.MM.yyyy") + " " + sch.time_start.ToString(@"hh\:mm") + " ━━");
                sb.AppendLine("Врач: " + (doc != null ? doc.fio : "не указан") + (spec != "" ? " (" + spec + ")" : ""));
                sb.AppendLine("Жалобы: " + (string.IsNullOrWhiteSpace(v.complaints) ? "не указаны" : v.complaints));
                sb.AppendLine("Анамнез: " + (string.IsNullOrWhiteSpace(v.anamnesis) ? "не указан" : v.anamnesis));
                sb.AppendLine("Анализы:");
                if (v.lab_test.Count == 0) sb.AppendLine("  — нет");
                foreach (var t in v.lab_test.OrderBy(x => x.test_date))
                    sb.AppendLine("  — " + t.name + ": " + (t.result ?? "результат не указан") + " (" + t.test_date.ToString("dd.MM.yyyy") + ")");

                sb.AppendLine("Диагноз:");
                if (v.diagnosis.Count == 0) sb.AppendLine("  — не указан");
                foreach (var d in v.diagnosis.OrderBy(x => x.icd10_code))
                    sb.AppendLine("  — " + d.icd10_code + " " + d.name);

                sb.AppendLine("Лечение:");
                if (v.prescription.Count == 0) sb.AppendLine("  — не назначено");
                foreach (var pr in v.prescription)
                    sb.AppendLine("  — " + pr.medicine + (pr.dosage != null ? ", " + pr.dosage : "") + (pr.duration != null ? ", " + pr.duration : ""));

                sb.AppendLine();
            }
            return sb.ToString();
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.GoBack();
        }
    }
}
