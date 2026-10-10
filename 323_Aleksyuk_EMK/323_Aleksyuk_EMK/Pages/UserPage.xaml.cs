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
    public partial class UserPage : Page
{
    public UserPage()
    {
        InitializeComponent();
    }

    private void Page_Loaded(object sender, RoutedEventArgs e)
    {
        var user = Core.CurrentUser;
        if (user == null) { NavigationService?.Navigate(new AuthPage()); return; }

        var Core.DB = Entities.GetContext();
        Core.DB.Refresh();

        TbName.Text = user.fio;
        TbInfo.Text = "Роль: " + user.Role + "  |  Логин: " + user.login;
        PhotoHelper.Apply(PhotoEllipse, PhotoHint, user.Photo);

        int uid = user.id;
        var patientIds = Core.DB.patient.Where(p => p.UserID == uid).Select(p => p.id).ToList();
        DgAppointments.ItemsSource = Core.DB.appointment
            .Where(a => patientIds.Contains(a.PatientID))
            .ToList()
            .OrderByDescending(a => a.Date)
            .ToList();
    }

    private void BtnAdd_Click(object sender, RoutedEventArgs e)
    {
        NavigationService?.Navigate(new AddAppointmentPage(null));
    }

    private void BtnChangePass_Click(object sender, RoutedEventArgs e)
    {
        NavigationService?.Navigate(new ChangePasswordPage());
    }

    private void BtnLogout_Click(object sender, RoutedEventArgs e)
    {
        NavigationService?.Navigate(new AuthPage());
    }
}
}
