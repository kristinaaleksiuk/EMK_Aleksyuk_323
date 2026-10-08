using DurableTask.Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
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
    /// <summary>
    /// Логика взаимодействия для AuthPage.xaml
    /// </summary>
    /// 
    public partial class AuthPage : Page
    {
        private int failedAttempts = 0;
        private user currentUser;
        public AuthPage()
        { }
        public void CaptchaSwitch()
        {
            switch (captcha.Visibility)
            {
                case Visibility.Visible:
                    TextBoxLogin.Clear();
                    PasswordBox.Clear();
                    captcha.Visibility = Visibility.Hidden; captchaInput.Visibility = Visibility.Hidden; labelCaptcha.Visibility = Visibility.Hidden; submitCaptcha.Visibility = Visibility.Hidden;
                    labelLogin.Visibility = Visibility.Visible; labelPass.Visibility = Visibility.Visible; TextBoxLogin.Visibility = Visibility.Visible; txtHintLogin_MouseLeftButtonUp.Visibility = Visibility.Visible; PasswordBox.Visibility = Visibility.Visible; txtHintPass.Visibility = Visibility.Visible;
                    ButtonChangePassword.Visibility = Visibility.Visible; ButtonEnter.Visibility = Visibility.Visible;
                    ButtonReg.Visibility = Visibility.Visible;
                    return;
                case Visibility.Hidden:
                    captcha.Visibility = Visibility.Visible;
                    captchaInput.Visibility = Visibility.Visible; labelCaptcha.Visibility = Visibility.Visible; submitCaptcha.Visibility = Visibility.Visible;
                    labelLogin.Visibility = Visibility.Hidden;
                    labelPass.Visibility = Visibility.Hidden;
                    TextBoxLogin.Visibility = Visibility.Hidden; txtHintLogin.Visibility = Visibility.Hidden; PasswordBox.Visibility = Visibility.Hidden; txtHintPass.Visibility = Visibility.Hidden;
                    ButtonChangePassword.Visibility = Visibility.Hidden; ButtonEnter.Visibility = Visibility.Hidden; ButtonReg.Visibility = Visibility.Hidden;
                    return;
            }
        }
public void CaptchaChange()
        {
            String allowchar = " ";
            allowchar = "A,B,C,D,E,F,G,H,I,J,K,L,M,N,O,P,Q,R,S,T,U,V,W,X,Y,Z"; allowchar += "a,b,c,d,e,f,g,h,i,j,k,l,m,n,o,p,q,r,s,t,u,v,w,y,z"; allowchar += "1,2,3,4,5,6,7,8,9,0";
            char[] a = { ',' };
            String[] ar = allowchar.Split(a);
            String pwd = "";
            string temp = "";
            Random r = new Random();
            for (int i = 0; i < 6; i++)
            {
                temp = ar[(r.Next(0, ar.Length))];
                pwd += temp;
            }
            captcha.Text = pwd;
        }


private void submitCaptcha_Click(object sender, RoutedEventArgs e)
        {
            if (captchaInput.Text != captcha.Text)
            {
                MessageBox.Show("Неверно введена капча", "Ошибка"); CaptchaChange();
            }
            else
            {
                MessageBox.Show("Капча введена успешно, можете продолжить  авторизацию", "Успех");
                CaptchaSwitch();
                failedAttempts = 0;
            }
        }
        
                CommandManager.PreviewExecuted="textBox_PreviewExecuted"
private void textBox_PreviewExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Command == ApplicationCommands.Copy ||
            e.Command == ApplicationCommands.Cut ||
            e.Command == ApplicationCommands.Paste)
            {
                e.Handled = true;
            }
        }

        public static string GetHash(String password)
        {
            using (var hash = SHA1.Create())
            {
                return
               string.Concat(hash.ComputeHash(Encoding.UTF8.GetBytes(password)).Select(x => x.ToString("X2")));
            }
        }
        private void ButtonEnter_OnClick(object sender, RoutedEventArgs e)
        {

            if (string.IsNullOrEmpty(TextBoxLogin.Text) || string.IsNullOrEmpty(PasswordBox.Password))
            {
                MessageBox.Show("Введите логин или пароль");
                return;
            }
            string hashedPassword = GetHash(PasswordBox.Password);
            using (var db = new EMKEntities())
            {
                var user = db.user
                .AsNoTracking()
                .FirstOrDefault(u => u.login == TextBoxLogin.Text && u.password == hashedPassword);
                if (user == null)
                {
                    MessageBox.Show("Пользователь с такими данными не  найден!");
                    failedAttempts++;
                    if (failedAttempts >= 3)
                    {
                        if (captcha.Visibility != Visibility.Visible)
                        {
                            CaptchaSwitch();
                        }
                        CaptchaChange();
                    }
                    return;
                }
                else
                {
                    MessageBox.Show("Пользователь успешно найден!");
                    switch (user.role)
                    {
                        case "User":
                            NavigationService?.Navigate(new Pages.UserPage()); break;
                        case "Admin":
                            NavigationService?.Navigate(new Pages.AdminPage()); break;
                    }
                }
            }
        }
        private void ButtonReg_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new RegPag());
        }
        private void ButtonChangePassword_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new ChangePass());
        }
        private void txtHintLogin_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            TextBoxLogin.Focus();
        }
        private void txtHintPass_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            PasswordBox.Focus();
        }
    }
}


