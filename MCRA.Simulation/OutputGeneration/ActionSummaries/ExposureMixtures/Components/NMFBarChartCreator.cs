using MCRA.Utils.Charting.OxyPlot;
using MCRA.Utils.ExtensionMethods;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class NMFBarChartCreator : OxyPlotChartCreator {

        private readonly ComponentSelectionOverviewSection _section;
        private readonly int _number;

        public NMFBarChartCreator(ComponentSelectionOverviewSection section, int number) {
            _section = section;
            _number = number;
            Height = 50;
            Width = 200;
            if (_section.SortedSubstancesComponentRecords.Count > 10) {
                Height += _section.SortedSubstancesComponentRecords.Count * 5;
            }
        }

        public override string ChartId {
            get {
                const string pictureId = "5373a2a4-98c1-43a3-b93d-ac3140c9c960";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId + _number);
            }
        }
        public override string Title => $"Co-exposure of substances, component {_number + 1}.";

        public override PlotModel Create() {
            var data = _section.SubstanceBarChartComponentRecords[_number];
            var plotModel = new PlotModel();
            var barSeries = new BarSeries() {
                IsStacked = false,
                StrokeThickness = 1,
                FillColor = OxyColor.FromAColor(50, OxyColors.Blue),
                StrokeColor = OxyColors.Blue,
            };

            var categoryAxis = new CategoryAxis() {
                GapWidth = .1,
                Position = AxisPosition.Left
            };
            foreach (var item in data) {
                categoryAxis.Labels.Add(item.SubstanceName);
                barSeries.Items.Add(new BarItem(item.NmfValue, -1));
            }
            plotModel.Axes.Add(categoryAxis);

            var linearAxis = new LinearAxis() {
                MaximumPadding = 0.06,
                Maximum = 1,
                TickStyle = TickStyle.None,
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
