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
            CbSpec.ItemsSource = Core.DB.specialization.ToList().OrderBy(s => s.name).ToList();
            CbRole.SelectedIndex = 0;

            if (_current != null)
            {
                TbTitle.Text = "Изменение пользователя";
                LblPass.Text = "Новый пароль (оставьте пустым, чтобы не менять)";
                HintPass.Content = "Оставьте пустым, чтобы не менять";
                TbFio.Text = _current.fio;
                TbLogin.Text = _current.login;
                TbPhoto.Text = _current.photo;
                CbRole.SelectedIndex = _current.role == "Admin" ? 1 : _current.role == "Doctor" ? 2 : 0;

                // у врача подставляем данные его карточки
                if (_current.doctor_id != null)
                {
                    var card = Core.DB.doctor.Find(_current.doctor_id);
                    if (card != null)
                    {
                        CbSpec.SelectedItem = ((IEnumerable<specialization>)CbSpec.ItemsSource)
                            .FirstOrDefault(s => s.id == card.specialization_id);
                        TbCabinet.Text = card.cabinet;
                        TbExperience.Text = card.experience.ToString();
                    }
                }
            }
        }

        // поля врача показываем только для роли Doctor
        private void CbRole_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DoctorPanel == null) return;
            var role = (CbRole.SelectedItem as ComboBoxItem)?.Content.ToString();
            DoctorPanel.Visibility = role == "Doctor" ? Visibility.Visible : Visibility.Collapsed;
        }

        // в стаже можно вводить только цифры
        private void TbExperience_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !e.Text.All(char.IsDigit);
        }
       
        // Плейсхолдеры: подсказка видна, пока поле пустое 
        private void TbFio_TextChanged(object sender, TextChangedEventArgs e)
        {
            HintFio.Visibility = TbFio.Text.Length > 0 ? Visibility.Hidden : Visibility.Visible;
        }

        private void TbLogin_TextChanged(object sender, TextChangedEventArgs e)
        {
            HintLogin.Visibility = TbLogin.Text.Length > 0 ? Visibility.Hidden : Visibility.Visible;
        }

        private void PbPass_PasswordChanged(object sender, RoutedEventArgs e)
        {
            HintPass.Visibility = PbPass.Password.Length > 0 ? Visibility.Hidden : Visibility.Visible;
        }

        private void TbPhoto_TextChanged(object sender, TextChangedEventArgs e)
        {
            HintPhoto.Visibility = TbPhoto.Text.Length > 0 ? Visibility.Hidden : Visibility.Visible;
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
                string passError = Core.ValidatePassword(PbPass.Password);
                if (passError != null) { ErrorText.Text = passError; return; }
            }

            string role = ((ComboBoxItem)CbRole.SelectedItem).Content.ToString();
            string photo = string.IsNullOrWhiteSpace(TbPhoto.Text) ? null : TbPhoto.Text.Trim();

            // данные врача (для карточки врача)
            int specId = 0, experience = 0;
            string cabinet = null;
            if (role == "Doctor")
            {
                var spec = CbSpec.SelectedItem as specialization;
                if (spec == null) { ErrorText.Text = "Выберите специализацию!"; return; }
                specId = spec.id;

                cabinet = TbCabinet.Text.Trim();
                if (cabinet.Length == 0) { ErrorText.Text = "Укажите кабинет!"; return; }

                if (!int.TryParse(TbExperience.Text.Trim(), out experience) || experience < 0 || experience > 60)
                { ErrorText.Text = "Стаж должен быть числом от 0 до 60!"; return; }
            }

            doctor newCard = null;
            try
            {
                if (_current == null)
                {
                    if (role == "Doctor") newCard = Core.CreateDoctorCard(fio, specId, cabinet, experience);
                    Core.CreateUser(fio, login, PbPass.Password, role, photo, doctorId: newCard?.id);
                }
                else
                {
                    _current.fio = fio;
                    _current.login = login;
                    _current.role = role;
                    _current.photo = photo;

                    // doctor_id и patient_id одновременно задавать нельзя (CK_user_one_link)
                    if (role == "Doctor")
                    {
                        if (_current.doctor_id == null)
                        {
                            newCard = Core.CreateDoctorCard(fio, specId, cabinet, experience);
                            _current.doctor_id = newCard.id;
                        }
                        else
                        {
                            var card = Core.DB.doctor.Find(_current.doctor_id);
                            if (card != null)
                            {
                                card.fio = fio;
                                card.specialization_id = specId;
                                card.cabinet = cabinet;
                                card.experience = experience;
                            }
                        }
                        _current.patient_id = null;
                    }
                    else
                    {
                        _current.doctor_id = null;
                    }

                    if (needPassword) _current.password = Core.GetHash(PbPass.Password);
                    Core.DB.SaveChanges();

                    if (role == "User" && _current.patient_id == null)
                        Core.EnsurePatient(_current);
                }

                MessageBox.Show("Данные успешно сохранены!");
                NavigationService?.GoBack();
            }
            catch (Exception ex)
            {
                Core.DiscardChanges();
                // если карточка врача создалась, а пользователя сохранить не удалось, убираем её
                if (newCard != null)
                {
                    try { Core.DB.doctor.Remove(newCard); Core.DB.SaveChanges(); }
                    catch { Core.DiscardChanges(); }
                }
                MessageBox.Show(Core.ErrorMessage(ex));
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.GoBack();
        }
    }
}