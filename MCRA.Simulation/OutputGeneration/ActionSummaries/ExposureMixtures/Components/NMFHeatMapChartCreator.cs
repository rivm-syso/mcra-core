using MCRA.Utils.Charting.OxyPlot;
using MCRA.Utils.ExtensionMethods;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class NMFHeatMapChartCreator : ReportChartCreatorBase {

        private ComponentSelectionOverviewSection _section;

        public override string ChartId {
            get {
                var pictureId = "146dd48e-f080-473b-ba80-d521eb39379d";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId);
            }
        }

        public override string Title => "Co-exposure of substances.";

        public NMFHeatMapChartCreator(ComponentSelectionOverviewSection section) {
            _section = section;
            Height = 600;
            if (_section.SortedSubstancesComponentRecords.Count > 20) {
                Height = 600 + _section.SortedSubstancesComponentRecords.Count * 5;
            }
        }

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
                        data[count, ix] = record.First(c => c.SubstanceCode == item.SubstanceCode).NmfValue;
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

            var heatMapSeries = new HeatMapSeries() {
                Interpolate = false,
                X0 = 0,
                X1 = matrix.GetLength(0),
                Y0 = 0,
                Y1 = matrix.GetLength(1),
                LabelFontSize = 0,
                Data = matrix,
                RenderMethod = HeatMapRenderMethod.Rectangles,
            };

            plotModel.PlotAreaBorderThickness = new OxyThickness(1, 1, 1, 1);
            plotModel.PlotMargins = new OxyThickness(100, double.NaN, 100, double.NaN);

            var linearColorAxis1 = new LinearColorAxis();
            linearColorAxis1.HighColor = OxyColors.Red;
            linearColorAxis1.LowColor = OxyColors.White;
            linearColorAxis1.Position = AxisPosition.Right;
            plotModel.Axes.Add(linearColorAxis1);

            linearColorAxis1.Palette = OxyPalette.Interpolate(100, OxyColors.White, OxyColors.Yellow, OxyColors.Red);
            linearColorAxis1.Palette.Colors[0] = OxyColor.FromAColor(50,  OxyColors.ForestGreen);
            var linearAxis1 = new LinearAxis();
            linearAxis1.Title = "Substances";
            linearAxis1.TickStyle = TickStyle.Inside;
            linearAxis1.IsAxisVisible = false;
            linearAxis1.Position = AxisPosition.Left;
            linearAxis1.MaximumPadding = 0;
            linearAxis1.MinimumPadding = 0;
            plotModel.Axes.Add(linearAxis1);

            var linearAxis2 = new LinearAxis();
            linearAxis2.Title = "K";
            linearAxis2.IsAxisVisible = false;
            linearAxis2.TickStyle = TickStyle.None;
            linearAxis2.Position = AxisPosition.Bottom;
            linearAxis2.TitlePosition = 1;
            linearAxis2.MaximumPadding = 0;
            linearAxis2.MinimumPadding = 0;
            plotModel.Axes.Add(linearAxis2);

            var categoryAxis1 = new CategoryAxis();
            categoryAxis1.MinorStep = 1;
            categoryAxis1.Position = AxisPosition.Left;
            foreach (var name in names) {
                categoryAxis1.Labels.Add(name);
            }
            plotModel.Axes.Add(categoryAxis1);

            var categoryAxis2 = new CategoryAxis();
            categoryAxis2.MinorStep = 1;
            categoryAxis2.Position = AxisPosition.Bottom;
            for (int i = 0; i < matrix.GetLength(0); i++) {
                var labelK = $"{i + 1}";
                categoryAxis2.Labels.Add(labelK);
            }
            plotModel.Axes.Add(categoryAxis2);

            plotModel.Series.Add(heatMapSeries);
            return plotModel;
        }
    }
}
