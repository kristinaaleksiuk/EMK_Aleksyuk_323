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
            Core.Context = new EMKEntities();
            UpdateList();
        }

        private void Filter_Changed(object sender, EventArgs e) { UpdateList(); }

        private void UpdateList()
        {
            if (!IsInitialized) return;

            var list = Core.DB.user.ToList().AsEnumerable();

            string q = TbSearch.Text.Trim().ToLower();
            if (q.Length > 0)
                list = list.Where(u => u.fio.ToLower().Contains(q) || u.login.ToLower().Contains(q));

            var role = ((ComboBoxItem)CbRole.SelectedItem)?.Content.ToString();
            if (role == "Admin" || role == "Doctor" || role == "User")
                list = list.Where(u => u.role == role);

            DgUsers.ItemsSource = list.OrderBy(u => u.id).ToList();
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
            var selected = DgUsers.SelectedItem as user;
            if (selected == null) { MessageBox.Show("Выберите пользователя в таблице."); return; }
            NavigationService?.Navigate(new AddUserPage(selected));
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            var selected = DgUsers.SelectedItem as user;
            if (selected == null) { MessageBox.Show("Выберите пользователя в таблице."); return; }

            if (Core.CurrentUser == null || selected.id != Core.CurrentUser.id)
            {
                if (MessageBox.Show("Вы точно хотите удалить пользователя «" + selected.fio + "»?\nБудут удалены связанные пациенты и приёмы.",
                        "Внимание", MessageBoxButton.YesNo, MessageBoxImage.Question) 
                    != MessageBoxResult.Yes)
                    return;

                try
                {
                    Core.DB.user.Remove(selected);
                    Core.DB.SaveChanges();
                    MessageBox.Show("Данные успешно удалены!");
                    UpdateList();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else
            {
                MessageBox.Show("Нельзя удалить учётную запись, под которой вы вошли.");
                return;
            }
        }
    }
}

