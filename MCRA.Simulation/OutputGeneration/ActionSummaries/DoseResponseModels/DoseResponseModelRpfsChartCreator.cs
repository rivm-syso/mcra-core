using MCRA.Utils.Charting.OxyPlot;
using MCRA.Utils.ExtensionMethods;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;


namespace MCRA.Simulation.OutputGeneration {
    public sealed class DoseResponseModelRpfsChartCreator : OxyPlotChartCreator {

        private DoseResponseModelSection _section;
        private bool _orderByRpf;

        public DoseResponseModelRpfsChartCreator(DoseResponseModelSection section, bool orderByRpf) {
            _section = section;
            _orderByRpf = orderByRpf;
            Height = 150 + section.DoseResponseFits.Count(r => !double.IsNaN(r.RelativePotencyFactor)) * 25;
        }

        public override string ChartId {
            get {
                var pictureId = "BEDE6BB7-28A1-47DB-B7E5-CBA361FE2A36";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId);
            }
        }

        public override PlotModel Create() {
            return createUncertain(_section.DoseResponseFits, _orderByRpf);
        }

        public override string Title {
            get {
                return $"Relative potency factors from dose-response model fit {_section.ExperimentCode}";
            }
        }

        private static PlotModel createUncertain(List<DoseResponseFitRecord> records, bool orderByRpf) {
            var plotRecords = orderByRpf
                ? records.OrderBy(r => r.RelativePotencyFactor).ToList()
                : records.OrderBy(r => r.SubstanceName, StringComparer.OrdinalIgnoreCase)
                    .ThenBy(r => r.SubstanceCode, StringComparer.OrdinalIgnoreCase)
                    .ToList();

            var plotModel = new PlotModel() {
                PlotMargins = new OxyThickness(200, double.NaN, double.NaN, double.NaN),
            };

            var nominals = records.Where(r => !double.IsNaN(r.RelativePotencyFactor)).Select(r => r.RelativePotencyFactor).ToList();
            var lowers = records.Where(r => r.RpfLower.HasValue && !double.IsNaN(r.RpfLower.Value) && r.RpfLower.Value > 0).Select(r => r.RpfLower.Value).ToList();
            var uppers = records.Where(r => r.RpfUpper.HasValue && !double.IsNaN(r.RpfUpper.Value) && r.RpfUpper.Value > 0).Select(r => r.RpfUpper.Value).ToList();
            var minimum = lowers.Any() ? lowers.Min() : nominals.DefaultIfEmpty(1).Min();
            var maximum = uppers.Any() ? uppers.Max() : nominals.DefaultIfEmpty(1).Max();

            var confidenceIntervalSeries = new ConfidenceIntervalBarSeries() {
                MarkerSize = 5,
                MarkerType = MarkerType.Circle,
            };
            plotModel.Series.Add(confidenceIntervalSeries);

            var categoryAxis = new CategoryAxis() {
                MinorStep = 1,
                GapWidth = 0.1,
                IsTickCentered = true,
                TextColor = OxyColors.Black,
                Position = AxisPosition.Left,
                Minimum = -.5,
                Maximum = records.Count - .5,
            };
            plotModel.Axes.Add(categoryAxis);

            var horizontalAxis = new LogarithmicAxis() {
                Title = $"Relative Potency Factors",
                Minimum = minimum * .1,
                Maximum = maximum * 10,
                MajorGridlineStyle = LineStyle.Dash,
                MinorGridlineStyle = LineStyle.None,
                MinorTickSize = 0,
                Position = AxisPosition.Top,
                Base = 10,
                UseSuperExponentialFormat = false,
            };
            plotModel.Axes.Add(horizontalAxis);

            var counter = 0;
            foreach (var item in records) {
                categoryAxis.Labels.Add(item.SubstanceName + item.CovariateLevel);
                confidenceIntervalSeries.Items.Add(new TornadoBarItem() {
                    BaseValue = item.RelativePotencyFactor,
                    Minimum = item.RpfLower ?? double.NaN,
                    Maximum = item.RpfUpper ?? double.NaN,
                    CategoryIndex = counter,
                });
                counter++;
            }

            return plotModel;
        }
    }
}
