using MCRA.Utils.Charting.OxyPlot;
using MCRA.Utils.ExtensionMethods;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;


namespace MCRA.Simulation.OutputGeneration {
    public sealed class RelativePotencyFactorsChartCreator : ReportChartCreatorBase {

        private RelativePotencyFactorsSummarySection _section;

        public override string ChartId {
            get {
                var pictureId = "8a08c351-f7f4-401d-bcf9-4642c9050d38";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId);
            }
        }

        public RelativePotencyFactorsChartCreator(RelativePotencyFactorsSummarySection section) {
            _section = section;
            Height = 150 + section.Records.Count(r => !double.IsNaN(r.RelativePotencyFactor)) * 25;
        }

        public override PlotModel Create() {
            if (_section.Records.Any(r => r.RelativePotencyFactorUncertaintyValues?.Count > 0)) {
                return createUncertain(_section.Records);
            } else {
                return createNominal(_section.Records);
            }
        }

        public override string Title => "Relative potency factors.";
        private static PlotModel createNominal(List<RelativePotencyFactorsSummaryRecord> records) {
            records = records
                .Where(r => !double.IsNaN(r.RelativePotencyFactor))
                .OrderBy(r => r.RelativePotencyFactor)
                .ToList();

            var plotModel = new PlotModel() {
                PlotMargins = new OxyThickness(200, double.NaN, double.NaN, double.NaN),
            };

            var minimum = records.Select(r => r.RelativePotencyFactor).DefaultIfEmpty(1).Min();
            var maximum = records.Select(r => r.RelativePotencyFactor).DefaultIfEmpty(1).Max();

            var scatterSeries = new ScatterSeries() {
                MarkerSize = 5,
                MarkerType = MarkerType.Circle,
            };
            plotModel.Series.Add(scatterSeries);

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
            var points = new List<ScatterPoint>();
            foreach (var item in records) {
                categoryAxis.Labels.Add(item.CompoundName);
                var bpItem = new ScatterPoint(item.RelativePotencyFactor, counter);
                points.Add(bpItem);
                counter++;
            }
            scatterSeries.ItemsSource = points;

            return plotModel;
        }

        private static PlotModel createUncertain(List<RelativePotencyFactorsSummaryRecord> records) {
            records = records.OrderBy(r => r.RelativePotencyFactor).ToList();

            var plotModel = new PlotModel() {
                PlotMargins = new OxyThickness(200, double.NaN, double.NaN, double.NaN),
            };
            var nominals = records.Where(r => !double.IsNaN(r.RelativePotencyFactor))
                .Select(r => r.RelativePotencyFactor).ToList();
            var lowers = records.Where(r => !double.IsNaN(r.RelativePotencyFactorLowerBoundPercentile) && r.RelativePotencyFactorLowerBoundPercentile > 0)
                .Select(r => r.RelativePotencyFactorLowerBoundPercentile).ToList();
            var uppers = records.Where(r => !double.IsNaN(r.RelativePotencyFactorUpperBoundPercentile) && r.RelativePotencyFactorUpperBoundPercentile > 0)
                .Select(r => r.RelativePotencyFactorUpperBoundPercentile).ToList();
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
                categoryAxis.Labels.Add(item.CompoundName);
                confidenceIntervalSeries.Items.Add(new TornadoBarItem() {
                    BaseValue = item.RelativePotencyFactor,
                    Minimum = item.RelativePotencyFactorLowerBoundPercentile,
                    Maximum = item.RelativePotencyFactorUpperBoundPercentile,
                    CategoryIndex = counter,
                });
                counter++;
            }

            return plotModel;
        }
    }
}
