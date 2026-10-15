using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Задание_43
{

    public partial class MainWindow : Window
    {
        private const int BasePrice = 500;
        private const int GiftPrice = 100;
        private const int ExpressPrice = 200;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Phone_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            foreach (char ch in e.Text)
            {
                if (!char.IsDigit(ch))
                {
                    e.Handled = true;
                    return;
                }
            }
        }
        private void Change(object sender, RoutedEventArgs e)
        {
            Recalculation();
        }
        private void Recalculation()
        {
            int quantity = (int)Sld.Value;
            int total = BasePrice * quantity;

            if (Gift.IsChecked == true)
            {
                total += GiftPrice;
            }

            if (Express.IsChecked == true)
            {
                total += ExpressPrice;
            }
            Total.Text = total.ToString();
        }
        private void Sld_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            double oldValue = e.OldValue;
            double newValue = e.NewValue;
            double diff = oldValue - newValue;

            if (IsInitialized)
            {
                Value.Text = ((int)Sld.Value).ToString();
                Recalculation();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Name.Text))
            {
                MessageBox.Show("Заполните поле имени");
                return;
            }

            if (string.IsNullOrWhiteSpace(Phone.Text))
            {
                MessageBox.Show("Заполните поле телефона");
                return;
            }

            string payMethod;
            if (Cash.IsChecked == true)
            {
                payMethod = "Наличные";
            }
            else if (Card.IsChecked == true)
            {
                payMethod = "Карта";
            }
            else if (Online.IsChecked == true)
            {
                payMethod = "Онлайн";
            }
            else
            {
                MessageBox.Show("Выберите способ оплаты");
                return;
            }

            string options = "";
            if (Gift.IsChecked == true)
            {
                options += "Подарочная упаковка";
            }
            if (Express.IsChecked == true)
            {
                options += "Срочная доставка";
            }
            if (string.IsNullOrEmpty(options))
            {
                options = "Без доп. опций";
            }

            int sld = (int)Sld.Value;

            string final =
                $"Имя: {Name.Text}\n" +
                $"Телефон: {Phone.Text}\n" +
                $"Способ оплаты: {payMethod}\n" +
                $"Дополнительно: {options}\n" +
                $"Количество: {sld} шт\n" +
                $"Итого: {Total.Text} ₽";

            MessageBox.Show($"{final}");
        }


    }
}
