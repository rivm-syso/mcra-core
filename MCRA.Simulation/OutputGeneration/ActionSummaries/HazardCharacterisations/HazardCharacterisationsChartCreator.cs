using MCRA.General;
using MCRA.Utils.Charting.OxyPlot;
using MCRA.Utils.ExtensionMethods;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;


namespace MCRA.Simulation.OutputGeneration {
    public sealed class HazardCharacterisationsChartCreator : OxyPlotChartCreator {

        private string _sectionId;
        private ExposureTarget _exposureTarget;
        private List<HazardCharacterisationsSummaryRecord> _records;
        private string _targetDoseUnit;

        public HazardCharacterisationsChartCreator(
            string sectionId, 
            ExposureTarget exposureTarget, 
            List<HazardCharacterisationsSummaryRecord> records, 
            string targetDoseUnit
        ) {
            _sectionId = sectionId;
            _exposureTarget = exposureTarget;
            _records = records;
            _targetDoseUnit = targetDoseUnit;
            var recordsWithValues = _records.Where(r => !double.IsNaN(r.HazardCharacterisation)).ToList();
            Height = 150 + recordsWithValues.Count * 25;
        }

        public override string ChartId {
            get {
                var pictureId = "1AD79198-A67D-46E7-9FBF-7C94B2AEA495";
                return StringExtensions.CreateFingerprint(_sectionId + pictureId + _exposureTarget.Code);
            }
        }

        public override string Title => $"Hazard characterisations ({_targetDoseUnit}).";

        public override PlotModel Create() {
            if (_records.Any(r => r.TargetDoseUncertaintyValues?.Any() ?? false)) {
                return createUncertain(_records, _targetDoseUnit);
            } else {
                return createNominal(_records, _targetDoseUnit);
            }
        }

        private static PlotModel createNominal(List<HazardCharacterisationsSummaryRecord> records, string targetDoseUnit) {
            records = records
                .Where(r => !double.IsNaN(r.HazardCharacterisation))
                .OrderByDescending(r => r.HazardCharacterisation)
                .ToList();

            var plotModel = new PlotModel() {
                PlotMargins = new OxyThickness(200, double.NaN, double.NaN, double.NaN),
            };

            if (records == null || records.Count == 0) {
                return plotModel;
            }

            var minimum = records.Min(r => r.HazardCharacterisation);
            var maximum = records.Max(r => r.HazardCharacterisation);

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
                Minimum = minimum * .9,
                Maximum = maximum * 1.1,
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
                var bpItem = new ScatterPoint(item.HazardCharacterisation, counter);
                points.Add(bpItem);
                counter++;
            }
            scatterSeries.ItemsSource = points;

            return plotModel;
        }

        private static PlotModel createUncertain(List<HazardCharacterisationsSummaryRecord> records, string targetDoseUnit) {
            records = records
                .Where(r => !double.IsNaN(r.HazardCharacterisation))
                .OrderByDescending(r => r.HazardCharacterisation)
                .ToList();

            var plotModel = new PlotModel() {
                //Title = $"Hazard characterisation ({targetDoseUnit})",
                PlotMargins = new OxyThickness(200, double.NaN, double.NaN, double.NaN),
                //IsLegendVisible = true,
            };

            var minimum = records.Min(r => r.TargetDoseLowerBoundPercentile);
            var maximum = records.Max(r => r.TargetDoseUpperBoundPercentile);

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
                    BaseValue = item.HazardCharacterisation,
                    Minimum = item.TargetDoseLowerBoundPercentile,
                    Maximum = item.TargetDoseUpperBoundPercentile,
                    CategoryIndex = counter,
                });
                counter++;
            }

            return plotModel;
        }
    }
}
