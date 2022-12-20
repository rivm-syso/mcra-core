using MCRA.Utils.Charting.OxyPlot;
using MCRA.Utils.ExtensionMethods;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class IviveHazardCharacterisationsChartCreator : OxyPlotChartCreator {

        private IviveHazardCharacterisationsSummarySection _section;
        private List<string> _substanceNames;
        private string _targetDoseUnit;

        public IviveHazardCharacterisationsChartCreator(IviveHazardCharacterisationsSummarySection section, string targetDoseUnit) {
            _section = section;
           _substanceNames = _section.Records.OrderByDescending(c => c.HazardCharacterisation).Select(r => r.CompoundName).Distinct().ToList();
            _targetDoseUnit = targetDoseUnit;
            Width = 800;
            Height = 150 + _substanceNames.Count * 25;
        }

        public override string Title => $"Histogram of IVIVE hazard characterisations ({_targetDoseUnit})";

        public override string ChartId {
            get {
                var pictureId = "429b886c-0736-495e-a7db-f368d6b4db96";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId);
            }
        }

        public override PlotModel Create() {
            return createNominal(_section.Records, _substanceNames, _targetDoseUnit);
        }

        private static PlotModel createNominal(List<IviveHazardCharacterisationsSummaryRecord> records, List<string> substances, string targetDoseUnit) {

            var plotModel = new PlotModel() {
                LegendPlacement = LegendPlacement.Outside,
                PlotMargins = new OxyThickness(200, double.NaN, double.NaN, double.NaN),
            };
            
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

            var grouping = records.GroupBy(r => r.EffectName);
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
