using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Calculators.EnvironmentalBurdenOfDiseaseCalculation;
using MCRA.Utils.ExtensionMethods;
using OxyPlot;
using OxyPlot.Annotations;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class ExposureResponseFunctionChartCreator : ReportLineChartCreatorBase {

        private readonly List<AttributableBodSummaryRecord> _records;
        private readonly ExposureResponseFunction _exposureResponseFunction;
        private readonly string _sectionId;

        public ExposureResponseFunctionChartCreator(
            List<AttributableBodSummaryRecord> records,
            ExposureResponseFunction exposureResponseFunction,
            string sectionId) {
            Width = 500;
            Height = 350;
            _records = records;
            _exposureResponseFunction = exposureResponseFunction;
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
            var x = _records.Select(c => c.Exposure).ToList();
            var y = _records.Select(c => c.Ratio).ToList();
            return create(x, y, _exposureResponseFunction);
        }

        private PlotModel create(
            List<double> x,
            List<double> y,
            ExposureResponseFunction exposureResponseFunction
        ) {
            var unit = exposureResponseFunction.DoseUnit.GetShortDisplayName();

            var plotModel = createDefaultPlotModel();

            var series1 = new FunctionSeries(
                exposureResponseFunction.Compute,
                exposureResponseFunction.Baseline,
                1.1 * x.Max(),
                0.0001
            ) {
                Color = OxyColors.Black,
                MarkerType = MarkerType.None
            };
            var series2 = new ScatterSeries() {
                MarkerType = MarkerType.Circle,
                MarkerFill = OxyColors.Red,
                MarkerSize = 2
            };
            for (int i = 0; i < x.Count; i++) {
                series2.Points.Add(new ScatterPoint(x[i], y[i]));
            }
            plotModel.Series.Add(series1);
            plotModel.Series.Add(series2);

            var baseline = new LineAnnotation {
                LineStyle = LineStyle.Dash,
                Type = LineAnnotationType.Vertical,
                X = exposureResponseFunction.Baseline,
                Color = OxyColors.Red
            };
            plotModel.Annotations.Add(baseline);

            var horizontalAxis = createLinearAxis("Exposure level (" + unit + ")", 0D);
            horizontalAxis.Position = AxisPosition.Bottom;
            plotModel.Axes.Add(horizontalAxis);

            var verticalAxis = createLinearAxis("Odds ratios");
            plotModel.Axes.Add(verticalAxis);

            return plotModel;
        }
    }
}
