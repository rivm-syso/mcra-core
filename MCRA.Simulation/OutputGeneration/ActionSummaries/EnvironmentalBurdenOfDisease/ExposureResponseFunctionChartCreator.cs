using MathNet.Numerics.Statistics;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;
using OxyPlot;
using OxyPlot.Annotations;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class ExposureResponseFunctionChartCreator : ReportLineChartCreatorBase {

        private readonly ErfSummaryRecord _record;
        private readonly string _sectionId;
        private readonly double _uncertaintyLowerLimit;
        private readonly double _uncertaintyUpperLimit;

        public ExposureResponseFunctionChartCreator(
            ErfSummaryRecord erfSummaryRecord,
            string sectionId,
            double uncertaintyLowerLimit,
            double uncertaintyUpperLimit
        ) {
            Width = 500;
            Height = 350;
            _record = erfSummaryRecord;
            _sectionId = sectionId;
            _uncertaintyLowerLimit = uncertaintyLowerLimit;
            _uncertaintyUpperLimit = uncertaintyUpperLimit;
        }

        public override string Title => "Exposure response function.";

        public override string ChartId {
            get {
                var pictureId = "518a01e8-b8f2-434d-89c2-286bdf86c89b";
                return StringExtensions.CreateFingerprint(_sectionId + pictureId);
            }
        }

        public override PlotModel Create() {
            return create(
                _record,
                _record.ExposureResponseGridDataPoints[0].UncertainValues.Count != 0,
                _uncertaintyLowerLimit,
                _uncertaintyUpperLimit
            );
        }

        private static PlotModel create(
            ErfSummaryRecord record,
            bool uncertainty,
            double uncertaintyLowerLimit,
            double uncertaintyUpperLimit
        ) {
            var x = record.ExposureResponseDataPoints.Select(c => c.Exposure).ToList();
            var y = record.ExposureResponseDataPoints.Select(c => c.ResponseValue).ToList();

            var gridDataPoints = record.ExposureResponseGridDataPoints;
            var xLine = gridDataPoints.Select(c => c.XValue).ToList();
            var yLine = gridDataPoints.Select(c => c.ReferenceValue).ToList();

            var plotModel = createDefaultPlotModel();

            var series1 = new LineSeries() {
                Color = OxyColors.Black,
                MarkerType = MarkerType.None
            };
            for (int i = 0; i < xLine.Count; i++) {
                series1.Points.Add(new DataPoint(xLine[i], yLine[i]));
            }
            var series2 = new ScatterSeries() {
                MarkerType = MarkerType.Circle,
                MarkerFill = OxyColors.Red,
                MarkerSize = 4
            };
            for (int i = 0; i < x.Count; i++) {
                series2.Points.Add(new ScatterPoint(x[i], y[i]));
            }
            plotModel.Series.Add(series1);
            plotModel.Series.Add(series2);

            var counterfactualValue = new LineAnnotation {
                LineStyle = LineStyle.Dash,
                Type = LineAnnotationType.Vertical,
                X = record.CounterfactualValue,
                Color = OxyColors.Red
            };
            plotModel.Annotations.Add(counterfactualValue);

            var horizontalAxis = createLinearAxis("Exposure level (" + record.TargetUnit + ")", 0D);
            horizontalAxis.Position = AxisPosition.Bottom;
            plotModel.Axes.Add(horizontalAxis);

            var verticalAxis = createLinearAxis(record.EffectMetric);
            plotModel.Axes.Add(verticalAxis);

            // Area series for visualisation of uncertainty of the line series
            if (uncertainty) {
                var areaSeries = new AreaSeries() {
                    Color = OxyColors.Red,
                    Color2 = OxyColors.Red,
                    Fill = OxyColor.FromAColor(50, OxyColors.Red),
                    StrokeThickness = .5,
                };
                for (var i = 0; i < gridDataPoints.Count; i++) {
                    areaSeries.Points.Add(new DataPoint(xLine[i], gridDataPoints[i].UncertainValues.Percentile(uncertaintyLowerLimit)));
                    areaSeries.Points2.Add(new DataPoint(xLine[i], gridDataPoints[i].UncertainValues.Percentile(uncertaintyUpperLimit)));
                }
                plotModel.Series.Add(areaSeries);
            }

            return plotModel;
        }
    }
}
