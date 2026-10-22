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
using static System.Collections.Specialized.BitVector32;

namespace _323_Aleksyuk_EMK.Pages
{
    /// <summary>Шаг 11 смена пароля текущего пользователя</summary>
    public partial class ChangePassPage : Page
    {
        public ChangePassPage()
        {
            InitializeComponent();
        }

        private void BtnChange_Click(object sender, RoutedEventArgs e)
        {
            var user = Core.CurrentUser;
            if (user == null) { NavigationService?.Navigate(new AuthPage()); return; }

            if (string.IsNullOrEmpty(PbCurrent.Password) || string.IsNullOrEmpty(PbNew.Password) ||
                string.IsNullOrEmpty(PbConfirm.Password))
            {
                ErrorText.Text = "Все поля обязательны к заполнению!";
                return;
            }

            if (Core.GetHash(PbCurrent.Password) != user.password)
            {
                ErrorText.Text = "Текущий пароль неверный!";
                return;
            }
            if (PbNew.Password == PbCurrent.Password)
            {
                ErrorText.Text = "Новый пароль не должен совпадать с текущим";
                return;
            }

           
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

            user.password = Core.GetHash(PbNew.Password);
            Core.DB.SaveChanges();
            MessageBox.Show("Пароль успешно изменен!");
            NavigationService?.GoBack();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.GoBack();
        }
    }
}
