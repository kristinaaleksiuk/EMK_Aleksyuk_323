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
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace _323_Aleksyuk_EMK.Pages
{
    public partial class AdminPage : Page
    {
        public AdminPage()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            var user = Core.CurrentUser;
            if (user == null) { NavigationService?.Navigate(new AuthPage()); return; }

            TbName.Text = user.fio;
            TbInfo.Text = "Роль: " + user.role;

            // Закомментировали строку с фото, так как оно необязательно
            // PhotoHelper.Apply(PhotoEllipse, PhotoHint, user.photo);
        }
        private void BtnUsers_Click(object sender, RoutedEventArgs e) 
        { NavigationService?.Navigate(new UsersPage()); }
        private void BtnPatients_Click(object sender, RoutedEventArgs e)
        { NavigationService?.Navigate(new PatientsPage()); }
        private void BtnAppointments_Click(object sender, RoutedEventArgs e) 
        { NavigationService?.Navigate(new AddAppointmentPage(null)); }
        private void BtnCharts_Click(object sender, RoutedEventArgs e) 
        { NavigationService?.Navigate(new ChartsPage()); }
        private void BtnChangePass_Click(object sender, RoutedEventArgs e) 
        { NavigationService?.Navigate(new ChangePassPage()); }
        private void BtnLogout_Click(object sender, RoutedEventArgs e) 
        { NavigationService?.Navigate(new AuthPage()); }
        private void BtnDiagramma_Click(object sender, RoutedEventArgs e)
        { NavigationService?.Navigate(new DiagrammPage()); }
    }
}
