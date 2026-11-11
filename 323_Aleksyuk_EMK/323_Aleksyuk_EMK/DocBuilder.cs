using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _323_Aleksyuk_EMK
{
    /// <summary>Тексты выходных документов: талон на приём, направление на анализы, рецепт.</summary>
    public static class DocBuilder
    {
        private const string Line = "------------------------------------------------------------";

        private static string Time(schedule s) { return s.time_start.ToString(@"hh\:mm"); }

        public static string BuildTicket(appointment a)
        {
            var s = a.schedule;
            var d = s != null ? s.doctor : null;
            var sb = new StringBuilder();
            sb.AppendLine("ТАЛОН НА ПРИЁМ № " + a.id);
            sb.AppendLine("Медицинский центр «ЭМК»");
            sb.AppendLine(Line);
            sb.AppendLine("Пациент:  " + (a.patient != null ? a.patient.fio : ""));
            sb.AppendLine("Врач:     " + (d != null ? d.fio : "не указан"));
            if (d != null && d.specialization != null)
                sb.AppendLine("Профиль:  " + d.specialization.name);
            if (s != null)
            {
                sb.AppendLine("Дата:     " + s.visit_date.ToString("dd.MM.yyyy"));
                sb.AppendLine("Время:    " + Time(s));
            }
            if (d != null) sb.AppendLine("Кабинет:  " + d.cabinet);
            sb.AppendLine("Статус:   " + a.status);
            sb.AppendLine(Line);
            sb.AppendLine("Талон выдан: " + DateTime.Now.ToString("dd.MM.yyyy HH:mm"));
            sb.AppendLine();
            sb.AppendLine("Просим прийти за 10 минут до начала приёма.");
            sb.AppendLine("Отменить или перенести запись можно в личном кабинете.");
            return sb.ToString();
        }

        private static void PatientBlock(StringBuilder sb, visit v)
        {
            var p = v.medical_record != null ? v.medical_record.patient : null;
            var s = v.appointment != null ? v.appointment.schedule : null;
            var d = s != null ? s.doctor : null;

            sb.AppendLine(Line);
            if (p != null)
            {
                sb.AppendLine("Пациент:        " + p.fio);
                sb.AppendLine("Дата рождения:  " + p.birth_date.ToString("dd.MM.yyyy"));
                sb.AppendLine("Полис ОМС:      " + p.policy_number);
            }
            if (s != null) sb.AppendLine("Дата приёма:    " + s.visit_date.ToString("dd.MM.yyyy") + " " + Time(s));
            sb.AppendLine("Врач:           " + (d != null ? d.fio : "не указан") +
                          (d != null && d.specialization != null ? " (" + d.specialization.name + ")" : ""));
            sb.AppendLine(Line);
        }

        public static string BuildLabReferral(visit v)
        {
            var sb = new StringBuilder();
            sb.AppendLine("НАПРАВЛЕНИЕ НА АНАЛИЗЫ");
            PatientBlock(sb, v);

            if (v.diagnosis.Count > 0)
            {
                sb.AppendLine("Диагноз:");
                foreach (var dg in v.diagnosis.OrderBy(x => x.icd10_code))
                    sb.AppendLine("  " + dg.icd10_code + " " + dg.name);
                sb.AppendLine();
            }

            sb.AppendLine("Назначены исследования:");
            if (v.lab_test.Count == 0) sb.AppendLine("  — не назначены");
            int i = 1;
            foreach (var t in v.lab_test.OrderBy(x => x.test_date))
                sb.AppendLine("  " + (i++) + ". " + t.name + "  (дата: " + t.test_date.ToString("dd.MM.yyyy") + ")");

            sb.AppendLine();
            sb.AppendLine("Подпись врача: ____________________");
            return sb.ToString();
        }

        public static string BuildPrescription(visit v)
        {
            var sb = new StringBuilder();
            sb.AppendLine("РЕЦЕПТ (НАЗНАЧЕНИЕ ЛЕЧЕНИЯ)");
            PatientBlock(sb, v);

            sb.AppendLine("Диагноз:");
            if (v.diagnosis.Count == 0) sb.AppendLine("  — не указан");
            foreach (var dg in v.diagnosis.OrderBy(x => x.icd10_code))
                sb.AppendLine("  " + dg.icd10_code + " " + dg.name);
            sb.AppendLine();

            sb.AppendLine("Назначения:");
            if (v.prescription.Count == 0) sb.AppendLine("  — не назначены");
            int i = 1;
            foreach (var p in v.prescription)
                sb.AppendLine("  " + (i++) + ". " + p.medicine +
                              (p.dosage != null ? " — " + p.dosage : "") +
                              (p.duration != null ? ", курс: " + p.duration : ""));

            sb.AppendLine();
            sb.AppendLine("Подпись врача: ____________________");
            return sb.ToString();
        }
    }
}