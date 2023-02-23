using MCRA.Utils.ExtensionMethods;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class IntakePercentileChartCreator : BoxPlotChartCreatorBase {

        private IntakePercentileSection _section;
        private string _intakeUnit;

        public IntakePercentileChartCreator(IntakePercentileSection section, string intakeUnit) {
            Width = 500;
            Height = 350;
            _section = section;
            _intakeUnit = intakeUnit;
        }

        public override string ChartId {
            get {
                var pictureId = "60514de0-6a67-495c-965f-9482608310b4";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId);
            }
        }

        public override string Title => "Uncertainty of percentiles";

        public override PlotModel Create() {
            return create(_section, _intakeUnit);
        }

        private PlotModel create(IntakePercentileSection intakePercentileSection, string intakeUnit) {
            var plotModel = createDefaultPlotModel();

            var linearAxis2 = createLinearLeftAxis($"exposure ({intakeUnit})");
            linearAxis2.MajorGridlineStyle = LineStyle.Dash;
            linearAxis2.MajorTickSize = 2;

            var categoryAxis1 = new CategoryAxis() {
                MinorStep = 1,
                Title = "percentage (%)",
            };
            var series = new BoxPlotSeries() {
                Fill = OxyColor.FromAColor(100, OxyColors.RoyalBlue),
                StrokeThickness = 1,
                Stroke = OxyColors.Black,
                WhiskerWidth = 1,
            };
            var uncertaintyLowerBound = intakePercentileSection.UncertaintyLowerLimit;
            var uncertaintyUpperBound = intakePercentileSection.UncertaintyUpperLimit;

            var minimum = double.PositiveInfinity;
            var maximum = double.NegativeInfinity;
            var counter = 0;
            foreach (var item in intakePercentileSection.Percentiles) {
                var dp = asBoxPlotDataPoint(item, lowerBound: uncertaintyLowerBound, upperBound: uncertaintyUpperBound);
                categoryAxis1.Labels.Add($"p{item.XValue:F2}");
                var boxPlotItem = new BoxPlotItem(counter, dp.LowerWisker, dp.LowerBox, dp.Median , dp.UpperBox, dp.UpperWisker) {
                    Outliers = dp.Outliers,
                    Mean =item.ReferenceValue,
                };
                series.Items.Add(boxPlotItem);
                minimum = Math.Min(minimum, dp.LowerWisker);
                maximum = Math.Max(maximum, dp.UpperWisker);
                counter++;
            };
            linearAxis2.MajorStep = Math.Pow(10, Math.Ceiling(Math.Log10((maximum - minimum) / 5)));
            linearAxis2.MajorStep = linearAxis2.MajorStep > 0 ? linearAxis2.MajorStep : double.NaN;
            plotModel.Axes.Add(linearAxis2);
            plotModel.Axes.Add(categoryAxis1);

            plotModel.Series.Add(series);
            return plotModel;
        }
    }
}


