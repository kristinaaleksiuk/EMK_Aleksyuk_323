using System.Windows.Controls;
using _323_Aleksyuk_EMK.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace _323_Aleksyuk_EMK
{
    public class Core
    {
        public static EMKEntities Context = new EMKEntities();
        public static EMKEntities DB { get; } = new EMKEntities();
        public static EMKEntities EMKEntities => DB;

        public static user CurrentUser { get; set; }

        // ---------- пароли (152-ФЗ): случайная соль + PBKDF2, формат v2$итерации$соль$хэш ----------
        private const string HashPrefix = "v2";
        private const int HashIterations = 10000;

        private static byte[] Pbkdf2(string password, byte[] salt, int iterations)
        {
            using (var d = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256))
                return d.GetBytes(32);
        }

        /// <summary>Хэш нового пароля со случайной солью.</summary>
        public static string GetHash(string input)
        {
            var salt = new byte[16];
            using (var rng = RandomNumberGenerator.Create()) rng.GetBytes(salt);
            return HashPrefix + "$" + HashIterations + "$" + Convert.ToBase64String(salt) + "$" +
                   Convert.ToBase64String(Pbkdf2(input, salt, HashIterations));
        }

        private static string LegacySha1(string input)
        {
            using (var sha = SHA1.Create())
            {
                byte[] hash = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
                return string.Concat(hash.Select(b => b.ToString("X2")));
            }
        }

        /// <summary>Проверка пароля: новый формат и старый SHA-1 (для уже созданных учётных записей).</summary>
        public static bool VerifyPassword(string input, string stored)
        {
            if (string.IsNullOrEmpty(input) || string.IsNullOrEmpty(stored)) return false;

            if (stored.StartsWith(HashPrefix + "$"))
            {
                var parts = stored.Split('$');
                int iterations;
                if (parts.Length != 4 || !int.TryParse(parts[1], out iterations)) return false;
                try
                {
                    byte[] salt = Convert.FromBase64String(parts[2]);
                    byte[] expected = Convert.FromBase64String(parts[3]);
                    byte[] actual = Pbkdf2(input, salt, iterations);
                    if (actual.Length != expected.Length) return false;
                    int diff = 0;
                    for (int i = 0; i < actual.Length; i++) diff |= actual[i] ^ expected[i];
                    return diff == 0;
                }
                catch (FormatException) { return false; }
            }
            return string.Equals(LegacySha1(input), stored, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>true, если пароль хранится в старом (слабом) формате и его нужно пересохранить.</summary>
        public static bool NeedsRehash(string stored)
        {
            return !string.IsNullOrEmpty(stored) && !stored.StartsWith(HashPrefix + "$");
        }

        public static string ValidatePassword(string password)
        {
            if (string.IsNullOrEmpty(password))
                return "Пароль не может быть пустым!";
            if (password.Length < 6)
                return "Пароль слишком короткий, должно быть минимум 6 символов!";
            if (password.Any(c => char.IsLetter(c) && !((c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z'))))
                return "Используйте только английскую раскладку!";
            if (!password.Any(char.IsDigit))
                return "Добавьте хотя бы одну цифру!";
            return null;
        }
        /// <summary>Обновляет приёмы из базы (если статус изменён в другом окне программы или в БД).</summary>
        public static void RefreshAppointments()
        {
            try
            {
                var oc = ((System.Data.Entity.Infrastructure.IObjectContextAdapter)DB).ObjectContext;
                var list = DB.appointment.Local.ToList();
                if (list.Count > 0)
                    oc.Refresh(System.Data.Entity.Core.Objects.RefreshMode.StoreWins, list);
            }
            catch { }
        }
        // ---------- вспомогательные ----------
        public static int Age(DateTime birth)
        {
            var today = DateTime.Today;
            int age = today.Year - birth.Year;
            if (birth.Date > today.AddYears(-age)) age--;
            return age;
        }

        private static readonly Random _rnd = new Random();

        /// <summary>Уникальный 16-значный номер полиса для автоматически созданной карточки.</summary>
        public static string GeneratePolicyNumber()
        {
            string num;
            do
            {
                num = string.Concat(Enumerable.Range(0, 16).Select(_ => _rnd.Next(10).ToString()));
            }
            while (DB.patient.Any(x => x.policy_number == num));
            return num;
        }

        /// <summary>Главная страница по роли текущего пользователя.</summary>
        public static Page HomePage()
        {
            if (CurrentUser == null) return new AuthPage();
            if (CurrentUser.role == "Admin") return new AdminPage();
            if (CurrentUser.role == "Doctor") return new DoctorPage();
            return new UserPage();
        }

        // ---------- пользователи и пациенты ----------
        /// <summary>
        /// Создаёт учётную запись. Для роли User сразу создаётся карточка пациента
        /// (пол, полис, дата рождения, контакты); для Doctor задаётся карточка врача.
        /// </summary>
        public static user CreateUser(string fio, string login, string password, string role, string photo,
                                      int? doctorId = null, string gender = null, string policyNumber = null,
                                      DateTime? birthDate = null, string phone = null, string email = null,
                                      string guardianFio = null)
        {
            var u = new user
            {
                fio = fio,
                login = login,
                password = GetHash(password),
                role = role,
                photo = photo,
                email = email,
                doctor_id = role == "Doctor" ? doctorId : null,
                is_active = true,
                created_at = DateTime.Now
            };

            patient newPatient = null;
            if (role == "User")
            {
                newPatient = new patient
                {
                    fio = fio,
                    birth_date = birthDate ?? DateTime.Today,
                    gender = gender ?? "М",
                    policy_number = policyNumber ?? GeneratePolicyNumber(),
                    phone = phone,
                    email = email,
                    guardian_fio = guardianFio
                };
                u.patient = newPatient;
            }

            DB.user.Add(u);
            try
            {
                DB.SaveChanges();
            }
            catch
            {
                DB.Entry(u).State = System.Data.Entity.EntityState.Detached;
                if (newPatient != null) DB.Entry(newPatient).State = System.Data.Entity.EntityState.Detached;
                throw;
            }
            return u;
        }
        /// <summary>Создаёт карточку врача (специализация, кабинет, стаж).</summary>
        public static doctor CreateDoctorCard(string fio, int specializationId, string cabinet, int experience)
        {
            var d = new doctor
            {
                fio = fio,
                specialization_id = specializationId,
                experience = experience,
                cabinet = cabinet
            };
            DB.doctor.Add(d);
            DB.SaveChanges();
            return d;
        }
        /// <summary>Возвращает карточку пациента пользователя; если её нет - создаёт с временными данными.</summary>
        public static patient EnsurePatient(user u)
        {
            if (u.patient_id != null)
                return DB.patient.Find(u.patient_id);

            var p = new patient
            {
                fio = u.fio,
                birth_date = DateTime.Today,                 // пациент уточнит позже
                gender = "М",                                // обязательное поле (М/Ж)
                policy_number = GeneratePolicyNumber(),      // обязательное и уникальное
                email = u.email
            };
            u.patient = p;
            try
            {
                DB.SaveChanges();
            }
            catch
            {
                DB.Entry(p).State = System.Data.Entity.EntityState.Detached;
                u.patient = null;
                throw;
            }
            return p;
        }

        // ---------- приёмы ----------
        /// <summary>Карточка приёма (visit) + медицинская карта пациента; создаются при первой необходимости.</summary>
        public static visit GetOrCreateVisit(appointment a)
        {
            var v = DB.visit.FirstOrDefault(x => x.appointment_id == a.id);
            if (v != null) return v;

            var record = DB.medical_record.FirstOrDefault(r => r.patient_id == a.patient_id);
            if (record == null)
            {
                record = new medical_record { patient_id = a.patient_id, created_at = DateTime.Now };
                DB.medical_record.Add(record);
            }

            v = new visit { appointment_id = a.id, medical_record = record, visit_date = DateTime.Now };
            DB.visit.Add(v);
            DB.SaveChanges();
            return v;
        }

        /// <summary>Отмена записи: статус «Отменён», время снова свободно для записи.</summary>
        public static void CancelAppointment(appointment a)
        {
            a.status = "Отменён";
            var slot = DB.schedule.Find(a.schedule_id);
            if (slot != null) slot.status = "Свободно";
            DB.SaveChanges();
        }
        /// <summary>Сбрасывает несохранённые изменения после неудачного SaveChanges.</summary>
        public static void DiscardChanges()
        {
            foreach (var e in DB.ChangeTracker.Entries().ToList())
            {
                if (e.State == System.Data.Entity.EntityState.Added)
                    e.State = System.Data.Entity.EntityState.Detached;
                else if (e.State == System.Data.Entity.EntityState.Modified ||
                         e.State == System.Data.Entity.EntityState.Deleted)
                    e.Reload();
            }
        }
        public static string ErrorMessage(Exception ex)
        {
            while (ex.InnerException != null) ex = ex.InnerException;
            return ex.Message;
        }
    }
}
