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

    /// <summary>Форма добавления / редактирования пациента</summary>
    public partial class AddPatientPage : Page
    {
        private readonly patient _current;

        public AddPatientPage(patient selected)
        {
            InitializeComponent();
            _current = selected;
            DpBirth.DisplayDateEnd = DateTime.Today;

            if (_current != null)
            {
                TbTitle.Text = "Изменение пациента";
                TbFio.Text = _current.fio;
                DpBirth.SelectedDate = _current.birth_date;
                foreach (ComboBoxItem item in CbGender.Items)
                    if (item.Content.ToString() == (_current.gender ?? "").Trim()) CbGender.SelectedItem = item;
                TbPolicy.Text = _current.policy_number;
                TbPhone.Text = _current.phone;
                TbEmail.Text = _current.email;
                TbGuardian.Text = _current.guardian_fio;
            }
                    }

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
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            string fio = TbFio.Text.Trim();
            if (fio.Length == 0) { ErrorText.Text = "Укажите ФИО пациента!"; return; }
            if (!DpBirth.SelectedDate.HasValue) { ErrorText.Text = "Укажите дату рождения!"; return; }
            DateTime birth = DpBirth.SelectedDate.Value.Date;
            if (birth > DateTime.Today) { ErrorText.Text = "Дата рождения не может быть в будущем!"; return; }
            if (birth < new DateTime(1900, 1, 1)) { ErrorText.Text = "Укажите корректную дату рождения!"; return; }

            string gender = (CbGender.SelectedItem as ComboBoxItem)?.Content.ToString();
            if (gender == null) { ErrorText.Text = "Выберите пол!"; return; }

            string policy = TbPolicy.Text.Trim();
            if (policy.Length != 16 || !policy.All(char.IsDigit))
            { ErrorText.Text = "Номер полиса должен состоять из 16 цифр!"; return; }

            int selfId = _current != null ? _current.id : 0;
            if (Core.DB.patient.Any(p => p.policy_number == policy && p.id != selfId))
            { ErrorText.Text = "Пациент с таким номером полиса уже есть!"; return; }

            string email = string.IsNullOrWhiteSpace(TbEmail.Text) ? null : TbEmail.Text.Trim();
            if (email != null && !System.Text.RegularExpressions.Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            { ErrorText.Text = "Введите корректный адрес электронной почты!"; return; }

            string guardian = null;
            if (Core.Age(birth) < 18)
            {
                guardian = TbGuardian.Text.Trim();
                if (guardian.Length < 5)
                { ErrorText.Text = "Для пациента младше 18 лет укажите ФИО законного представителя!"; return; }
            }

            string phone = string.IsNullOrWhiteSpace(TbPhone.Text) ? null : TbPhone.Text.Trim();

            try

            {
                if (_current == null)
                {
                    Core.DB.patient.Add(new patient
                    {
                        fio = fio,
                        birth_date = birth,
                        gender = gender,
                        policy_number = policy,
                        phone = phone,
                        email = email,
                        guardian_fio = guardian

                    });
                }
                else
                {
                    _current.fio = fio;
                    _current.birth_date = birth;
                    _current.gender = gender;
                    _current.policy_number = policy;
                    _current.phone = phone;
                    _current.email = email;
                    _current.guardian_fio = guardian;

                }
                Core.DB.SaveChanges();
                MessageBox.Show("Данные успешно сохранены!");
                NavigationService?.GoBack();
            }
            catch (Exception ex)
            {
                if (_current != null) Core.DB.Entry(_current).Reload();
                MessageBox.Show(ex.InnerException?.InnerException?.Message ?? ex.Message);
                ;
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.GoBack();
        }
    }
}