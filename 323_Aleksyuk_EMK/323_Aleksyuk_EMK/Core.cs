using DurableTask.Core.Common;
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
        // Создаем один общий контекст под именами DB и EMKEntities для совместимости

        public static EMKEntities Context = new EMKEntities();
        public static EMKEntities DB { get; } = new EMKEntities();
        public static EMKEntities EMKEntities => DB;

        // Добавляем статическое свойство для хранения текущего пользователя
        // Замените 'user' на точное имя вашего класса пользователя, если оно отличается (например, User)
        public static user CurrentUser { get; set; }
        public static string GetHash(string input)
        {
            using (var sha = SHA1.Create())
            {
                byte[] hash = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
                return string.Concat(hash.Select(b => b.ToString("X2")));
            }
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
        /// <summary>Создаёт учётную запись (пароль хешируется). Для роли User сразу создаётся карточка пациента,
        /// чтобы пользователь мог записываться на приём; недостающие данные пациента заполняет администратор.</summary>
        public static user CreateUser(string fio, string login, string password, string role, string photo)
        {
            var u = new user
            {
                fio = fio,
                login = login,
                password = GetHash(password),
                role = role,
                photo = photo,
                is_active = true,
                created_at = DateTime.Now
            };

            if (role == "User")
            {
                u.patient = new patient
                {
                    fio = fio,
                    birth_date = DateTime.Today,                                   // уточняется в таблице «Пациенты»
                    gender = "Н",                                                  // не указан
                    policy_number = "P" + Guid.NewGuid().ToString("N").Substring(0, 12)
                };
            }

            DB.user.Add(u);
            try
            {
                DB.SaveChanges();
            }
            catch
            {
                // неудавшуюся запись убираем из контекста, чтобы она не мешала следующим сохранениям
                if (u.patient != null) DB.Entry(u.patient).State = System.Data.Entity.EntityState.Detached;
                DB.Entry(u).State = System.Data.Entity.EntityState.Detached;
                throw;
            }
            return u;
        }

    }

}
