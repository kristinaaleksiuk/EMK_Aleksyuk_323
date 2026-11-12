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
using System.IO;

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
                var appointments = InPeriod(_context.appointment.ToList()).ToList();
                foreach (var status in statuses)
                {
                    currentSeries.Points.AddXY(status,
                        appointments.Count(a => a.schedule != null &&
                                                a.schedule.doctor_id == currentDoctor.id &&
                                                a.status == status));
                }
            }
        }
        // ---------- период и статистика ----------
        private IEnumerable<appointment> InPeriod(IEnumerable<appointment> source)
        {
            DateTime? dFrom = DpFrom.SelectedDate, dTo = DpTo.SelectedDate;
            return source.Where(a => a.schedule != null &&
                                     (dFrom == null || a.schedule.visit_date.Date >= dFrom.Value.Date) &&
                                     (dTo == null || a.schedule.visit_date.Date <= dTo.Value.Date));
        }

        private string PeriodText()
        {
            DateTime? dFrom = DpFrom.SelectedDate, dTo = DpTo.SelectedDate;
            if (!dFrom.HasValue && !dTo.HasValue) return "за всё время";
            return "за период " + (dFrom.HasValue ? dFrom.Value.ToString("dd.MM.yyyy") : "…") + " — " +
                   (dTo.HasValue ? dTo.Value.ToString("dd.MM.yyyy") : "…");
        }

        private void BtnResetPeriod_Click(object sender, RoutedEventArgs e)
        {
            DpFrom.SelectedDate = null;
            DpTo.SelectedDate = null;
        }

        private class DiagStat
        {
            public string Code { get; set; }
            public string Name { get; set; }
            public int Count { get; set; }
        }

        /// <summary>Сколько раз ставился каждый диагноз на завершённых приёмах за период.</summary>
        private List<DiagStat> GetDiagnosisStats()
        {
            DateTime? dFrom = DpFrom.SelectedDate, dTo = DpTo.SelectedDate;
            var visits = _context.visit.ToList().Where(v =>
                v.appointment != null && v.appointment.schedule != null && v.appointment.status == "Завершён" &&
                (dFrom == null || v.appointment.schedule.visit_date.Date >= dFrom.Value.Date) &&
                (dTo == null || v.appointment.schedule.visit_date.Date <= dTo.Value.Date));

            return visits.SelectMany(v => v.diagnosis)
                .GroupBy(d => d.id)
                .Select(g => new DiagStat { Code = g.First().icd10_code, Name = g.First().name, Count = g.Count() })
                .OrderByDescending(x => x.Count).ThenBy(x => x.Code)
                .ToList();
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
            try { ExportExcel(); }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось сформировать отчёт: " + ex.Message + "\nПроверьте, что Microsoft Excel установлен.");
            }
        }
        private void ExportExcel()
        {

            var allDoctors = _context.doctor.ToList();
            if (allDoctors.Count == 0) return;
            var appointments = InPeriod(_context.appointment.ToList()).ToList();

            var application = new Excel.Application();
            application.SheetsInNewWorkbook = allDoctors.Count + 1;      // по листу на врача + лист диагнозов
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
                    worksheet.Cells[row, 1] = a.schedule.visit_date.ToString("dd.MM.yyyy") + " " + a.schedule.time_start.ToString(@"hh\:mm");
                    worksheet.Cells[row, 2] = a.patient != null ? a.patient.fio : "";
                    worksheet.Cells[row, 3] = a.status;
                    row++;
                }
                // лист со статистикой по диагнозам (завершённые приёмы за период)
                var diagSheet = (Excel.Worksheet)application.Worksheets.Item[allDoctors.Count + 1];
                diagSheet.Name = MakeSheetName("Диагнозы", usedSheetNames);
                diagSheet.Cells[1, 1] = "Код МКБ-10";
                diagSheet.Cells[1, 2] = "Диагноз";
                diagSheet.Cells[1, 3] = "Количество";
                diagSheet.Range[diagSheet.Cells[1, 1], diagSheet.Cells[1, 3]].Font.Bold = true;
                int drow = 2;
                foreach (var st in GetDiagnosisStats())
                {
                    diagSheet.Cells[drow, 1] = st.Code;
                    diagSheet.Cells[drow, 2] = st.Name;
                    diagSheet.Cells[drow, 3] = st.Count;
                    drow++;
                }
                diagSheet.Cells[drow + 1, 1] = "Период:";
                diagSheet.Cells[drow + 1, 2] = PeriodText();
                diagSheet.Columns.AutoFit();

                worksheet.Cells[row + 1, 1] = "Всего приёмов:";
                worksheet.Cells[row + 1, 2] = own.Count;
                worksheet.Columns.AutoFit();
            }

            application.Visible = true;
        }

        private void BtnWord_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Документ Word (*.docx)|*.docx",
                FileName = "Отчёт по приёмам " + DateTime.Now.ToString("yyyy-MM-dd")
            };
            if (dlg.ShowDialog() != true) return;

            try { ExportWord(dlg.FileName, System.IO.Path.ChangeExtension(dlg.FileName, ".pdf")); }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось сформировать отчёт: " + ex.Message + "\nПроверьте, что Microsoft Word установлен.");
            }
        }

        private void ExportWord(string docxPath, string pdfPath)
        {
            var allDoctors = _context.doctor.ToList();
            var allStatuses = new[] { "Запланирован", "Завершён", "Отменён" };
            var appointments = InPeriod(_context.appointment.ToList()).ToList();
            var application = new Word.Application();
            Word.Document document = application.Documents.Add();

            foreach (var doctor in allDoctors)
            {
                // название страницы - ФИО врача
                Word.Paragraph doctorParagraph = document.Paragraphs.Add();
                Word.Range doctorRange = doctorParagraph.Range;
                doctorRange.Text = doctor.fio;
                doctorParagraph.set_Style("Заголовок 1");      
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

                // самый поздний приём (тёмно-красный)
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

                // самый ранний приём (тёмно-зелёный)
                appointment firstAppointment = own.OrderBy(a => a.schedule.visit_date).FirstOrDefault();
                if (firstAppointment != null)
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

            // статистика по диагнозам (завершённые приёмы за период)
            var stats = GetDiagnosisStats();
            if (allDoctors.Count > 0) document.Words.Last.InsertBreak(Word.WdBreakType.wdPageBreak);
            Word.Paragraph statTitle = document.Paragraphs.Add();
            Word.Range statTitleRange = statTitle.Range;
            statTitleRange.Text = "Статистика по диагнозам " + PeriodText();
            object headingStyle = Word.WdBuiltinStyle.wdStyleHeading1;
            statTitle.set_Style(headingStyle);
            statTitleRange.InsertParagraphAfter();

            if (stats.Count == 0)
            {
                Word.Paragraph none = document.Paragraphs.Add();
                none.Range.Text = "За выбранный период завершённых приёмов с диагнозами нет.";
            }
            else
            {
                Word.Paragraph statTableParagraph = document.Paragraphs.Add();
                Word.Table statTable = document.Tables.Add(statTableParagraph.Range, stats.Count + 1, 3);
                statTable.Borders.InsideLineStyle = statTable.Borders.OutsideLineStyle = Word.WdLineStyle.wdLineStyleSingle;
                statTable.Cell(1, 1).Range.Text = "Код МКБ-10";
                statTable.Cell(1, 2).Range.Text = "Диагноз";
                statTable.Cell(1, 3).Range.Text = "Количество";
                statTable.Rows[1].Range.Bold = 1;
                for (int i = 0; i < stats.Count; i++)
                {
                    statTable.Cell(i + 2, 1).Range.Text = stats[i].Code;
                    statTable.Cell(i + 2, 2).Range.Text = stats[i].Name;
                    statTable.Cell(i + 2, 3).Range.Text = stats[i].Count.ToString();
                }
                statTable.Range.Font.Name = "Times New Roman"; statTable.Range.Font.Size = 12;
            }

            // колонтитулы: номер страницы и дата

            foreach (Word.Section section in document.Sections)
            {
                Word.HeaderFooter footer = section.Footers[Word.WdHeaderFooterIndex.wdHeaderFooterPrimary];
                footer.PageNumbers.Add(Word.WdPageNumberAlignment.wdAlignPageNumberCenter);

                Word.Range headerRange = section.Headers[Word.WdHeaderFooterIndex.wdHeaderFooterPrimary].Range;
                headerRange.Text = DateTime.Now.ToString("dd/MM/yyyy") + "  |  Отчёт по приёмам " + PeriodText();
                headerRange.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                headerRange.Font.ColorIndex = Word.WdColorIndex.wdBlack;
                headerRange.Font.Size = 10;
            }

            application.Visible = true;

            document.SaveAs2(docxPath);
            document.SaveAs2(pdfPath, Word.WdExportFormat.wdExportFormatPDF);
            MessageBox.Show("Отчёт сохранён:" + docxPath + "" + pdfPath);

        }
    }
}