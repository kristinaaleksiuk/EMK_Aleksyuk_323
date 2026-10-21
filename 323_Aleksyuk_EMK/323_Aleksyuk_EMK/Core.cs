using System;
using System.Security.Cryptography;
using System.Text;

namespace _323_Aleksyuk_EMK
{
    public class Core
    {
        // один общий контекст базы данных для всего приложения
        public static EMKEntities DB { get; } = new EMKEntities();

        // кто сейчас вошёл в систему
        public static user CurrentUser { get; set; }

        // Превращает пароль в хеш SHA1 (40 символов), по хешу пароль не восстановить
        public static string GetHash(string input)
        {
            SHA1 sha = SHA1.Create();
            byte[] bytes = Encoding.UTF8.GetBytes(input);   // пароль -> байты
            byte[] hash = sha.ComputeHash(bytes);           // байты -> хеш

            string result = "";
            foreach (byte b in hash)
                result += b.ToString("X2");                 // каждый байт -> 2 символа
            return result;
        }

        // Проверка формата пароля. Возвращает текст ошибки или null, если всё хорошо
        public static string ValidatePassword(string password)
        {
            if (password.Length < 6)
                return "Пароль слишком короткий, минимум 6 символов!";

            bool hasDigit = false;
            foreach (char c in password)
            {
                if (char.IsDigit(c))
                    hasDigit = true;

                bool isLatin = (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z');
                if (char.IsLetter(c) && !isLatin)
                    return "Используйте только английскую раскладку!";
            }

            if (!hasDigit)
                return "Добавьте хотя бы одну цифру!";

            return null;
        }

        // Возвращает карточку пациента пользователя; если её нет - создаёт (при первой записи на приём)
        public static patient EnsurePatient(user u)
        {
            if (u.patient_id != null)
                return DB.patient.Find(u.patient_id);

            var p = new patient
            {
                fio = u.fio,
                birth_date = DateTime.Today,
                gender = "М",
                policy_number = "P" + Guid.NewGuid().ToString("N").Substring(0, 12)
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
    }
}