﻿using MCRA.Utils.ExtensionMethods;
using OxyPlot;
using OxyPlot.Annotations;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class ExposureResponseFunctionChartCreator : ReportLineChartCreatorBase {

        private readonly ErfSummaryRecord _record;
        private readonly string _sectionId;

        public ExposureResponseFunctionChartCreator(
            ErfSummaryRecord erfSummaryRecord,
            string sectionId
        ) {
            Width = 500;
            Height = 350;
            _record = erfSummaryRecord;
            _sectionId = sectionId;
        }

        public override string Title => "Exposure response function.";

        public override string ChartId {
            get {
                var pictureId = "518a01e8-b8f2-434d-89c2-286bdf86c89b";
                return StringExtensions.CreateFingerprint(_sectionId + pictureId);
            }
        }

        public override PlotModel Create() {
            return create(_record);
        }

        private static PlotModel create(
            ErfSummaryRecord record
        ) {
            var x = record.ExposureResponseDataPoints.Select(c => c.Exposure).ToList();
            var y = record.ExposureResponseDataPoints.Select(c => c.ResponseValue).ToList();

            var xLine = record.ExposureResponseChartDataPoints.Select(c => c.Exposure).ToList();
            var yLine = record.ExposureResponseChartDataPoints.Select(c => c.ResponseValue).ToList();

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

            var baseline = new LineAnnotation {
                LineStyle = LineStyle.Dash,
                Type = LineAnnotationType.Vertical,
                X = record.Baseline,
                Color = OxyColors.Red
            };
            plotModel.Annotations.Add(baseline);

            var horizontalAxis = createLinearAxis("Exposure level (" + record.TargetUnit + ")", 0D);
            horizontalAxis.Position = AxisPosition.Bottom;
            plotModel.Axes.Add(horizontalAxis);

            var verticalAxis = createLinearAxis(record.EffectMetric);
            plotModel.Axes.Add(verticalAxis);

            return plotModel;
        }
    }
}
