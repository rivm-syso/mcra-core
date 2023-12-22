using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace MCRA.Utils.Charting.OxyPlot {
    public class BarChartCreator : OxyPlotChartCreator {

        private readonly string _title;
        private IEnumerable<(string name, double contribution)> _data;

        public BarChartCreator(string title, IEnumerable<(string name, double contribution)> data) {
            _title = title;
            _data = data;
        }

        public override PlotModel Create() {
            var plotModel = createDefaultPlotModel(_title);
            var barSeries = new BarSeries() {
                IsStacked = false,
                StrokeThickness = 2,
                FillColor = OxyColor.FromAColor(50, OxyColors.DarkBlue),
                StrokeColor = OxyColors.DarkBlue,
            };

            var categoryAxis = new CategoryAxis() {
                GapWidth = .1,
                Position = AxisPosition.Left
            };

            foreach (var (name, contribution) in _data) {
                categoryAxis.Labels.Add($"{name}");
                barSeries.Items.Add(new BarItem(contribution, -1));
            }
            plotModel.Axes.Add(categoryAxis);

            var linearAxis = new LinearAxis() {
                MaximumPadding = 0.06,
                Maximum = 1,
                TickStyle = TickStyle.Inside,
                IsAxisVisible = true,
                Position = AxisPosition.Bottom,
                MinimumPadding = 0,
            };
            plotModel.Axes.Add(linearAxis);
            plotModel.Series.Add(barSeries);
            return plotModel;
        }
    }
}

