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
    public partial class PatientsPage : Page
    {
        public PatientsPage()
        {
            InitializeComponent();
            var ctx = Core.DB;

            // Передаем список пациентов в DataGrid
            DgPatients.ItemsSource = Core.DB.patient.ToList();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            //Core.DB().Refresh();
            UpdateList();
        }

        private void Filter_Changed(object sender, EventArgs e) { UpdateList(); }

        private void UpdateList()
        {
            if (!IsInitialized) return;

            var list = Core.DB.patient.ToList().AsEnumerable();
            string q = TbSearch.Text.Trim().ToLower();
            if (q.Length > 0)
                list = list.Where(p => p.fio.ToLower().Contains(q) || (p.phone ?? "").ToLower().Contains(q));

            DgPatients.ItemsSource = list.OrderBy(p => p.id).ToList();
        }

        private void BtnReset_Click(object sender, RoutedEventArgs e) { TbSearch.Text = ""; }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new AddPatientPage(null));
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            var selected = DgPatients.SelectedItem as patient;
            if (selected == null) { MessageBox.Show("Выберите пациента в таблице."); return; }
            NavigationService?.Navigate(new AddPatientPage(selected));
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            var selected = DgPatients.SelectedItem as patient;
            if (selected == null) { MessageBox.Show("Выберите пациента в таблице."); return; }

            if (MessageBox.Show("Вы точно хотите удалить пациента «" + selected.fio + "»?\nБудут удалены его приёмы.",
                    "Внимание", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                return;

            try
            {
                Core.DB.patient.Remove(selected);
                Core.DB.SaveChanges();
                MessageBox.Show("Данные успешно удалены!");
                UpdateList();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}

