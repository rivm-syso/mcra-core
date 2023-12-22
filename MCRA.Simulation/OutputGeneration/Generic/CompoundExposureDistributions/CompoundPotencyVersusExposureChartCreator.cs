using MCRA.Utils.Charting.OxyPlot;
using MCRA.Utils.ExtensionMethods;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Legends;
using OxyPlot.Series;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class CompoundPotencyVersusExposureChartCreator : ReportLineChartCreatorBase {

        private string _intakeUnit;
        private CompoundExposureDistributionsSection _section;

        public CompoundPotencyVersusExposureChartCreator(CompoundExposureDistributionsSection section, string intakeUnit) {
            Width = 750;
            Height = 450;
            _section = section;
            _intakeUnit = intakeUnit;
        }
        public override string Title => "RPFs vs mean exposures";
        public override string ChartId {
            get {
                var pictureId = "9c9d2339-a864-4e43-8e5a-227f68250d62";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId);
            }
        }

        public override PlotModel Create() {
            return create(_section.CompoundExposureDistributionRecords, _intakeUnit);
        }

        private PlotModel create(List<CompoundExposureDistributionRecord> records, string intakeUnit) {
            var plotModel = createDefaultPlotModel(string.Empty);

            var positiveExposureRecords = records.Where(r => !double.IsNaN(r.Mu)).ToList();
            var exposureMeans = positiveExposureRecords.Select(r => Math.Exp(r.Mu)).ToList();

            var series = new ScatterSeries() {
                MarkerType = MarkerType.Circle
            };
            var originPoints = positiveExposureRecords.Select(r => new ScatterPoint(Math.Exp(r.Mu), r.RelativePotencyFactor, 5 * r.AssessmentGroupMembership));
            series.Points.AddRange(originPoints);
            plotModel.Series.Add(series);

            var horizontalAxis = createLogarithmicAxis($"Mean exposure ({_intakeUnit})");
            horizontalAxis.Position = AxisPosition.Bottom;
            horizontalAxis.MaximumPadding = .1;
            horizontalAxis.MinimumPadding = .1;
            plotModel.Axes.Add(horizontalAxis);

            var verticalAxis = createLogarithmicAxis("RPF");
            verticalAxis.Position = AxisPosition.Left;
            verticalAxis.MaximumPadding = .1;
            verticalAxis.MinimumPadding = .1;
            plotModel.Axes.Add(verticalAxis);

            plotModel.IsLegendVisible = true;
            var Legend = new Legend {
                LegendPlacement = LegendPlacement.Outside,
            };
            plotModel.Legends.Add(Legend);


            return plotModel;
        }
    }
}
