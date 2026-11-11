using System;
using System.Collections.Generic;
using System.IO;
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
    /// <summary>Просмотр, печать и сохранение выходного документа (талон, направление, рецепт).</summary>
    public partial class PrintPage : Page
    {
        private readonly string _title;
        private readonly string _text;
        private readonly Func<Page> _closeTo;

        public PrintPage(string title, string text, Func<Page> closeTo = null)
        {
            InitializeComponent();
            _title = title;
            _text = text;
            _closeTo = closeTo;
            Title = "ЭМК — " + title;
            TbTitle.Text = title;
            TbDoc.Text = text;
        }

        private void BtnPrint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dlg = new System.Windows.Controls.PrintDialog();
                if (dlg.ShowDialog() != true) return;

                var doc = new FlowDocument
                {
                    PagePadding = new Thickness(60),
                    FontFamily = new FontFamily("Consolas"),
                    FontSize = 13,
                    PageWidth = dlg.PrintableAreaWidth,
                    PageHeight = dlg.PrintableAreaHeight,
                    ColumnWidth = dlg.PrintableAreaWidth
                };
                foreach (var line in _text.Replace("\r", "").Split('\n'))
                    doc.Blocks.Add(new Paragraph(new Run(line.Length == 0 ? " " : line)) { Margin = new Thickness(0) });

                dlg.PrintDocument(((IDocumentPaginatorSource)doc).DocumentPaginator, _title);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось напечатать документ: " + ex.Message);
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Текстовый файл (*.txt)|*.txt",
                FileName = _title
            };
            if (dlg.ShowDialog() != true) return;

            try
            {
                File.WriteAllText(dlg.FileName, _text, new UTF8Encoding(true));
                MessageBox.Show("Файл сохранён.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось сохранить файл: " + ex.Message);
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            if (_closeTo != null) NavigationService?.Navigate(_closeTo());
            else NavigationService?.GoBack();
        }
    }
}