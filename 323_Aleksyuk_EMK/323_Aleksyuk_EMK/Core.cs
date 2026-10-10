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
			if (string.IsNullOrWhiteSpace(password)) return "Пароль не может быть пустым";
			if (password.Length < 6) return "Пароль должен быть не менее 6 символов";
			if (!password.Any(char.IsDigit)) return "Пароль должен содержать хотя бы одну цифру";
			if (password.Any(c => (c >= 'а' && c <= 'я') || (c >= 'А' && c <= 'Я') || c == 'ё' || c == 'Ё'))
				return "Пароль должен быть на английской раскладке";
			return null;
		}
	}

}
