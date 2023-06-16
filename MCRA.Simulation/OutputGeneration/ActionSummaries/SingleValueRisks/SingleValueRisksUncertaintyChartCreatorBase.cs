using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace MCRA.Simulation.OutputGeneration {
    public class SingleValueRisksUncertaintyChartCreatorBase : BoxPlotChartCreatorBase {

        public SingleValueRisksThresholdExposureRatioSection _thresholdExposureSection;
        public SingleValueRisksExposureThresholdRatioSection _exposureThresholdSection;
        public string _title;
        public List<double> _adjusted;
        public List<double> _unAdjusted;
        public double _uncertaintyLowerLimit;
        public double _uncertaintyUpperLimit;

        public SingleValueRisksUncertaintyChartCreatorBase(SingleValueRisksThresholdExposureRatioSection section) {
            Width = 500;
            Height = 300;
            _thresholdExposureSection = section;
            _title = $"Threshold value/exposure.";
            _unAdjusted = section.Records.First().Risks;
            _adjusted = section.Records.First().AdjustedRisks;
            _uncertaintyLowerLimit = section.Records.First().UncertaintyLowerLimit;
            _uncertaintyUpperLimit = section.Records.First().UncertaintyUpperLimit;
        }
        public SingleValueRisksUncertaintyChartCreatorBase(SingleValueRisksExposureThresholdRatioSection section) {
            Width = 500;
            Height = 300;
            _exposureThresholdSection = section;
            _title = $"Exposure/threshold value.";
            _unAdjusted = section.Records.First().Risks;
            _adjusted = section.Records.First().AdjustedRisks;
            _uncertaintyLowerLimit = section.Records.First().UncertaintyLowerLimit;
            _uncertaintyUpperLimit = section.Records.First().UncertaintyUpperLimit;
        }
        public override string ChartId => throw new NotImplementedException();

        public override PlotModel Create() {
            throw new NotImplementedException();
        }


        protected PlotModel CreateBoxPlot() {
            var plotModel = createDefaultPlotModel();

            var linearAxis2 = createLinearLeftAxis(_title);
            linearAxis2.MajorGridlineStyle = LineStyle.Dash;
            linearAxis2.MajorTickSize = 2;

            var categoryAxis1 = new CategoryAxis() {
                MinorStep = 1,
                //Title = /*$"exposure"*/,
            };
            var seriesUnadjusted = new BoxPlotSeries() {
                Fill = OxyColor.FromAColor(100, OxyColors.Blue),
                StrokeThickness = 2,
                Stroke = OxyColors.Blue,
                WhiskerWidth = .5,
            };
            var seriesAdjusted = new BoxPlotSeries() {
                Fill = OxyColor.FromAColor(100, OxyColors.Red),
                StrokeThickness = 2,
                Stroke = OxyColors.Red,
                WhiskerWidth = .5,
            };
            var minimum = double.PositiveInfinity;
            var maximum = double.NegativeInfinity;

            (minimum, maximum) = getBoxPlot(_unAdjusted, categoryAxis1, seriesUnadjusted, _uncertaintyLowerLimit, _uncertaintyUpperLimit, minimum, maximum, 0);
            (minimum, maximum) = getBoxPlot(_adjusted, categoryAxis1, seriesAdjusted, _uncertaintyLowerLimit, _uncertaintyUpperLimit, minimum, maximum, 1, true);

            linearAxis2.MajorStep = Math.Pow(10, Math.Ceiling(Math.Log10((maximum - minimum) / 5)));
            linearAxis2.MajorStep = linearAxis2.MajorStep > 0 ? linearAxis2.MajorStep : double.NaN;
            linearAxis2.Minimum = 0;
            plotModel.Axes.Add(linearAxis2);
            plotModel.Axes.Add(categoryAxis1);
            plotModel.Series.Add(seriesUnadjusted);
            plotModel.Series.Add(seriesAdjusted);
            return plotModel;
        }

        private (double min, double max) getBoxPlot(
                List<double> source,
                CategoryAxis categoryAxis1,
                BoxPlotSeries series,
                double uncertaintyLowerBound,
                double uncertaintyUpperBound,
                double minimum,
                double maximum,
                int counter,
                bool isAdjusted = false
            ) {
            var dp = asBoxPlotDataPoint(source, lowerBound: uncertaintyLowerBound, upperBound: uncertaintyUpperBound);
            var label = isAdjusted ? "Model + Expert" : "Model";
            categoryAxis1.Labels.Add(label);
            var boxPlotItem = new BoxPlotItem(counter, dp.LowerWisker, dp.LowerBox, dp.Median, dp.UpperBox, dp.UpperWisker) {
                Outliers = dp.Outliers,
            };
            series.Items.Add(boxPlotItem);
            minimum = Math.Min(minimum, dp.LowerWisker);
            maximum = Math.Max(maximum, dp.UpperWisker);
            return (minimum, maximum);
        }
    }
}


