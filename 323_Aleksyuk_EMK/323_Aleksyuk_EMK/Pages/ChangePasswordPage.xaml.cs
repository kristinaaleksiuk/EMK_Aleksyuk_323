using DurableTask.Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Providers.Entities;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace _323_Aleksyuk_EMK.Pages
{
    /// <summary>Шаг 11: смена пароля текущего пользователя (доступна обеим ролям)</summary>
    public partial class ChangePasswordPage : Page
    {
        public ChangePasswordPage()
        {
            InitializeComponent();
        }

        private void BtnChange_Click(object sender, RoutedEventArgs e)
        {
            var user = Core.CurrentUser;
            if (user == null) { NavigationService?.Navigate(new AuthPage()); return; }

            // 3. заполнены все поля
            if (string.IsNullOrEmpty(PbCurrent.Password) || string.IsNullOrEmpty(PbNew.Password) ||
                string.IsNullOrEmpty(PbConfirm.Password))
            {
                ErrorText.Text = "Все поля обязательны к заполнению!";
                return;
            }

            // 4. текущий пароль верный (сравниваем хеши)
            if (Core.GetHash(PbCurrent.Password) != user.Password)
            {
                ErrorText.Text = "Текущий пароль неверный!";
                return;
            }

            // новый пароль не должен совпадать с текущим
            if (PbNew.Password == PbCurrent.Password)
            {
                ErrorText.Text = "Новый пароль не должен совпадать с текущим";
                return;
            }

            // 5. корректность нового пароля (как на странице регистрации)
            string passError = Core.ValidatePassword(PbNew.Password);
            if (passError != null)
            {
                ErrorText.Text = passError;
                return;
            }

            if (PbNew.Password != PbConfirm.Password)
            {
                ErrorText.Text = "Пароли не совпадают!";
                return;
            }

            // 6. сохраняем хеш нового пароля
            user.Password = Core.GetHash(PbNew.Password);
            Entities.GetContext().SaveChanges();
            MessageBox.Show("Пароль успешно изменен!");
            NavigationService?.GoBack();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.GoBack();
        }
    }
}