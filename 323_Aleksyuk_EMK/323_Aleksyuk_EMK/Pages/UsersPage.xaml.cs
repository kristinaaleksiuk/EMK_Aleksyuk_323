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
    /// <summary>Шаг 10: список пользователей, полный CRUD (таблица User)</summary>
    public partial class UsersPage : Page
    {
        public UsersPage()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Entities.GetContext().Refresh();     // обновляем данные при каждом показе страницы
            UpdateList();
        }

        private void Filter_Changed(object sender, EventArgs e) { UpdateList(); }

        private void UpdateList()
        {
            if (!IsInitialized) return;

            var list = Entities.GetContext().Users.ToList().AsEnumerable();

            string q = TbSearch.Text.Trim().ToLower();
            if (q.Length > 0)
                list = list.Where(u => u.FIO.ToLower().Contains(q) || u.Login.ToLower().Contains(q));

            var role = ((ComboBoxItem)CbRole.SelectedItem)?.Content.ToString();
            if (role == "Admin" || role == "User")
                list = list.Where(u => u.Role == role);

            DgUsers.ItemsSource = list.OrderBy(u => u.ID).ToList();
        }

        private void BtnReset_Click(object sender, RoutedEventArgs e)
        {
            TbSearch.Text = "";
            CbRole.SelectedIndex = 0;
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new AddUserPage(null));
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            var selected = DgUsers.SelectedItem as Users;
            if (selected == null) { MessageBox.Show("Выберите пользователя в таблице."); return; }
            NavigationService?.Navigate(new AddUserPage(selected));
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            var selected = DgUsers.SelectedItem as Users;
            if (selected == null) { MessageBox.Show("Выберите пользователя в таблице."); return; }

            if (Session.CurrentUser != null && selected.ID == Session.CurrentUser.ID)
            {
                MessageBox.Show("Нельзя удалить учётную запись, под которой вы вошли.");
                return;
            }

            if (MessageBox.Show("Вы точно хотите удалить пользователя «" + selected.FIO + "»?\nБудут удалены связанные пациенты и приёмы.",
                    "Внимание", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                return;

            try
            {
                DbOps.DeleteUser(selected);                // каскадное удаление
                Entities.GetContext().SaveChanges();
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

