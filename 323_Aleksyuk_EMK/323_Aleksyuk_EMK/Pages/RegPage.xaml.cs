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

            var ctx = Core.DB;

            // 6.2 логин должен быть уникальным
            string login = TbLogin.Text.Trim();
            if (Core.DB.user.AsNoTracking().Any(u => u.login == login))
            {
                ErrorText.Text = "Пользователь с таким логином уже существует!";
                return;
            }

			// 6.3 формат пароля: проверка длины
			if (PbFirst.Password.Length < 6)
			{
				ErrorText.Text = "Пароль должен быть не менее 6 символов!";
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
            var user = new user
            {
                fio = TbFio.Text.Trim(),
                login = login,
                password = Core.GetHash(PbFirst.Password),
                role = role
            };
            Core.DB.user.Add(user);
            Core.DB.SaveChanges();

			// для роли User сразу создаём карточку пациента, чтобы можно было записываться на приём
			if (role == "User")
			{
				var newPatient = new patient { fio = user.fio };
				newPatient.user = (ICollection<user>)user;
				Core.DB.patient.Add(newPatient);
				Core.DB.SaveChanges();
			}

			MessageBox.Show("Пользователь успешно зарегистрирован!", "Регистрация",
                MessageBoxButton.OK, MessageBoxImage.Information);
            NavigationService?.Navigate(new AuthPage());
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new AuthPage());
        }
    }
}