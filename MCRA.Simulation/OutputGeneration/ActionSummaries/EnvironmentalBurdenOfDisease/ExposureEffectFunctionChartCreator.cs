using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Calculators.EnvironmentalBurdenOfDiseaseCalculation;
using MCRA.Utils.ExtensionMethods;
using OxyPlot;
using OxyPlot.Annotations;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class ExposureEffectFunctionChartCreator : ReportLineChartCreatorBase {
        
        private readonly List<AttributableBodSummaryRecord> _records;
        private readonly ExposureEffectFunction _exposureEffectFunction;
        private readonly string _sectionId;

        public ExposureEffectFunctionChartCreator(
            List<AttributableBodSummaryRecord> records,
            ExposureEffectFunction exposureEffectFunction,
            string sectionId) {
            Width = 500;
            Height = 350;
            _records = records;
            _exposureEffectFunction = exposureEffectFunction;
            _sectionId = sectionId;
        }

        public override string Title => "Exposure effect function.";

        public override string ChartId {
            get {
                var pictureId = "518a01e8-b8f2-434d-89c2-286bdf86c89b";
                return StringExtensions.CreateFingerprint(_sectionId + pictureId);
            }
        }

        public override PlotModel Create() {
            var x = _records.Select(c => c.Exposure).ToList();
            var y = _records.Select(c => c.Ratio).ToList();
            return create(x, y, _exposureEffectFunction);
        }

        private PlotModel create(
            List<double> x,
            List<double> y,
            ExposureEffectFunction exposureEffectFunction
        ) {
            var unit = exposureEffectFunction.DoseUnit.GetShortDisplayName();

            var plotModel = createDefaultPlotModel();

            var series1 = new FunctionSeries(
                exposureEffectFunction.Compute,
                exposureEffectFunction.Baseline,
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
                X = exposureEffectFunction.Baseline,
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
