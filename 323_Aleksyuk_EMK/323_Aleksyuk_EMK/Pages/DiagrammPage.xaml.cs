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
using System.Windows.Forms.DataVisualization.Charting;
using Word = Microsoft.Office.Interop.Word;
using Excel = Microsoft.Office.Interop.Excel;
using System.Collections.Generic;

namespace _323_Aleksyuk_EMK.Pages
{

    public partial class DiagrammPage : Page
    {
        private EMKEntities _context = Core.DB;

        public DiagrammPage()
        {
            InitializeComponent();

            ChartAppointments.ChartAreas.Add(new ChartArea("Main"));

            var currentSeries = new Series("Приёмы")
            {
                IsValueShownAsLabel = true
            };
            ChartAppointments.Series.Add(currentSeries);

            CmbDoctor.ItemsSource = _context.doctor.ToList();                      // ФИО врачей
            CmbDiagram.ItemsSource = Enum.GetValues(typeof(SeriesChartType));      // типы диаграммы
        }

        private void UpdateChart(object sender, SelectionChangedEventArgs e)
        {
            if (CmbDoctor.SelectedItem is doctor currentDoctor &&
                CmbDiagram.SelectedItem is SeriesChartType currentType)
            {
                Series currentSeries = ChartAppointments.Series.FirstOrDefault();
                currentSeries.ChartType = currentType;
                currentSeries.Points.Clear();

                var statuses = new[] { "Запланирован", "Завершён", "Отменён" };
                var appointments = _context.appointment.ToList();
                foreach (var status in statuses)
                {
                    currentSeries.Points.AddXY(status,
                        appointments.Count(a => a.schedule != null &&
                                                a.schedule.doctor_id == currentDoctor.id &&
                                                a.status == status));
                }
            }
        }
        private static string MakeSheetName(string fio, HashSet<string> used)
        {
            foreach (char c in new[] { ':', '\\', '/', '?', '*', '[', ']' })
                fio = fio.Replace(c, ' ');
            string name = fio.Length > 31 ? fio.Substring(0, 31) : fio;

            string unique = name;
            int n = 2;
            while (!used.Add(unique.ToLower()))
            {
                string suffix = " (" + n + ")";
                unique = (name.Length + suffix.Length > 31 ? name.Substring(0, 31 - suffix.Length) : name) + suffix;
                n++;
            }
            return unique;
        }
        private void BtnExcel_Click(object sender, RoutedEventArgs e)
        {
            var allDoctors = _context.doctor.ToList();
            if (allDoctors.Count == 0) return;
            var appointments = _context.appointment.ToList();

            var application = new Excel.Application();
            application.SheetsInNewWorkbook = allDoctors.Count;                 // по листу на врача
            Excel.Workbook workbook = application.Workbooks.Add(Type.Missing);
            var usedSheetNames = new HashSet<string> { "общий итог" };
            for (int i = 0; i < allDoctors.Count; i++)
            {
                var doctor = allDoctors[i];
                var worksheet = (Excel.Worksheet)application.Worksheets.Item[i + 1];
                worksheet.Name = MakeSheetName(doctor.fio, usedSheetNames);                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                     // имя листа: до 31 символа

                worksheet.Cells[1, 1] = "Дата и время";
                worksheet.Cells[1, 2] = "Пациент";
                worksheet.Cells[1, 3] = "Статус";
                worksheet.Range[worksheet.Cells[1, 1], worksheet.Cells[1, 3]].Font.Bold = true;

                var own = appointments
                    .Where(a => a.schedule != null && a.schedule.doctor_id == doctor.id)
                    .OrderBy(a => a.schedule.visit_date).ToList();

                int row = 2;
                foreach (var a in own)
                {
                    worksheet.Cells[row, 1] = a.schedule.visit_date;
                    worksheet.Cells[row, 2] = a.patient != null ? a.patient.fio : "";
                    worksheet.Cells[row, 3] = a.status;
                    row++;
                }

                worksheet.Cells[row + 1, 1] = "Всего приёмов:";
                worksheet.Cells[row + 1, 2] = own.Count;
                worksheet.Columns.AutoFit();
            }

            application.Visible = true;
        }

        private void BtnWord_Click(object sender, RoutedEventArgs e)
        {
            var allDoctors = _context.doctor.ToList();
            var allStatuses = new[] { "Запланирован", "Завершён", "Отменён" };
            var appointments = _context.appointment.ToList();

            var application = new Word.Application();
            Word.Document document = application.Documents.Add();

            foreach (var doctor in allDoctors)
            {
                // название страницы - ФИО врача
                Word.Paragraph doctorParagraph = document.Paragraphs.Add();
                Word.Range doctorRange = doctorParagraph.Range;
                doctorRange.Text = doctor.fio;
                doctorParagraph.set_Style("Times New Roman");      //Заголовок 1
                doctorRange.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                doctorRange.InsertParagraphAfter();
                document.Paragraphs.Add();                     // пустая строка

                // таблица: статус / количество приёмов
                Word.Paragraph tableParagraph = document.Paragraphs.Add();
                Word.Range tableRange = tableParagraph.Range;
                Word.Table table = document.Tables.Add(tableRange, allStatuses.Length + 1, 2);
                table.Borders.InsideLineStyle = table.Borders.OutsideLineStyle =
                    Word.WdLineStyle.wdLineStyleSingle;
                table.Range.Cells.VerticalAlignment = Word.WdCellVerticalAlignment.wdCellAlignVerticalCenter;

                Word.Range cellRange;
                cellRange = table.Cell(1, 1).Range;
                cellRange.Text = "Статус приёма";
                cellRange = table.Cell(1, 2).Range;
                cellRange.Text = "Количество приёмов";
                table.Rows[1].Range.Font.Name = "Times New Roman";
                table.Rows[1].Range.Font.Size = 14;
                table.Rows[1].Range.Bold = 1;
                table.Rows[1].Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;

                var own = appointments.Where(a => a.schedule != null && a.schedule.doctor_id == doctor.id).ToList();

                for (int i = 0; i < allStatuses.Length; i++)
                {
                    var currentStatus = allStatuses[i];
                    cellRange = table.Cell(i + 2, 1).Range;
                    cellRange.Text = currentStatus;
                    cellRange.Font.Name = "Times New Roman";
                    cellRange.Font.Size = 12;

                    cellRange = table.Cell(i + 2, 2).Range;
                    cellRange.Text = own.Count(a => a.status == currentStatus).ToString();
                    cellRange.Font.Name = "Times New Roman";
                    cellRange.Font.Size = 12;
                }
                document.Paragraphs.Add();                     // пустая строка

                // аналог максимального платежа: самый поздний приём (тёмно-красный)
                appointment lastAppointment = own.OrderByDescending(a => a.schedule.visit_date).FirstOrDefault();
                if (lastAppointment != null)
                {
                    Word.Paragraph lastParagraph = document.Paragraphs.Add();
                    Word.Range lastRange = lastParagraph.Range;
                    lastRange.Text = $"Последний приём - {lastAppointment.patient.fio} " +
                                     $"от {lastAppointment.schedule.visit_date.ToString("dd.MM.yyyy HH:mm")}";
                    lastParagraph.set_Style("Подзаголовок");
                    lastRange.Font.Color = Word.WdColor.wdColorDarkRed;
                    lastRange.InsertParagraphAfter();
                }
                document.Paragraphs.Add();

                // аналог минимального платежа: самый ранний приём (тёмно-зелёный)
                appointment firstAppointment = own.OrderBy(a => a.schedule.visit_date).FirstOrDefault();
                if (firstAppointment != null)      // в методичке тут опечатка: проверяется maxPayment
                {
                    Word.Paragraph firstParagraph = document.Paragraphs.Add();
                    Word.Range firstRange = firstParagraph.Range;
                    firstRange.Text = $"Первый приём - {firstAppointment.patient.fio} " +
                                      $"от {firstAppointment.schedule.visit_date.ToString("dd.MM.yyyy HH:mm")}";
                    firstParagraph.set_Style("Подзаголовок");
                    firstRange.Font.Color = Word.WdColor.wdColorDarkGreen;
                    firstRange.InsertParagraphAfter();
                }

                // разрыв страницы между врачами
                if (doctor != allDoctors.LastOrDefault())
                    document.Words.Last.InsertBreak(Word.WdBreakType.wdPageBreak);
            }

            // колонтитулы (пп. 3 и 4 методички)
            foreach (Word.Section section in document.Sections)
            {
                Word.HeaderFooter footer = section.Footers[Word.WdHeaderFooterIndex.wdHeaderFooterPrimary];
                footer.PageNumbers.Add(Word.WdPageNumberAlignment.wdAlignPageNumberCenter);

                Word.Range headerRange = section.Headers[Word.WdHeaderFooterIndex.wdHeaderFooterPrimary].Range;
                headerRange.Text = DateTime.Now.ToString("dd/MM/yyyy");
                headerRange.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                headerRange.Font.ColorIndex = Word.WdColorIndex.wdBlack;
                headerRange.Font.Size = 10;
            }

            application.Visible = true;

            // сохранение: путь укажите самостоятельно (в методичке D:\)
            document.SaveAs2(@"C:\Users\Admin\Desktop\Аксенова УП\Appointments.docx");
            document.SaveAs2(@"C:\Users\Admin\Desktop\Аксенова УП\Appointments.pdf", Word.WdExportFormat.wdExportFormatPDF);
        }
    }
}