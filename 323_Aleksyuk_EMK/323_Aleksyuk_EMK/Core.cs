using DurableTask.Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _323_Aleksyuk_EMK
{
    public class Core
    {
        // Создаем один общий контекст под именами DB и EMKEntities для совместимости
        public static EMKEntities DB { get; } = new EMKEntities();
        public static EMKEntities EMKEntities => DB;

        // Добавляем статическое свойство для хранения текущего пользователя
        // Замените 'user' на точное имя вашего класса пользователя, если оно отличается (например, User)
        public static user CurrentUser { get; set; }
    }
}
