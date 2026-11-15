using MCRA.Utils.ExtensionMethods;
using OxyPlot;
using OxyPlot.Annotations;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace MCRA.Simulation.OutputGeneration {
    public class PbkSimulationTimeSeriesChartCreator : ReportLineChartCreatorBase {

        private readonly PbkSimulationDrilldownSection _section;
        private readonly PbkSimulationDrilldownRecord _record;

        public PbkSimulationTimeSeriesChartCreator(
            PbkSimulationDrilldownSection section,
            PbkSimulationDrilldownRecord record
        ) {
            Width = 350;
            Height = 250;
            _section = section;
            _record = record;
        }

        public override string Title => $"PBK simulation output {_record.EndpointName}";

        public override string ChartId {
            get {
                var pictureId = "CBA6F071-B26F-4518-9F78-F72627270860";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId + _record.SimulationId + _record.EndpointCode);
            }
        }

        public override PlotModel Create() {
            var plotModel = createPlotModel(
                _record,
                _section.IsLifetime,
                _section.NumberOfDaysSkipped
            );
            return plotModel;
        }

        private static PlotModel createPlotModel(
            PbkSimulationDrilldownRecord record,
            bool isLifetime,
            int numberOfDaysSkipped
        ) {
            var plotModel = createDefaultPlotModel();

            var xValues = isLifetime
                ? Enumerable
                    .Range(0, record.OutputTimeSeries.Count)
                    .Select(r => r / 365D).ToList()
                : Enumerable
                    .Range(0, record.OutputTimeSeries.Count)
                    .Select(r => (double)r).ToList();
            var yValues = record.OutputTimeSeries;

            // Time axis
            var xAxis = new LinearAxis() {
                Position = AxisPosition.Bottom,
                Title = isLifetime ? "Time (days)" : $"Time (years)",
                MajorGridlineStyle = LineStyle.Dash,
            };
            plotModel.Axes.Add(xAxis);

            // Y-axis
            var ytitle = $"{record.EndpointName} ({record.Unit})";
            var verticalAxis = createLinearAxis(ytitle);
            verticalAxis.Maximum = yValues.DefaultIfEmpty(1).Max() * 1.05;
            verticalAxis.Minimum = 0;
            plotModel.Axes.Add(verticalAxis);

            // Time series
            var series = new LineSeries() {
                Color = OxyColors.Red,
                MarkerType = MarkerType.None,
                StrokeThickness = 1.5,
            };
            for (var i = 0; i < xValues.Count; i++) {
                series.Points.Add(new DataPoint(xValues[i], yValues[i]));
            }
            plotModel.Series.Add(series);

            if (!isLifetime) {
                // Vertical burn-in period reference line
                var burnInPeriodLineAnnotation = new LineAnnotation() {
                    Type = LineAnnotationType.Vertical,
                    X = numberOfDaysSkipped,
                    LineStyle = LineStyle.Solid,
                    Color = OxyColors.Black,
                    StrokeThickness = 1
                };
                plotModel.Annotations.Add(burnInPeriodLineAnnotation);
            }
            return plotModel;
        }
    }
}
