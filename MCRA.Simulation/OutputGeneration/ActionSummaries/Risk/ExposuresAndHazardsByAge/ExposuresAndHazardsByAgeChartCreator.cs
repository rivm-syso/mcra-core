using MCRA.General;
using MCRA.Utils.ExtensionMethods;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class ExposuresAndHazardsByAgeChartCreator : ReportLineChartCreatorBase {

        private readonly ExposuresAndHazardsByAgeSection _section;

        private string _title;

        public override string Title => _title;

        public override string ChartId {
            get {
                var pictureId = "BB9C9AED-918F-47AF-83B9-4B72AE3A3BF6";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId);
            }
        }

        public ExposuresAndHazardsByAgeChartCreator(ExposuresAndHazardsByAgeSection section) {
            Width = 500;
            Height = 350; ;
            _section = section;
            var recordsWithAgeCount = section.HazardExposureByAgeRecords
                .Where(r => r.Exposure > 0)
                .Count();
            var positivesCount = section.HazardExposureByAgeRecords
                .Where(r => r.Age.HasValue)
                .Where(r => r.Exposure > 0)
                .Count();
            var percentagePositiveExposure = section.HazardCharacterisationRecords.Any()
                ? (double)positivesCount / recordsWithAgeCount * 100d
                : 0;
            _title = $"Exposures and hazard characterisations by age ({percentagePositiveExposure}% positives)."
                + " Green line: the default HC. Blue line: age dependent HCs.";
        }

        public override PlotModel Create() {
            return create(_section);
        }

        private PlotModel create(ExposuresAndHazardsByAgeSection section) {
            var hcRecords = section.HazardCharacterisationRecords;
            var riskRecords = section.HazardExposureByAgeRecords
                .Where(r => r.Age.HasValue)
                .Where(r => r.Exposure > 0)
                .ToList();

            var plotModel = createDefaultPlotModel();

            var minAge = 0.9 * riskRecords.Min(c => c.Age.Value);
            var maxAge = 1.1 * riskRecords.Max(c => c.Age.Value);

            // Exposures versus age scatter series
            var exposuresSeries = new ScatterSeries() {
                MarkerType = MarkerType.Circle,
                MarkerFill = OxyColor.FromAColor(100, OxyColors.Black),
                MarkerSize = 3,
                MarkerStroke = OxyColors.Black
            };
            foreach (var item in riskRecords) {
                exposuresSeries.Points.Add(new ScatterPoint(item.Age.Value, item.Exposure));
            }
            plotModel.Series.Add(exposuresSeries);

            if (hcRecords?.Count > 0) {
                // The default HC (green line)
                var hcSeries = new LineSeries() {
                    Color = OxyColors.Green,
                    MarkerType = MarkerType.None,
                    MarkerStrokeThickness = 3,
                };
                hcSeries.Points.Add(new DataPoint(minAge, section.NominalHazardCharacterisationValue));
                hcSeries.Points.Add(new DataPoint(maxAge, section.NominalHazardCharacterisationValue));
                plotModel.Series.Add(hcSeries);

                // Age dependent HCs (blue line)
                var hcDependentSeries = new LineSeries() {
                    Color = OxyColors.CornflowerBlue,
                    MarkerType = MarkerType.Circle,
                    MarkerStrokeThickness = 1,
                    MarkerSize = 3,
                    MarkerFill = OxyColor.FromAColor(100, OxyColors.CornflowerBlue),
                    MarkerStroke = OxyColors.CornflowerBlue
                };
                foreach (var record in hcRecords) {
                    hcDependentSeries.Points.Add(new DataPoint(record.Age, record.HazardCharacterisationValue));
                }
                hcDependentSeries.Points.Add(new DataPoint(maxAge, hcRecords.MaxBy(r => r.Age).HazardCharacterisationValue));
                plotModel.Series.Add(hcDependentSeries);
            }

            var horizontalAxis = createLinearAxis(
                title: "Age (years)",
                position: AxisPosition.Bottom
            );
            horizontalAxis.AbsoluteMinimum = minAge;
            horizontalAxis.AbsoluteMaximum = maxAge;
            plotModel.Axes.Add(horizontalAxis);

            var verticalAxis = createLogarithmicAxis(
                title: section.IsCumulative
                    ? $"Cumulative exposure ({section.TargetUnit.GetShortDisplayName()})"
                    : $"Exposure ({section.TargetUnit.GetShortDisplayName()})",
                position: AxisPosition.Left
            );

            plotModel.Axes.Add(verticalAxis);

            return plotModel;
        }
    }
}
