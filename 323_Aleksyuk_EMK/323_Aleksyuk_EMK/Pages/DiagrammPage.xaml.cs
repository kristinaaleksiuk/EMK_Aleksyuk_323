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
        private void BtnExcel_Click(object sender, RoutedEventArgs e)
        {
            //  Excel
        }

        private void BtnWord_Click(object sender, RoutedEventArgs e)
        {
            // Word
        }
    }
}