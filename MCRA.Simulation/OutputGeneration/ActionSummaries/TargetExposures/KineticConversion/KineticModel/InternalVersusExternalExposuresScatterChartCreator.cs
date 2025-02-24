using MCRA.General;
using MCRA.Utils.ExtensionMethods;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace MCRA.Simulation.OutputGeneration {

    public class InternalVersusExternalExposuresScatterChartCreator : ReportLineChartCreatorBase {

        private readonly InternalVersusExternalExposuresSection _section;
        private readonly TargetUnit _targetUnit;
        private readonly ExposureUnitTriple _externalExposureUnit;

        public InternalVersusExternalExposuresScatterChartCreator(
            InternalVersusExternalExposuresSection section,
            TargetUnit targetUnit
        ) {
            Width = 500;
            Height = 350;
            _section = section;
            _externalExposureUnit = _section.ExternalExposureUnit;
            _targetUnit = targetUnit;
        }

        public override string Title => $"Internal ({_targetUnit.Target.GetDisplayName()}) versus external exposures of {_section.SubstanceName}.";

        public override string ChartId {
            get {
                var pictureId = "c651aee4-f5f5-49aa-8b14-cd6e33fa1d04";
                return StringExtensions.CreateFingerprint(_section.SectionId + _targetUnit.Target.Code + pictureId);
            }
        }

        public override PlotModel Create() {
            var xtitle = $"External exposure ({_externalExposureUnit.GetShortDisplayName()})";
            var ytitle = $"Internal exposure ({_targetUnit.GetShortDisplayName()})";
            return createPlotModel(xtitle, ytitle);
        }

        protected PlotModel createPlotModel(string xtitle, string ytitle) {
            var plotModel = createDefaultPlotModel();

            var target = _targetUnit.Target;
            var records = _section.Records
                .Where(r => !double.IsNaN(r.TargetExposure[target]))
                .Where(r => r.TargetExposure[target] > 0)
                .ToList();

            var externalExposures = records
                .Select(r => r.ExternalExposure)
                .ToList();
            var targetExposures = records
                .Select(r => r.TargetExposure[target])
                .ToList();

            var minExternalExposures = externalExposures.Any(c => c > 0)
                ? externalExposures.Where(c => c > 0).Min() * 0.1 : 0.1;
            var maxExternalExposures = externalExposures.Any(c => c > 0)
                ? externalExposures.Where(c => c > 0).Max() * 2 : 2;
            var minInternalExposures = double.MinValue;
            var maxInternalExposures = double.MaxValue;
            minInternalExposures = targetExposures.Any(c => c > 0)
                ? targetExposures.Where(c => c > 0).Min() * 0.1 : 0.1;
            maxInternalExposures = targetExposures.Any(c => c > 0)
                ? targetExposures.Where(c => c > 0).Max() * 1.1 : 2;

            var minimum = Math.Min(minInternalExposures, minExternalExposures);
            var maximum = Math.Max(maxInternalExposures, maxExternalExposures);

            var horizontalAxis = new LogarithmicAxis() {
                Position = AxisPosition.Bottom,
                Title = xtitle,
                MajorGridlineStyle = LineStyle.Dash,
                MinorGridlineStyle = LineStyle.None,
                MinorTickSize = 0,
                Minimum = minimum,
                Maximum = maximum,
            };
            plotModel.Axes.Add(horizontalAxis);

            var verticalAxis = new LogarithmicAxis() {
                Position = AxisPosition.Left,
                Title = ytitle,
                MajorGridlineStyle = LineStyle.Dash,
                MinorGridlineStyle = LineStyle.None,
                MinorTickSize = 0,
                Minimum = minimum,
                Maximum = maximum,
            };
            plotModel.Axes.Add(verticalAxis);

            var scatterSeries = new ScatterSeries() {
                MarkerType = MarkerType.Circle,
                MarkerSize = 2,
                MarkerFill = OxyColor.FromArgb(125, 0, 128, 0),
                MarkerStroke = OxyColors.Green,
                MarkerStrokeThickness = 0.4,
            };
            foreach (var record in records) {
                scatterSeries.Points.Add(
                    new ScatterPoint(record.ExternalExposure, record.TargetExposure[target])
                );
            }
            plotModel.Series.Add(scatterSeries);

            var lineSeriesConversionFactor = new LineSeries() {
                Color = OxyColors.Black,
                LineStyle = LineStyle.Solid,
                StrokeThickness = 1,
            };

            lineSeriesConversionFactor.Points.Add(new DataPoint(minimum, minimum));
            lineSeriesConversionFactor.Points.Add(new DataPoint(maximum, maximum));
            plotModel.Series.Add(lineSeriesConversionFactor);
            return plotModel;
        }
    }
}
