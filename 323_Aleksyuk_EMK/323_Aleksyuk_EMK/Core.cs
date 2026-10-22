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

        /// <summary>Создаёт учётную запись пароль хешируется. Карточка пациента здесь не создаётся.</summary>
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

            DB.user.Add(u);
            try
            {
                DB.SaveChanges();
            }
            catch
            {
                DB.Entry(u).State = System.Data.Entity.EntityState.Detached;
                throw;
            }
            return u;
        }

        /// <summary>Возвращает карточку пациента пользователя; если её нет - создаёт (при первой записи на приём).</summary>
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