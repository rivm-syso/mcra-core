using MCRA.Utils.Charting.OxyPlot;
using MCRA.Utils.ExtensionMethods;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class AvailableHazardCharacterisationsChartCreator : OxyPlotChartCreator {

        private readonly AvailableHazardCharacterisationsSummarySection _section;
        private readonly List<string> _substanceNames;
        private readonly string _targetDoseUnit;

        public AvailableHazardCharacterisationsChartCreator(AvailableHazardCharacterisationsSummarySection section, string targetDoseUnit) {
            _section = section;
            _substanceNames = _section.Records
                .OrderByDescending(c => c.HazardCharacterisation)
                .Select(r => r.CompoundName)
                .Distinct()
                .ToList();
            _targetDoseUnit = targetDoseUnit;
            Width = 600;
            Height = 150 + _substanceNames.Count * 25;
        }

        public override string ChartId {
            get {
                var pictureId = "01458766-B490-4AE2-8066-D732BE17A443";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId);
            }
        }

        public override PlotModel Create() {
            return createNominal(_section.Records, _substanceNames, _targetDoseUnit);
        }

        public override string Title => $"Hazard characterisations ({_targetDoseUnit})";

        private static PlotModel createNominal(List<AvailableHazardCharacterisationsSummaryRecord> records, List<string> substances, string targetDoseUnit) {
            var plotModel = new PlotModel() {
                PlotMargins = new OxyThickness(100, double.NaN, double.NaN, double.NaN),
                IsLegendVisible = records.Distinct(r => r.EffectCode ?? "-").Count() > 1
            };

            var Legend = new OxyPlot.Legends.Legend {
                LegendPlacement = OxyPlot.Legends.LegendPlacement.Outside
            };
            plotModel.Legends.Add(Legend);

            var minimum = records.Min(r => r.HazardCharacterisation);
            var maximum = records.Max(r => r.HazardCharacterisation);

            var categoryAxis = new CategoryAxis() {
                MinorStep = 1,
                GapWidth = 0.1,
                IsTickCentered = true,
                TextColor = OxyColors.Black,
                Position = AxisPosition.Left,
                Minimum = -0.5,
                Maximum = substances.Count - .5,
            };
            foreach (var item in substances) {
                categoryAxis.Labels.Add(item);
            }
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

            var grouping = records.GroupBy(r => r.EffectName ?? "-");
            foreach (var group in grouping) {
                var scatterSeries = new ScatterSeries() {
                    Title = group.Key,
                    MarkerSize = 5,
                    MarkerType = MarkerType.Circle,
                };
                plotModel.Series.Add(scatterSeries);
                var dataPoints = group.ToLookup(r => r.CompoundName);
                var counter = 0;
                var points = new List<ScatterPoint>();
                foreach (var substance in substances) {
                    var items = dataPoints[substance];
                    foreach (var item in items) {
                        var bpItem = new ScatterPoint(item.HazardCharacterisation, counter);
                        points.Add(bpItem);
                    }
                    counter++;
                }
                scatterSeries.ItemsSource = points;
            }

            return plotModel;
        }
    }
}
