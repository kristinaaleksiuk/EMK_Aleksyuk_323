using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace _323_Aleksyuk_EMK.Pages
{
    public partial class AuthPage : Page
    {
        private int failedAttempts = 0;        
        private string captchaValue = "";      
        private Random rnd = new Random();

        public AuthPage()
        {
            InitializeComponent();
            Core.CurrentUser = null;           
        }

        private void ButtonEnter_Click(object sender, RoutedEventArgs e)
        {
            ErrorText.Text = "";

           
            if (TbLogin.Text == "" || PbPassword.Password == "")
            {
                ErrorText.Text = "Введите логин и пароль";
                return;
            }

           
            if (CaptchaPanel.Visibility == Visibility.Visible && TbCaptcha.Text.Trim() != captchaValue)
            {
                ErrorText.Text = "Неверно введена капча";
                NewCaptcha();
                return;
            }

            
            string hash = Core.GetHash(PbPassword.Password);
            user found = Core.DB.user.FirstOrDefault(u => u.login == TbLogin.Text && u.password == hash);


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

            // 5. Нашли - запоминаем пользователя
            Core.CurrentUser = found;
            failedAttempts = 0;
            ErrorText.Text = "";

            string roleName = found.role == "Admin" ? "Администратор"
                            : found.role == "Doctor" ? "Врач"
                                                     : "Пользователь";

            MessageBox.Show("Добро пожаловать, " + found.fio + "!\nРоль: " + roleName,
                "Авторизация успешна",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            if (found.role == "Admin")
                NavigationService?.Navigate(new AdminPage());
            else if (found.role == "Doctor")
                NavigationService?.Navigate(new DoctorPage());
            else
                NavigationService?.Navigate(new UserPage());
        }

        private void ButtonReg_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new RegPage());
        }

        
        private void NewCaptcha()
        {
            string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
            captchaValue = "";
            for (int i = 0; i < 5; i++)
                captchaValue += chars[rnd.Next(chars.Length)];

            CaptchaText.Text = string.Join(" ", captchaValue.ToCharArray());   
            TbCaptcha.Clear();
        }

        
        private void TbCaptcha_PreviewExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Command == ApplicationCommands.Copy ||
                e.Command == ApplicationCommands.Cut ||
                e.Command == ApplicationCommands.Paste)
                e.Handled = true;
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