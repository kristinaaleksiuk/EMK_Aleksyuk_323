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

namespace _323_Aleksyuk_EMK.Pages
{
    /// <summary>Шаг 10: форма добавления / редактирования пользователя</summary>
    public partial class AddUserPage : Page
    {
        private readonly user _current;       // null - добавление новой записи

        public AddUserPage(user selectedUser)
        {
            InitializeComponent();
            _current = selectedUser;
            CbRole.SelectedIndex = 0;

            if (_current != null)
            {
                TbTitle.Text = "Изменение пользователя";
                LblPass.Text = "Новый пароль (оставьте пустым, чтобы не менять)";
                TbFio.Text = _current.fio;
                TbLogin.Text = _current.login;
                TbPhoto.Text = _current.photo;
                CbRole.SelectedIndex = _current.Role == "Admin" ? 1 : 0;
            }
        }

        private void BtnBrowse_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Изображения (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg"
            };
            if (dlg.ShowDialog() == true)
                TbPhoto.Text = dlg.FileName;
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            var ctx = Entities.GetContext();
            string fio = TbFio.Text.Trim();
            string login = TbLogin.Text.Trim();

            if (fio.Length == 0) { ErrorText.Text = "Укажите ФИО!"; return; }
            if (login.Length == 0) { ErrorText.Text = "Укажите логин!"; return; }
            if (CbRole.SelectedItem == null) { ErrorText.Text = "Выберите роль!"; return; }

            int selfId = _current != null ? _current.id : 0;
            if (ctx.user.Any(u => u.login == login && u.id != selfId))
            {
                ErrorText.Text = "Пользователь с таким логином уже существует!";
                return;
            }

            bool needPassword = _current == null || PbPass.Password.Length > 0;
            if (needPassword)
            {
                string passError = Core.ValidatePassword(PbPass.Password);
                if (passError != null) { ErrorText.Text = passError; return; }
            }

            string Role = ((ComboBoxItem)CbRole.SelectedItem).Content.ToString();
            string photo = string.IsNullOrWhiteSpace(TbPhoto.Text) ? null : TbPhoto.Text.Trim();

            try
            {
                if (_current == null)
                {
                    var user = new user
                    {
                        fio = fio,
                        login = login,
                        role = role,
                        photo = photo,
                        password = Core.GetHash(PbPass.Password)
                    };
                    ctx.user.Add(user);
                    ctx.SaveChanges();

                    if (Role == "User")
                    {
                        ctx.patient.Add(new patient { fio = fio, user = user });
                        ctx.SaveChanges();
                    }
                }
                else
                {
                    _current.fio = fio;
                    _current.login = login;
                    _current.role = role;
                    _current.photo = photo;
                    if (needPassword)
                        _current.password = Core.GetHash(PbPass.Password);
                    ctx.SaveChanges();
                }

                MessageBox.Show("Данные успешно сохранены!");
                NavigationService?.GoBack();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.GoBack();
        }
    }
}

