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

    /// <summary>Форма добавления / редактирования пациента</summary>
    public partial class AddPatientPage : Page
    {
        private readonly patient _current;

        public AddPatientPage(patient selected)
        {
            InitializeComponent();
            _current = selected;

            if (_current != null)
            {
                TbTitle.Text = "Изменение пациента";
                TbFio.Text = _current.fio;
                DpBirth.SelectedDate = _current.birth_date;
                TbPhone.Text = _current.phone;
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            string fio = TbFio.Text.Trim();
            if (fio.Length == 0) { ErrorText.Text = "Укажите ФИО пациента!"; return; }
            if (DpBirth.SelectedDate.HasValue && DpBirth.SelectedDate.Value > DateTime.Today)
            { ErrorText.Text = "Дата рождения не может быть в будущем!"; return; }

			var ctx = Core.DB;
			try
            {
                if (_current == null)
                {
                    Core.DB.patient.Add(new patient
                    {
                        fio = fio,
						birth_date = DpBirth.SelectedDate.Value,
						phone = string.IsNullOrWhiteSpace(TbPhone.Text) ? null : TbPhone.Text.Trim()
                    });
                }
                else
                {
                    _current.fio = fio;
					_current.birth_date = DpBirth.SelectedDate.Value;
					_current.phone = string.IsNullOrWhiteSpace(TbPhone.Text) ? null : TbPhone.Text.Trim();
                }
                Core.DB.SaveChanges();
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