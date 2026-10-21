using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace _323_Aleksyuk_EMK.Pages
{
    public partial class AuthPage : Page
    {
        private int failedAttempts = 0;        // сколько раз подряд ввели неверно
        private string captchaValue = "";      // правильный текст капчи
        private Random rnd = new Random();

        public AuthPage()
        {
            InitializeComponent();
            Core.CurrentUser = null;           // открыли страницу входа = вышли из системы
        }

        private void ButtonEnter_Click(object sender, RoutedEventArgs e)
        {
            ErrorText.Text = "";

            // 1. Поля заполнены?
            if (TbLogin.Text == "" || PbPassword.Password == "")
            {
                ErrorText.Text = "Введите логин и пароль";
                return;
            }

            // 2. Если на экране капча - сначала проверяем её
            if (CaptchaPanel.Visibility == Visibility.Visible && TbCaptcha.Text.Trim() != captchaValue)
            {
                ErrorText.Text = "Неверно введена капча";
                NewCaptcha();
                return;
            }

            // 3. Ищем пользователя: логин такой же и хеш пароля такой же
            string hash = Core.GetHash(PbPassword.Password);
            user found = Core.DB.user.FirstOrDefault(u => u.login == TbLogin.Text && u.password == hash);

            // 4. Не нашли - считаем ошибку; после 3-й показываем капчу
            if (found == null)
            {
                failedAttempts++;
                PbPassword.Clear();
                ErrorText.Text = "Неверный логин или пароль";

                if (failedAttempts >= 3)
                {
                    CaptchaPanel.Visibility = Visibility.Visible;
                    NewCaptcha();
                }
                return;
            }

            // 5. Нашли - запоминаем пользователя и открываем страницу по роли
            Core.CurrentUser = found;

            if (found.role == "Admin")
                NavigationService?.Navigate(new AdminPage());
            else
                NavigationService?.Navigate(new UserPage());
        }

        private void ButtonReg_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new RegPage());
        }

        // Новая капча: 5 случайных символов
        private void NewCaptcha()
        {
            string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
            captchaValue = "";
            for (int i = 0; i < 5; i++)
                captchaValue += chars[rnd.Next(chars.Length)];

            CaptchaText.Text = string.Join(" ", captchaValue.ToCharArray());   // с пробелами для читаемости
            TbCaptcha.Clear();
        }

        // В поле капчи нельзя копировать и вставлять
        private void TbCaptcha_PreviewExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Command == ApplicationCommands.Copy ||
                e.Command == ApplicationCommands.Cut ||
                e.Command == ApplicationCommands.Paste)
                e.Handled = true;
        }

        // ----- Плейсхолдеры -----
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