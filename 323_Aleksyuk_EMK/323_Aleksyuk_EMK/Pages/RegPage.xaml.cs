using _323_Aleksyuk_EMK.Pages;
using System;
using System.Data.Entity;
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
            DpBirth.DisplayDateEnd = DateTime.Today;      // роль по умолчанию - User
        }
              // законный представитель нужен для пациентов младше 18 лет
        private void DpBirth_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
           if (GuardianPanel == null) return;
           var d = DpBirth.SelectedDate;
            GuardianPanel.Visibility = (d.HasValue && Core.Age(d.Value) < 18) ? Visibility.Visible : Visibility.Collapsed;
       }

        private void TbPolicy_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !e.Text.All(char.IsDigit);
        }

        private void TbPhone_PreviewTextInput(object sender, TextCompositionEventArgs e) => e.Handled = !e.Text.All(ch => char.IsDigit(ch) || "+()- ".IndexOf(ch) >= 0);
        private void BtnRegister_Click(object sender, RoutedEventArgs e)
        {
           
            if (string.IsNullOrWhiteSpace(TbLogin.Text) || string.IsNullOrWhiteSpace(TbFio.Text) ||
                string.IsNullOrEmpty(PbFirst.Password) || string.IsNullOrEmpty(PbSecond.Password))
            {
                ErrorText.Text = "Заполните все поля!";
                return;
            }

            string login = TbLogin.Text.Trim();
            if (Core.DB.user.AsNoTracking().Any(u => u.login == login))
            {
                ErrorText.Text = "Пользователь с таким логином уже существует!";
                return;
            }

            string passError = Core.ValidatePassword(PbFirst.Password);
            if (passError != null)
            {
                ErrorText.Text = passError;
                return;
            }
            if (PbFirst.Password != PbSecond.Password)
            {
                ErrorText.Text = "Пароли не совпадают!";
                return;
            }


            // ----- данные пациента -----
            string gender = (CbGender.SelectedItem as ComboBoxItem)?.Content.ToString();
            if (gender == null) { ErrorText.Text = "Выберите пол!"; return; }

            if (!DpBirth.SelectedDate.HasValue) { ErrorText.Text = "Укажите дату рождения!"; return; }
            DateTime birth = DpBirth.SelectedDate.Value.Date;
            if (birth > DateTime.Today || birth < new DateTime(1900, 1, 1))
            {
                ErrorText.Text = "Укажите корректную дату рождения!";
                return;
            }

            string policy = TbPolicy.Text.Trim();
            if (policy.Length != 16 || !policy.All(char.IsDigit))
            {
                ErrorText.Text = "Номер полиса должен состоять из 16 цифр!"; return;
            }
            if (Core.DB.patient.AsNoTracking().Any(p => p.policy_number == policy))
            {
                ErrorText.Text = "Пациент с таким номером полиса уже зарегистрирован!";
                return;
            }

            string phone = TbPhone.Text.Trim();
            if (phone.Count(char.IsDigit) < 10)
            {
                ErrorText.Text = "Введите телефон полностью (не менее 10 цифр)!";
                return;
            }

            string email = TbEmail.Text.Trim();
            if (!System.Text.RegularExpressions.Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                ErrorText.Text = "Введите корректный адрес электронной почты!";
                return;
            }

            string guardian = null;
            if (Core.Age(birth) < 18)
            {
                guardian = TbGuardian.Text.Trim();
                if (guardian.Length < 5)
                {
                    ErrorText.Text = "Для пациента младше 18 лет укажите ФИО законного представителя!";
                    return;
                }
            }

            if (ChkConsent.IsChecked != true)
            {
                ErrorText.Text = "Необходимо согласие на обработку персональных данных!";
                return;
            }

            try
            {

               Core.CreateUser(TbFio.Text.Trim(), login, PbFirst.Password, "User", null,
               gender: gender, policyNumber: policy, birthDate: birth,
               phone: phone, email: email, guardianFio: guardian);

            }
            catch (Exception ex)
            {
                ErrorText.Text = "Не удалось зарегистрировать: " + (ex.InnerException?.InnerException?.Message ?? ex.Message);
                return;
            }

            MessageBox.Show("Вы успешно зарегистрированы! Теперь можно войти в систему.", "Регистрация",
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