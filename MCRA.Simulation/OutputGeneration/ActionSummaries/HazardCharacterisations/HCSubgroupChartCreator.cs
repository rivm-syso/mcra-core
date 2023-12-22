using MCRA.General;
using MCRA.Utils.Charting.OxyPlot;
using MCRA.Utils.ExtensionMethods;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class HCSubgroupChartCreator : ReportLineChartCreatorBase {

        private readonly HCSubgroupSubstancePlotRecords _substanceRecords;
        private readonly HazardCharacterisationsFromDataSummarySection _section;
        private readonly bool _isUncertainty;
        private readonly string _uncertaintyMessage;

        public override string Title => $"Hazard characterisation vs age: {_substanceRecords.SubstanceName}. Green line: the default HC. Blue line: age dependent HCs. {_uncertaintyMessage}";

        public override string ChartId {
            get {
                var pictureId = "c71051ad-4899-4825-96ad-ad6fc0a01121";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId);
            }
        }

        public HCSubgroupChartCreator(HazardCharacterisationsFromDataSummarySection section, HCSubgroupSubstancePlotRecords substanceRecords) {
            Width = 500;
            Height = 350; ;
            _substanceRecords = substanceRecords;
            _section = section;
            _isUncertainty = substanceRecords.PlotRecords.SelectMany(c => c.UncertaintyValues).Any();
            _uncertaintyMessage = _isUncertainty ? "Red dots: uncertainty." : string.Empty;
        }

        public override PlotModel Create() {
            return create(_substanceRecords);
        }

        private PlotModel create(HCSubgroupSubstancePlotRecords records) {
            var seriesHC = new LineSeries() {
                Color = OxyColors.Green,
                MarkerType = MarkerType.None,
                MarkerStrokeThickness = 3,
            };
            var seriesHCUncertain = new ScatterSeries() {
                MarkerType = MarkerType.Circle,
                MarkerFill = OxyColor.FromAColor(100, OxyColors.Red),
                MarkerSize = 3,
                MarkerStroke = OxyColors.Red
            };
            var seriesHCDependent = new LineSeries() {
                Color = OxyColors.CornflowerBlue,
                MarkerType = MarkerType.Circle,
                MarkerStrokeThickness = 1,
                MarkerSize = 3,
                MarkerFill = OxyColor.FromAColor(100, OxyColors.CornflowerBlue),
                MarkerStroke = OxyColors.CornflowerBlue
            };

            var minAge = records.PlotRecords.Min(c => c.Age);
            var maxAge = records.PlotRecords.Max(c => c.Age);
            seriesHC.Points.Add(new DataPoint(minAge, records.Value));
            seriesHC.Points.Add(new DataPoint(maxAge, records.Value));
            foreach (var record in records.PlotRecords) {
                seriesHCDependent.Points.Add(new DataPoint(record.Age, record.HazardCharacterisationValue));
                foreach (var value in record.UncertaintyValues) {
                    seriesHCUncertain.Points.Add(new ScatterPoint(record.Age, value));
                }
            }
            var plotModel = createDefaultPlotModel(string.Empty);

            plotModel.Series.Add(seriesHC);
            plotModel.Series.Add(seriesHCUncertain);
            plotModel.Series.Add(seriesHCDependent);

            var horizontalAxis = createLinearAxis("Age (years)");
            horizontalAxis.Position = AxisPosition.Bottom;
            plotModel.Axes.Add(horizontalAxis);

            var verticalAxis = createLinearAxis($"Hazard characterisation ({_substanceRecords.Unit})");
            plotModel.Axes.Add(verticalAxis);

            return plotModel;
        }
    }
}
