using _323_Aleksyuk_EMK;
using DurableTask.Core.Common;
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
using System.Windows.Threading;
using static System.Collections.Specialized.BitVector32;
using System.Web.Providers.Entities;
namespace _323_Aleksyuk_EMK.Pages
{
    /// <summary>Шаги 4-5: авторизация + капча.
    /// По макету: капча появляется после первой неудачной попытки, после трёх ошибок вход блокируется.</summary>
    public partial class AuthPage : Page
    {
        private const int MaxAttempts = 3;     // допустимое число неверных попыток

        private static readonly Random Rnd = new Random();
        private int failedAttempts = 0;
        private string captchaValue = "";
        private DispatcherTimer lockTimer;

        public AuthPage()
        {
            InitializeComponent();
            Core.CurrentUser = null;          // открытие страницы авторизации = выход из системы
        }
        
        private void ButtonEnter_Click(object sender, RoutedEventArgs e)
        {
            
            // Если капча показана - сначала проверяем её
            if (CaptchaPanel.Visibility == Visibility.Visible && TbCaptcha.Text.Trim() != captchaValue)
            {
                ErrorText.Text = "Неверно введена капча";
                NewCaptcha();
                return;
            }

            string hashedPassword = Core.GetHash(PbPassword.Password);   // Шаг 7
            var user = Core.DB.user.AsNoTracking()
                .FirstOrDefault(u => u.login == TbLogin.Text && u.password == hashedPassword);

            if (user == null)
            {
                failedAttempts++;
                PbPassword.Clear();
                ErrorText.Text = "Неверный логин или пароль";

                if (failedAttempts >= MaxAttempts)      // капча после 3 неверных попыток (Шаг 5)
                    ShowCaptcha();
                return;
            }

            // Успешный вход: берём отслеживаемую запись пользователя
            Core.CurrentUser = Core.DB.user.First(u => u.id == user.id);
            failedAttempts = 0;

            switch (user.role)
            {
                case "Admin":
                    NavigationService?.Navigate(new AdminPage());
                    break;
                case "User":
                    NavigationService?.Navigate(new UserPage());
                    break;
                case "Doctor":
                    NavigationService?.Navigate(new DoctorPage());
                    break;
                default:
                    ErrorText.Text = "Для роли «" + user.role + "» страница не предусмотрена";
                    break;
            }
        }

        private void ButtonReg_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new RegPage());
        }

        // ----- Капча -----
        private void ShowCaptcha()
        {
            if (CaptchaPanel.Visibility != Visibility.Visible)
                CaptchaPanel.Visibility = Visibility.Visible;
            NewCaptcha();
        }

        /// <summary>Случайные 5 символов (без похожих 0/O, 1/I)</summary>
        private void NewCaptcha()
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
            var sb = new StringBuilder();
            for (int i = 0; i < 5; i++)
                sb.Append(chars[Rnd.Next(chars.Length)]);
            captchaValue = sb.ToString();
            CaptchaText.Text = string.Join(" ", captchaValue.ToCharArray());
            TbCaptcha.Clear();
        }

        //вставка
        private void TbCaptcha_PreviewExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Command == ApplicationCommands.Copy ||
                e.Command == ApplicationCommands.Cut ||
                e.Command == ApplicationCommands.Paste)
            {
                e.Handled = true;
            }
        }
    }
}


