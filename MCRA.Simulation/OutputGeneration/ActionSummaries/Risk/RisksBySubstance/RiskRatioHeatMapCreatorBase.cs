using MCRA.General;
using MCRA.Utils.Charting.OxyPlot;
using MCRA.Utils.ExtensionMethods;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace MCRA.Simulation.OutputGeneration {
    public abstract class RiskRatioHeatMapCreatorBase : ReportChartCreatorBase {

        protected static PlotModel create(
            double xLow,
            double xHigh,
            double xThreshold,
            List<SubstanceRiskDistributionRecord> records,
            bool isUncertainty,
            RiskMetricType riskMetric
        ) {
            var yMin = -.5;
            var yMax = records.Count - .5;

            var plotModel = new PlotModel() {
                PlotMargins = new OxyThickness(250, double.NaN, double.NaN, double.NaN),
            };
            var axisMultiplier = 0.999;
            var maximum = xHigh;

            var xx = Math.Floor(Math.Log10(maximum) + 0);
            xHigh = Math.Exp(xx * Math.Log(10)) * axisMultiplier;

            // Create heatmap series
            var heatMapSeries = createRiskHeatmapSeries(riskMetric, xLow, xHigh, xThreshold, yMin, yMax);
            plotModel.Series.Add(heatMapSeries);

            // Create risk threshold line
            (var lineSeriesShiny, var lineSeriesRisk) = createThresholdLineSeries(xThreshold, yMin, yMax);
            plotModel.Series.Add(lineSeriesShiny);
            plotModel.Series.Add(lineSeriesRisk);

            // create vertical line at 1
            var lineSeriesOne = createVerticalReferenceLine(yMin, yMax, plotModel);
            plotModel.Series.Add(lineSeriesOne);

            var categoryAxis = defaultCategoryAxis(yMin, yMax);
            plotModel.Axes.Add(categoryAxis);

            var horizontalAxis = defaultLogaritmicAxis(xLow, xHigh, riskMetric);
            plotModel.Axes.Add(horizontalAxis);

            // Create boxplot items
            var boxPlotSeries = new HorizontalBoxPlotSeries() {
                Fill = OxyColors.Gray,
            };

            var counter = 0;
            maximum = xHigh * 1.2;
            foreach (var item in records) {
                categoryAxis.Labels.Add(item.SubstanceName);
                var boxLow = item.PLowerRiskNom;
                var boxHigh = item.PUpperRiskNom;
                if (isUncertainty) {
                    boxLow = item.PLowerRiskUncP50;
                    boxHigh = item.PUpperRiskUncP50;
                }
                var wiskerLow = boxLow;
                var wiskerHigh = boxHigh;
                var median = item.RiskP50Nom;
                if (isUncertainty) {
                    median = item.RiskP50UncP50;
                    wiskerLow = item.PLowerRiskUncLower;
                    wiskerHigh = item.PUpperRiskUncUpper;
                }
                median = !double.IsInfinity(median) ? median : maximum;
                boxLow = !double.IsInfinity(boxLow) ? boxLow : maximum;
                wiskerLow = !double.IsInfinity(wiskerLow) ? wiskerLow : maximum;
                boxHigh = !double.IsInfinity(boxHigh) ? boxHigh : maximum;
                wiskerHigh = !double.IsInfinity(wiskerHigh) ? wiskerHigh : maximum;
                var bpItem = new BoxPlotItem(counter, wiskerLow, boxLow, median, boxHigh, wiskerHigh);
                boxPlotSeries.Items.Add(bpItem);
                counter++;
            }

            plotModel.Series.Add(boxPlotSeries);

            return plotModel;
        }

        protected static LogarithmicAxis defaultLogaritmicAxis(double xLow, double xHigh, RiskMetricType riskMetric) {
            return new LogarithmicAxis() {
                Minimum = xLow,
                Maximum = xHigh,
                MajorGridlineStyle = LineStyle.Dash,
                MinorGridlineStyle = LineStyle.None,
                MinorTickSize = 0,
                Base = 10,
                Position = AxisPosition.Bottom,
                UseSuperExponentialFormat = false,
                Title = $"Risk characterisation ratio ({riskMetric.GetDisplayName()})"
            };
        }

        private static CategoryAxis defaultCategoryAxis(double yMin, double yMax) {
            return new CategoryAxis() {
                MinorStep = 1,
                GapWidth = 0.1,
                IsTickCentered = true,
                TextColor = OxyColors.Black,
                Position = AxisPosition.Left,
                Minimum = yMin,
                Maximum = yMax,
            };
        }

        protected static LineSeries createVerticalReferenceLine(double yMin, double yMax, PlotModel plotModel) {
            var lineSeriesOne = new LineSeries() {
                Color = OxyColors.Black,
                StrokeThickness = .8,
                LineStyle = LineStyle.Solid,
            };
            lineSeriesOne.Points.Add(new DataPoint(1D, yMin));
            lineSeriesOne.Points.Add(new DataPoint(1D, yMax));
            return lineSeriesOne;
        }

        protected static HorizontalHeatMapSeries createRiskHeatmapSeries(
            RiskMetricType riskMetric,
            double xLow,
            double xHigh,
            double xThreshold,
            double ymin,
            double yMax
        ) {
            return new HorizontalHeatMapSeries() {
                XLow = xLow,
                XHigh = xHigh * 1.2,
                YLow = ymin,
                YHigh = yMax + .5,
                ResolutionX = 100,
                HeatMapMappingFunction = (x, y) => {
                    Func<double, double> transform = (val) => Math.Log10(val);
                    var transformValue = riskMetric == RiskMetricType.HazardExposureRatio
                        ? 2 / (1 + Math.Exp(-(transform(x) - transform(xThreshold)))) - 1
                        : -(2 / (1 + Math.Exp(-(transform(x) - transform(xThreshold)))) - 1);
                    return transformValue;
                },
            };
        }

        protected static (LineSeries lineSeriesShiny, LineSeries lineSeriesRisk) createThresholdLineSeries(double xThreshold, double yMin, double yMax) {
            // Create threshold line (shiny white background line)
            var lineSeriesShiny = new LineSeries() {
                Color = OxyColors.White,
                StrokeThickness = 4,
                LineStyle = LineStyle.Solid,
            };
            lineSeriesShiny.Points.Add(new DataPoint(xThreshold, yMin));
            lineSeriesShiny.Points.Add(new DataPoint(xThreshold, yMax));

            // Create threshold line (black foreground background line)
            var lineSeriesRisk = new LineSeries() {
                Color = OxyColors.Black,
                StrokeThickness = .8,
                LineStyle = LineStyle.Solid,
            };
            lineSeriesRisk.Points.Add(new DataPoint(xThreshold, yMin));
            lineSeriesRisk.Points.Add(new DataPoint(xThreshold, yMax));
            return (lineSeriesShiny, lineSeriesRisk);
        }
    }
}
