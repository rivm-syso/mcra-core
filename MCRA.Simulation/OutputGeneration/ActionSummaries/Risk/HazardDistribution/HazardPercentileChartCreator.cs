using MCRA.Utils.ExtensionMethods;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class HazardPercentileChartCreator : BoxPlotChartCreatorBase {

        private readonly HazardPercentileSection _section;

        public HazardPercentileChartCreator(HazardPercentileSection section) {
            Width = 500;
            Height = 350;
            _section = section;
        }

        public override string ChartId {
            get {
                var pictureId = "9729ffd4-1896-4707-80c2-51e8c25211ab";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId);
            }
        }

        public override string Title => "Uncertainty of percentiles";

        public override PlotModel Create() {
            return create(_section);
        }

        private PlotModel create(HazardPercentileSection section) {
            var plotModel = createDefaultPlotModel();

            var linearAxis2 = createLinearLeftAxis($"hazard ({section.TargetUnit.GetShortDisplayName()})");
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

            var minimum = double.PositiveInfinity;
            var maximum = double.NegativeInfinity;
            var counter = 0;
            foreach (var item in section.Percentiles) {
                var dp = asBoxPlotDataPoint(item);
                categoryAxis1.Labels.Add($"p{item.XValue:F2}");
                var boxPlotItem = new BoxPlotItem(counter, dp.LowerWisker, dp.LowerBox, dp.Median, dp.UpperBox, dp.UpperWisker) {
                    Outliers = dp.Outliers,
                    Mean = item.ReferenceValue,
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
