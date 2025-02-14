using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Calculators.EnvironmentalBurdenOfDiseaseCalculation;
using MCRA.Utils.ExtensionMethods;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class ExposureEffectFunctionChartCreator : ReportLineChartCreatorBase {

        private ExposureEffectFunctionSummarySection _section;

        public ExposureEffectFunctionChartCreator(ExposureEffectFunctionSummarySection section) {
            Width = 500;
            Height = 350;
            _section = section;
        }

        public override string Title => "Exposure effect function.";

        public override string ChartId {
            get {
                var pictureId = "518a01e8-b8f2-434d-89c2-286bdf86c89b";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId);
            }
        }

        public override PlotModel Create() {
            var x = _section.Records.Select(c => c.Exposure).ToList();
            var y = _section.Records.Select(c => c.Ratio).ToList();
            return create(x, y, _section.ExposureEffectFunction);
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
                0,
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

            var horizontalAxis = createLinearAxis("Exposure level (" + unit + ")");
            horizontalAxis.Position = AxisPosition.Bottom;
            plotModel.Axes.Add(horizontalAxis);

            var verticalAxis = createLinearAxis("Odds ratios");
            plotModel.Axes.Add(verticalAxis);

            return plotModel;
        }
    }
}
