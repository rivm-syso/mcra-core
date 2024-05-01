using MCRA.Utils.ExtensionMethods;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace MCRA.Simulation.OutputGeneration {

    /// <summary>
    /// Work around. This is a surrogate for heatplots when the number of components is equal to 1,
    /// For 1 component the normal heat plot crashes.
    /// </summary>
    public sealed class NMFSingleHeatMapChartCreator : ReportChartCreatorBase {

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
                        data[count, ix] = record.First(c => c.SubstanceCode == item.SubstanceCode).NmfValue;
                    }
                    count++;
                }
            }
            return create(data, selectedForPlot.Select(c => c.SubstanceName).ToList());
        }

        private static PlotModel create(double[,] matrix, List<string> names) {
            var plotModel = new PlotModel {
                IsLegendVisible = false,
                PlotMargins = new OxyThickness(100, double.NaN, 100, double.NaN)
            };

            // Right: linear gradient color axis
            var n = matrix.GetLength(1);
            var max = Math.Max(matrix[0, 0], matrix[0, n - 1]);
            var linearColorAxisRight = new LinearColorAxis {
                HighColor = OxyColors.Red,
                LowColor = OxyColors.White,
                Position = AxisPosition.Right,
                Maximum = max,
                Palette = OxyPalette.Interpolate(100, OxyColors.White, OxyColors.Yellow, OxyColors.Red)
            };

            // Left: category axis
            var categoryAxisLeft = new CategoryAxis {
                Position = AxisPosition.Left,
                GapWidth = 0D
            };
            foreach (var name in names) {
                categoryAxisLeft.Labels.Add(name);
            }

            // Bottom: category axis 
            var categoryAxisBottom = new CategoryAxis {
                GapWidth = 0,
                Minimum = -0.5,
                Maximum = 0.5,
                Position = AxisPosition.Bottom
            };
            categoryAxisBottom.Labels.Add("1");

            // The bar series
            var multiplier = 100 / max;
            var barSeries = new BarSeries { IsStacked = true };
            for (int i = 0; i < matrix.GetLength(1); i++) {
                var ix = (int)Math.Floor(multiplier * matrix[0, i]) - 1;
                var barItem1 = new BarItem { Value = 0.5, Color = linearColorAxisRight.Palette.Colors[ix], CategoryIndex = i };
                var barItem2 = new BarItem { Value = -0.5, Color = linearColorAxisRight.Palette.Colors[ix], CategoryIndex = i };
                barSeries.Items.Add(barItem1);
                barSeries.Items.Add(barItem2);
            }

            plotModel.Axes.Add(categoryAxisLeft);
            plotModel.Axes.Add(linearColorAxisRight);
            plotModel.Axes.Add(categoryAxisBottom);
            plotModel.Series.Add(barSeries);

            return plotModel;
        }
    }
}
