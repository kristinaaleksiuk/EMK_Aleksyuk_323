//using DurableTask.Core.Common;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.Windows;
//using System.Windows.Controls;
//using System.Windows.Data;
//using System.Windows.Documents;
//using System.Windows.Input;
//using System.Windows.Media;
//using System.Windows.Media.Imaging;
//using System.Windows.Navigation;
//using System.Windows.Shapes;

//namespace _323_Aleksyuk_EMK.Pages
//{
//    /// <summary>
//    /// Логика взаимодействия для UserPage.xaml
//    /// </summary>
//    public partial class UserPage : Page
//    {
//        // Инициализируем контекст (имя класса проверьте в Model1.Context.cs!)
//        private EMKEntities Context = new EMKEntities();

//        public UserPage()
//        {
//            InitializeComponent();
//            LoadComboBoxData(); // Заполнение фильтров
//            RefreshData();
//        }

//        private void LoadComboBoxData()
//        {
//            // Фильтр, например, по Категориям
//            var categories = EMKEntities.categoris.ToList();
//            categories.Insert(0, new Category { Name = "Все категории" });
//            ComboFilter.ItemsSource = categories;
//            ComboFilter.SelectedIndex = 0;

//            // Комбобокс для сортировки
//            ComboSort.ItemsSource = new string[] { "Без сортировки", "По возрастанию цены", "По убыванию цены" };
//            ComboSort.SelectedIndex = 0;
//        }

//        private void RefreshData()
//        {
//            // 1. Берем исходный список из БД (например, Products)
//            var currentData = _context.Products.ToList();

//            // 2. ПОИСК (по названию или описанию)
//            if (!string.IsNullOrWhiteSpace(TxtSearch.Text))
//            {
//                string search = TxtSearch.Text.ToLower();
//                currentData = currentData.Where(p => p.Name.ToLower().Contains(search) ||
//                                                     p.Description.ToLower().Contains(search)).ToList();
//            }

//            // 3. ФИЛЬТРАЦИЯ
//            if (ComboFilter.SelectedIndex > 0)
//            {
//                var selectedCategory = ComboFilter.SelectedItem as Category;
//                currentData = currentData.Where(p => p.CategoryID == selectedCategory.ID).ToList();
//            }

//            // 4. СОРТИРОВКА
//            switch (ComboSort.SelectedIndex)
//            {
//                case 1:
//                    currentData = currentData.OrderBy(p => p.Price).ToList();
//                    break;
//                case 2:
//                    currentData = currentData.OrderByDescending(p => p.Price).ToList();
//                    break;
//            }

//            // Назначаем отфильтрованный список в ваш DataGrid
//            MainDataGrid.ItemsSource = currentData;
//        }

//        // Навешиваем вызов RefreshData на события изменения элементов интерфейса
//        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e) => RefreshData();
//        private void ComboFilter_SelectionChanged(object sender, SelectionChangedEventArgs e) => RefreshData();
//        private void ComboSort_SelectionChanged(object sender, SelectionChangedEventArgs e) => RefreshData();
//    }
//}
