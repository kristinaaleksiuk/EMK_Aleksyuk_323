using DurableTask.Core.Common;
using System;
using System.Collections.Generic;
using System.Data;
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
                CbRole.SelectedIndex = _current.role == "Admin" ? 1 : 0;
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
            var list = Core.DB.user.ToList().AsEnumerable();
            string fio = TbFio.Text.Trim();
            string login = TbLogin.Text.Trim();

            if (fio.Length == 0) { ErrorText.Text = "Укажите ФИО!"; return; }
            if (login.Length == 0) { ErrorText.Text = "Укажите логин!"; return; }
            if (CbRole.SelectedItem == null) { ErrorText.Text = "Выберите роль!"; return; }

            int selfId = _current != null ? _current.id : 0;
            if (Core.DB.user.Any(u => u.login == login && u.id != selfId))
            {
                ErrorText.Text = "Пользователь с таким логином уже существует!";
                return;
            }

            bool needPassword = _current == null || PbPass.Password.Length > 0;
            if (needPassword)
            {
                if (string.IsNullOrWhiteSpace(PbPass.Password))
                {
                    ErrorText.Text = "Пароль не может быть пустым!";
                    return;
                }
            }

            string Role = ((ComboBoxItem)CbRole.SelectedItem).Content.ToString();
            string photo = string.IsNullOrWhiteSpace(TbPhoto.Text) ? null : TbPhoto.Text.Trim();

            try
            {
                // 1. Считываем данные роли напрямую из интерфейса, чтобы избежать ошибок с переменными
                string selectedRole = CbRole.Text;

                if (_current == null)
                {
                    var user = new user
                    {
                        fio = fio,
                        login = login,
                        role = selectedRole, // Передаем правильную переменную
                        photo = photo,
                        password = Core.GetHash(PbPass.Password)
                    };
                    Core.DB.user.Add(user);
                    Core.DB.SaveChanges();

                    if (selectedRole == "User")
                    {
                        // Создаем пациента и привязываем к нему созданного пользователя по правилам EF5
                        var newPatient = new patient { fio = fio };
                        newPatient.user = (ICollection<user>)user;

                        Core.DB.patient.Add(newPatient);
                        Core.DB.SaveChanges();
                    }
                }
                else
                {
                    _current.fio = fio;
                    _current.login = login;
                    _current.role = selectedRole; // Передаем правильную переменную
                    _current.photo = photo;

                    if (needPassword)
                    {
                        _current.password = Core.GetHash(PbPass.Password);
                    }
                    Core.DB.SaveChanges();
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

