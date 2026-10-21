using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace _323_Aleksyuk_EMK.Pages
{
    /// <summary>Авторизация + капча. Капча появляется после 3 неудачных попыток входа.</summary>
    public partial class AuthPage : Page
    {
        private const int MaxAttempts = 3;     // после стольких ошибок появляется капча

        private static readonly Random Rnd = new Random();
        private int failedAttempts = 0;
        private string captchaValue = "";

        public AuthPage()
        {
            InitializeComponent();
            Core.CurrentUser = null;          // открытие страницы авторизации = выход из системы
        }

        private void ButtonEnter_Click(object sender, RoutedEventArgs e)
        {
            ErrorText.Text = "";

            if (string.IsNullOrEmpty(TbLogin.Text) || string.IsNullOrEmpty(PbPassword.Password))
            {
                ErrorText.Text = "Введите логин и пароль";
                return;
            }

            // Если капча уже показана - сначала проверяем её
            if (CaptchaPanel.Visibility == Visibility.Visible && TbCaptcha.Text.Trim() != captchaValue)
            {
                ErrorText.Text = "Неверно введена капча";
                NewCaptcha();
                return;
            }

            string hashedPassword = Core.GetHash(PbPassword.Password);
            var user = Core.DB.user.AsNoTracking()
                .FirstOrDefault(u => u.login == TbLogin.Text && u.password == hashedPassword);

            if (user == null)
            {
                failedAttempts++;
                PbPassword.Clear();

                if (failedAttempts >= MaxAttempts)
                {
                    ShowCaptcha();                        // капча после 3 неудачных попыток
                    ErrorText.Text = "Неверный логин или пароль. Введите капчу";
                }
                else
                {
                    ErrorText.Text = "Неверный логин или пароль. Осталось попыток до капчи: "
                                     + (MaxAttempts - failedAttempts);
                }
                return;
            }

            // Успешный вход
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

        // Копирование и вставка в поле капчи запрещены
        private void TbCaptcha_PreviewExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Command == ApplicationCommands.Copy ||
                e.Command == ApplicationCommands.Cut ||
                e.Command == ApplicationCommands.Paste)
            {
                e.Handled = true;
            }
        }
    
        private void TbLogin_TextChanged(object sender, TextChangedEventArgs e)
        {
            lblLoginHint.Visibility = TbLogin.Text.Length > 0 ? Visibility.Hidden : Visibility.Visible;
        }
        private void lblLoginHint_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) { TbLogin.Focus(); }

        private void PbPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            lblPassHint.Visibility = PbPassword.Password.Length > 0 ? Visibility.Hidden : Visibility.Visible;
        }
        private void lblPassHint_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) { PbPassword.Focus(); }
    }
}