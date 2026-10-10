//using DurableTask.Core.Common;
//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Drawing;
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
//    /// <summary>Шаги 12-14: диаграмма по данным БД и экспорт отчётов в Word / Excel / PDF</summary>
//    public partial class ChartsPage : Page
//    {
//        private readonly Entities _context = Entities.GetContext();

//        public ChartsPage()
//        {
//            InitializeComponent();

//            // область построения и набор данных
//            ChartMain.ChartAreas.Add(new Charting.ChartArea("Main"));
//            ChartMain.Series.Add(new Charting.Series("Приёмы") { IsValueShownAsLabel = true });
//            ChartMain.Legends.Add(new Charting.Legend("Legend"));

//            // список врачей: первый элемент - «Все врачи»
//            var doctors = new List<Doctors> { new Doctors { id = 0, FIO = "Все врачи" } };
//            doctors.AddRange(_context.Doctors.ToList().OrderBy(d => d.FIO));
//            CmbDoctor.ItemsSource = doctors;

//            // типы диаграммы
//            CmbType.ItemsSource = new List<KeyValuePair<string, Charting.SeriesChartType>>
//            {
//                new KeyValuePair<string, Charting.SeriesChartType>("Столбчатая", Charting.SeriesChartType.Column),
//                new KeyValuePair<string, Charting.SeriesChartType>("Линейчатая", Charting.SeriesChartType.Bar),
//                new KeyValuePair<string, Charting.SeriesChartType>("Круговая",   Charting.SeriesChartType.Pie),
//                new KeyValuePair<string, Charting.SeriesChartType>("Кольцевая",  Charting.SeriesChartType.Doughnut),
//                new KeyValuePair<string, Charting.SeriesChartType>("График",     Charting.SeriesChartType.Line)
//            };

//            CmbDoctor.SelectedIndex = 0;
//            CmbType.SelectedIndex = 0;
//        }

//        // ---------------------------------------------------------------- Шаг 12: диаграмма
//        private void UpdateChart(object sender, SelectionChangedEventArgs e)
//        {
//            var doctor = CmbDoctor.SelectedItem as Doctors;
//            if (doctor == null || CmbType.SelectedValue == null) return;

//            var type = (Charting.SeriesChartType)CmbType.SelectedValue;
//            var series = ChartMain.Series.First();
//            series.ChartType = type;
//            series.Points.Clear();

//            bool isPie = type == Charting.SeriesChartType.Pie || type == Charting.SeriesChartType.Doughnut;
//            series.IsVisibleInLegend = isPie;
//            ChartMain.Legends[0].Enabled = isPie;
//            if (!isPie)
//                series.Color = ColorTranslator.FromHtml("#0198AF");     // бирюзовый из Руководства по стилю

//            var appointments = _context.Appointments.ToList();

//            if (doctor.id == 0)
//            {
//                // количество приёмов по врачам
//                foreach (var d in _context.Doctors.ToList().OrderBy(x => x.FIO))
//                {
//                    int count = appointments.Count(a => a.Schedule != null && a.Schedule.DoctorID == d.id);
//                    series.Points.AddXY(d.ShortName, count);
//                }
//            }
//            else
//            {
//                // приёмы выбранного врача по статусам
//                foreach (var status in new[] { "Запланирован", "Завершён", "Отменён" })
//                {
//                    int count = appointments.Count(a => a.Schedule != null && a.Schedule.DoctorID == doctor.id && a.Status == status);
//                    series.Points.AddXY(status, count);
//                }
//            }
//        }

//        // ---------------------------------------------------------------- Шаг 13: Word
//        private void BtnWord_Click(object sender, RoutedEventArgs e)
//        {
//            var dlg = new SaveFileDialog
//            {
//                Filter = "Документ Word (*.docx)|*.docx",
//                FileName = "Отчёт по приёмам " + DateTime.Now.ToString("yyyy-MM-dd") + ".docx"
//            };
//            if (dlg.ShowDialog() != true) return;

//            Word.Application app = null;
//            try
//            {
//                app = new Word.Application();
//                Word.Document document = BuildWordReport(app);
//                document.SaveAs2(dlg.FileName);
//                app.Visible = true;                           // показываем готовый документ
//            }
//            catch (Exception ex)
//            {
//                MessageBox.Show("Не удалось выполнить экспорт в Word.\nПроверьте, что Microsoft Word установлен.\n\n" + ex.Message,
//                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
//                try { if (app != null) app.Quit(); } catch { }
//            }
//        }

//        // ---------------------------------------------------------------- Шаг 13: PDF (через Word)
//        private void BtnPdf_Click(object sender, RoutedEventArgs e)
//        {
//            var dlg = new SaveFileDialog
//            {
//                Filter = "Документ PDF (*.pdf)|*.pdf",
//                FileName = "Отчёт по приёмам " + DateTime.Now.ToString("yyyy-MM-dd") + ".pdf"
//            };
//            if (dlg.ShowDialog() != true) return;

//            Word.Application app = null;
//            try
//            {
//                app = new Word.Application();
//                Word.Document document = BuildWordReport(app);
//                document.ExportAsFixedFormat(dlg.FileName, Word.WdExportFormat.wdExportFormatPDF);
//                document.Close(Word.WdSaveOptions.wdDoNotSaveChanges);
//                app.Quit();
//                Process.Start(new ProcessStartInfo(dlg.FileName) { UseShellExecute = true });
//            }
//            catch (Exception ex)
//            {
//                MessageBox.Show("Не удалось сохранить PDF.\nПроверьте, что Microsoft Word установлен.\n\n" + ex.Message,
//                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
//                try { if (app != null) app.Quit(); } catch { }
//            }
//        }

//        /// <summary>Формирует документ Word: по каждому врачу - таблица приёмов и итоги.
//        /// Если выбран конкретный врач, в отчёт попадает только он.</summary>
//        private Word.Document BuildWordReport(Word.Application application)
//        {
//            var selected = CmbDoctor.SelectedItem as Doctors;
//            var doctors = _context.Doctors.ToList().OrderBy(d => d.FIO).ToList();
//            if (selected != null && selected.id != 0)
//                doctors = doctors.Where(d => d.id == selected.id).ToList();

//            var appointments = _context.Appointments.ToList();

//            Word.Document document = application.Documents.Add();

//            // Заголовок отчёта
//            Word.Paragraph titleParagraph = document.Paragraphs.Add();
//            Word.Range titleRange = titleParagraph.Range;
//            titleRange.Text = "Отчёт по приёмам. Приём и ведение ЭМК";
//            titleRange.Font.Name = "Open Sans";
//            titleRange.Font.Size = 18;
//            titleRange.Font.Bold = 1;
//            titleRange.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
//            titleRange.InsertParagraphAfter();

//            foreach (var doctor in doctors)
//            {
//                var items = appointments
//                    .Where(a => a.Schedule != null && a.Schedule.DoctorID == doctor.id)
//                    .OrderBy(a => a.Date)
//                    .ToList();

//                // Название страницы - ФИО врача
//                Word.Paragraph docParagraph = document.Paragraphs.Add();
//                Word.Range docRange = docParagraph.Range;
//                docRange.Text = doctor.FIO + " (" + doctor.Specialty + ", кабинет " + doctor.Cabinet + ")";
//                docRange.Font.Name = "Open Sans";
//                docRange.Font.Size = 14;
//                docRange.Font.Bold = 1;
//                docRange.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphLeft;
//                docRange.InsertParagraphAfter();

//                if (items.Count > 0)
//                {
//                    // Таблица: Дата / Пациент / Статус
//                    Word.Paragraph tableParagraph = document.Paragraphs.Add();
//                    Word.Range tableRange = tableParagraph.Range;
//                    Word.Table table = document.Tables.Add(tableRange, items.Count + 1, 3);
//                    table.Borders.InsideLineStyle = table.Borders.OutsideLineStyle = Word.WdLineStyle.wdLineStyleSingle;
//                    table.Range.Cells.VerticalAlignment = Word.WdCellVerticalAlignment.wdCellAlignVerticalCenter;
//                    table.Range.Font.Name = "Open Sans";
//                    table.Range.Font.Size = 11;

//                    string[] headers = { "Дата и время", "Пациент", "Статус" };
//                    for (int c = 0; c < headers.Length; c++)
//                        table.Cell(1, c + 1).Range.Text = headers[c];
//                    table.Rows[1].Range.Font.Bold = 1;
//                    table.Rows[1].Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;

//                    for (int i = 0; i < items.Count; i++)
//                    {
//                        table.Cell(i + 2, 1).Range.Text = items[i].DateText;
//                        table.Cell(i + 2, 2).Range.Text = items[i].Patient != null ? items[i].Patient.FIO : "";
//                        table.Cell(i + 2, 3).Range.Text = items[i].Status;
//                    }
//                    document.Paragraphs.Add();                 // пустая строка
//                }

//                // Итоги по врачу
//                int planned = items.Count(a => a.Status == "Запланирован");
//                int done = items.Count(a => a.Status == "Завершён");
//                int canceled = items.Count(a => a.Status == "Отменён");

//                Word.Paragraph sumParagraph = document.Paragraphs.Add();
//                Word.Range sumRange = sumParagraph.Range;
//                sumRange.Text = "Всего приёмов: " + items.Count + " (запланировано: " + planned +
//                                ", завершено: " + done + ", отменено: " + canceled + ")";
//                sumRange.Font.Name = "Open Sans";
//                sumRange.Font.Size = 12;
//                sumRange.Font.Bold = 0;
//                sumRange.Font.Color = items.Count > 0 ? Word.WdColor.wdColorDarkGreen : Word.WdColor.wdColorDarkRed;
//                sumRange.InsertParagraphAfter();
//                document.Paragraphs.Add();

//                // разрыв страницы между врачами
//                if (doctor != doctors.Last())
//                    document.Words.Last.InsertBreak(Word.WdBreakType.wdPageBreak);
//            }

//            // Колонтитулы: номер страницы внизу, текущая дата вверху
//            foreach (Word.Section section in document.Sections)
//            {
//                Word.HeaderFooter footer = section.Footers[Word.WdHeaderFooterIndex.wdHeaderFooterPrimary];
//                footer.PageNumbers.Add(Word.WdPageNumberAlignment.wdAlignPageNumberCenter);

//                Word.Range headerRange = section.Headers[Word.WdHeaderFooterIndex.wdHeaderFooterPrimary].Range;
//                headerRange.Text = DateTime.Now.ToString("dd/MM/yyyy");
//                headerRange.Font.Size = 10;
//                headerRange.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphRight;
//            }

//            return document;
//        }

//        // ---------------------------------------------------------------- Шаг 14: Excel
//        private void BtnExcel_Click(object sender, RoutedEventArgs e)
//        {
//            var dlg = new SaveFileDialog
//            {
//                Filter = "Книга Excel (*.xlsx)|*.xlsx",
//                FileName = "Приёмы " + DateTime.Now.ToString("yyyy-MM-dd") + ".xlsx"
//            };
//            if (dlg.ShowDialog() != true) return;

//            Excel.Application excel = null;
//            try
//            {
//                var appointments = _context.Appointments.ToList().OrderBy(a => a.Date).ToList();
//                var doctors = _context.Doctors.ToList().OrderBy(d => d.FIO).ToList();

//                excel = new Excel.Application();
//                Excel.Workbook workbook = excel.Workbooks.Add();

//                // ----- Лист 1: все приёмы -----
//                Excel.Worksheet sheet = (Excel.Worksheet)workbook.Worksheets[1];
//                sheet.Name = "Приёмы";

//                string[] headers = { "id", "Пациент", "Врач", "Кабинет", "Дата и время", "Статус" };
//                for (int c = 0; c < headers.Length; c++)
//                    sheet.Cells[1, c + 1] = headers[c];

//                for (int i = 0; i < appointments.Count; i++)
//                {
//                    var a = appointments[i];
//                    sheet.Cells[i + 2, 1] = a.id;
//                    sheet.Cells[i + 2, 2] = a.Patient != null ? a.Patient.FIO : "";
//                    sheet.Cells[i + 2, 3] = a.Schedule != null && a.Schedule.Doctor != null ? a.Schedule.Doctor.FIO : "";
//                    sheet.Cells[i + 2, 4] = a.Cabinet;
//                    sheet.Cells[i + 2, 5] = a.DateText;
//                    sheet.Cells[i + 2, 6] = a.Status;
//                }
//                FormatHeader(sheet, headers.Length);
//                sheet.Columns.AutoFit();

//                // ----- Лист 2: сводка по врачам -----
//                Excel.Worksheet summary = (Excel.Worksheet)workbook.Worksheets.Add(After: sheet);
//                summary.Name = "По врачам";

//                string[] sumHeaders = { "Врач", "Запланировано", "Завершено", "Отменено", "Всего" };
//                for (int c = 0; c < sumHeaders.Length; c++)
//                    summary.Cells[1, c + 1] = sumHeaders[c];

//                for (int i = 0; i < doctors.Count; i++)
//                {
//                    var d = doctors[i];
//                    var own = appointments.Where(a => a.Schedule != null && a.Schedule.DoctorID == d.id).ToList();
//                    summary.Cells[i + 2, 1] = d.FIO;
//                    summary.Cells[i + 2, 2] = own.Count(a => a.Status == "Запланирован");
//                    summary.Cells[i + 2, 3] = own.Count(a => a.Status == "Завершён");
//                    summary.Cells[i + 2, 4] = own.Count(a => a.Status == "Отменён");
//                    summary.Cells[i + 2, 5] = own.Count;
//                }
//                FormatHeader(summary, sumHeaders.Length);
//                summary.Columns.AutoFit();

//                sheet.Activate();
//                workbook.SaveAs(dlg.FileName);
//                excel.Visible = true;
//            }
//            catch (Exception ex)
//            {
//                MessageBox.Show("Не удалось выполнить экспорт в Excel.\nПроверьте, что Microsoft Excel установлен.\n\n" + ex.Message,
//                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
//                try { if (excel != null) excel.Quit(); } catch { }
//            }
//        }

//        /// <summary>Шапка таблицы: тёмно-синий фон, белый жирный текст</summary>
//        private static void FormatHeader(Excel.Worksheet sheet, int columns)
//        {
//            Excel.Range header = sheet.Range[sheet.Cells[1, 1], sheet.Cells[1, columns]];
//            header.Font.Bold = true;
//            header.Font.Color = ColorTranslator.ToOle(Color.White);
//            header.Interior.Color = ColorTranslator.ToOle(ColorTranslator.FromHtml("#233E76"));
//        }
//    }
//}
