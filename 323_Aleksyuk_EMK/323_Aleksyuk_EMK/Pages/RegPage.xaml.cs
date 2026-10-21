using _323_Aleksyuk_EMK.Pages;
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

namespace _323_Aleksyuk_EMK
{
    /// <summary>Шаги 6-7: регистрация нового пользователя (пароль хешируется SHA1)</summary>
    public partial class RegPage : Page
    {
        public RegPage()
        {
            InitializeComponent();
            CbRole.SelectedIndex = 0;       // роль по умолчанию - User
        }

        private void BtnRegister_Click(object sender, RoutedEventArgs e)
        {
            // 6.1 проверка заполнения полей
            if (string.IsNullOrWhiteSpace(TbLogin.Text) || string.IsNullOrWhiteSpace(TbFio.Text) ||
                string.IsNullOrEmpty(PbFirst.Password) || string.IsNullOrEmpty(PbSecond.Password))
            {
                ErrorText.Text = "Заполните все поля!";
                return;
            }

           

            // 6.2 логин должен быть уникальным
            string login = TbLogin.Text.Trim();
            if (Core.DB.user.AsNoTracking().Any(u => u.login == login))
            {
                ErrorText.Text = "Пользователь с таким логином уже существует!";
                return;
            }

            // 6.3 формат пароля: длина, английская раскладка, цифра
            string passError = Core.ValidatePassword(PbFirst.Password);
            if (passError != null)
            {
                ErrorText.Text = passError;
                return;
            }

            // 6.4 пароли совпадают
            if (PbFirst.Password != PbSecond.Password)
            {
                ErrorText.Text = "Пароли не совпадают!";
                return;
            }

            // 6.5 запись в БД; пароль хранится только в виде хеша (Шаг 7)
            string role = ((ComboBoxItem)CbRole.SelectedItem).Content.ToString();
            try
            {
                // CreateUser хеширует пароль, добавляет запись в контекст и вызывает SaveChanges
                Core.CreateUser(TbFio.Text.Trim(), login, PbFirst.Password, role, null);
            }
            catch (Exception ex)
            {
                ErrorText.Text = "Не удалось зарегистрировать: " + (ex.InnerException?.InnerException?.Message ?? ex.Message);
                return;
            }

            MessageBox.Show("Пользователь успешно зарегистрирован!", "Регистрация",
                MessageBoxButton.OK, MessageBoxImage.Information);
            NavigationService?.Navigate(new AuthPage());
        }
        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new AuthPage());
        }
        private void TbFio_TextChanged(object sender, TextChangedEventArgs e)
        { lblFioHint.Visibility = TbFio.Text.Length > 0 ? Visibility.Hidden : Visibility.Visible; }
        private void lblFioHint_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) { TbFio.Focus(); }

        private void TbLogin_TextChanged(object sender, TextChangedEventArgs e)
        { lblLoginHint.Visibility = TbLogin.Text.Length > 0 ? Visibility.Hidden : Visibility.Visible; }
        private void lblLoginHint_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) { TbLogin.Focus(); }

        private void PbFirst_PasswordChanged(object sender, RoutedEventArgs e)
        { lblPassHint.Visibility = PbFirst.Password.Length > 0 ? Visibility.Hidden : Visibility.Visible; }
        private void lblPassHint_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) { PbFirst.Focus(); }

        private void PbSecond_PasswordChanged(object sender, RoutedEventArgs e)
        { lblPassSecHint.Visibility = PbSecond.Password.Length > 0 ? Visibility.Hidden : Visibility.Visible; }
        private void lblPassSecHint_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) { PbSecond.Focus(); }
    }
}