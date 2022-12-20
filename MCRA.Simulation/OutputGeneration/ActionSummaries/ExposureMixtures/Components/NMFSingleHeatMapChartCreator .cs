using MCRA.Utils.Charting.OxyPlot;
using MCRA.Utils.ExtensionMethods;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace MCRA.Simulation.OutputGeneration {

    /// <summary>
    /// Work around. This is a surrogate for heatplots when the number of components is equal to 1,
    /// For 1 component the normal heat plot crashes.
    /// </summary>
    public sealed class NMFSingleHeatMapChartCreator : OxyPlotChartCreator {

        private ComponentSelectionOverviewSection _section;

        public NMFSingleHeatMapChartCreator(ComponentSelectionOverviewSection section) {
            _section = section;
            Height = 600;
            if (_section.SortedSubstancesComponentRecords.Count > 20) {
                Height = 600 + _section.SortedSubstancesComponentRecords.Count * 5;
            }
        }

        public override string ChartId {
            get {
                var pictureId = "d34795a5-351b-4b12-9fe8-2fd5ec4d743d";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId);
            }
        }
        public override string Title => "Co-exposure of substances.";

        public override PlotModel Create() {
            var numberOfComponents = _section.SubstanceComponentRecords.Count;

            List<SubstanceComponentRecord> selectedForPlot = null;
            selectedForPlot = _section.SortedSubstancesComponentRecords;
            var numberOfCompounds = selectedForPlot.Count;

            var data = new double[numberOfComponents, numberOfCompounds];
            foreach (var item in selectedForPlot) {
                var count = 0;
                foreach (var record in _section.SubstanceComponentRecords) {
                    if (record.Select(c => c.SubstanceCode).Contains(item.SubstanceCode)) {
                        var ix = selectedForPlot.IndexOf(item);
                        data[count, ix] = record.Where(c => c.SubstanceCode == item.SubstanceCode).First().NmfValue;
                    }
                    count++;
                }
            }
            return create(data, selectedForPlot.Select(c => c.SubstanceName).ToList());
        }

        private static PlotModel create(double[,] matrix, List<string> names) {
            var plotModel = new PlotModel() {
                IsLegendVisible = false,
            };

            var n = matrix.GetLength(1);
            var max = Math.Max(matrix[0, 0], matrix[0, n - 1]);
            var linearColorAxis1 = new LinearColorAxis();
            linearColorAxis1.HighColor = OxyColors.Red;
            linearColorAxis1.LowColor = OxyColors.White;
            linearColorAxis1.Position = AxisPosition.Right;
            linearColorAxis1.Maximum = max;
            plotModel.Axes.Add(linearColorAxis1);

            linearColorAxis1.Palette = OxyPalette.Interpolate(100, OxyColors.White, OxyColors.Yellow, OxyColors.Red);
            var multiplier = 100 / max;
            for (int i = 0; i < matrix.GetLength(1); i++) {
                var series = new ColumnSeries();
                series.IsStacked = true;
                series.StrokeThickness = 0;
                var ix = (int)Math.Floor(multiplier * matrix[0, i]) - 1;
                series.FillColor = linearColorAxis1.Palette.Colors[ix];
                series.Items.Add(new ColumnItem(1.0 / n));
                plotModel.Series.Add(series);
            }
            plotModel.PlotAreaBorderThickness = new OxyThickness(1, 1, 1, 1);
            plotModel.PlotMargins = new OxyThickness(100, double.NaN, 100, double.NaN);

            var linearAxis1 = new LinearAxis();
            linearAxis1.Title = "Substances";
            linearAxis1.TickStyle = TickStyle.Inside;
            linearAxis1.IsAxisVisible = false;
            linearAxis1.Position = AxisPosition.Left;
            linearAxis1.MaximumPadding = 0;
            linearAxis1.MinimumPadding = 0;
            plotModel.Axes.Add(linearAxis1);

            var categoryAxis1 = new CategoryAxis();
            categoryAxis1.MinorStep = 1;
            categoryAxis1.Position = AxisPosition.Left;
            foreach (var name in names) {
                categoryAxis1.Labels.Add(name);
            }
            plotModel.Axes.Add(categoryAxis1);

            var categoryAxis2 = new CategoryAxis();
            categoryAxis2.GapWidth = 0;
            categoryAxis2.MinorStep = 1;
            categoryAxis2.Position = AxisPosition.Bottom;
            categoryAxis2.Labels.Add("1");
            plotModel.Axes.Add(categoryAxis2);
            return plotModel;
        }
    }
}
