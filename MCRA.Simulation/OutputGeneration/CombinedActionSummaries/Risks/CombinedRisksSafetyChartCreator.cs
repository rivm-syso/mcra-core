using MCRA.Utils.Charting.OxyPlot;
using MCRA.Utils.ExtensionMethods;
using OxyPlot.Axes;
using OxyPlot.Series;
using OxyPlot;
using MCRA.General;

namespace MCRA.Simulation.OutputGeneration.CombinedActionSummaries.Risks {
    public class CombinedRisksSafetyChartCreator : ReportChartCreatorBase {

        protected readonly CombinedRiskPercentilesSection _section;

        public CombinedRisksSafetyChartCreator(
            CombinedRiskPercentilesSection section
        ) {
            Width = 700;
            Height = 100 + section.ModelSummaryRecords.Count * 18;
            _section = section;
        }

        public override string ChartId {
            get {
                var pictureId = "39dc3d6e-e2d0-4d10-8bf1-e11f2625a1a0";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId);
            }
        }

        public override string Title {
            get {
                var str = $"The safety plot shows the variability of the risk characterisation ratio " +
                    $"{_section.RiskMetric.GetDisplayName()} in the population. The left side of the bar is the " +
                    $"p{_section.LowerPercentile:G4} percentile and the right side is the " +
                    $"p{_section.UpperPercentile:G4} of the risk distribution.";

                if (_section.HasUncertainty) {
                    str += $" The left whisker indicates the {_section.UncertaintyLowerLimit}% limit " +
                        $"of the p{_section.LowerPercentile:G4} and the right wisker the " +
                        $"{_section.UncertaintyUpperLimit}% limit of the p{_section.UpperPercentile:G4}";
                }
                return str;
            }
        }

        public override PlotModel Create() {
            var plotModel = createDefaultPlotModel();

            // Boxplot items apparently are added in reverse order, this OrderByDescending ensures the same order
            // as at other places in the output, like the table output.
            var models = _section.ModelSummaryRecords
                .OrderByDescending(r => r.Name)
                .ToList();

            var xLeftMargin = _section.LeftMarginSafetyPlot;
            var xRightMargin = _section.RightMarginSafetyPlot;
            var xThreshold = _section.Threshold;
            var ymin = -0.5;
            var yMax = models.Count - 0.5;

            // Colored heatmap from green to red
            var heatMapSeries = new HorizontalHeatMapSeries() {
                XLow = xLeftMargin,
                XHigh = xRightMargin * 1.2,
                YLow = ymin,
                YHigh = yMax + 0.5,
                ResolutionX = 100,
                HeatMapMappingFunction = (x, y) => {
                    static double transform(double val) => Math.Log10(val);
                    var value = 2 / (1 + Math.Exp(-(transform(x) - transform(xThreshold)))) - 1;
                    return _section.RiskMetric == RiskMetricType.HazardExposureRatio ? value : -value;
                },
            };
            plotModel.Series.Add(heatMapSeries);

            // Vertical line at 1
            var lineSeriesOne = new LineSeries() {
                Color = OxyColors.Black,
                StrokeThickness = .8,
                LineStyle = LineStyle.Solid,
            };
            lineSeriesOne.Points.Add(new DataPoint(1D, ymin));
            lineSeriesOne.Points.Add(new DataPoint(1D, yMax));
            plotModel.Series.Add(lineSeriesOne);

            // Vertical line threshold
            var lineSeriesThresholdShiny = new LineSeries() {
                Color = OxyColors.White,
                StrokeThickness = 4,
                LineStyle = LineStyle.Solid,
            };
            lineSeriesThresholdShiny.Points.Add(new DataPoint(xThreshold, ymin));
            lineSeriesThresholdShiny.Points.Add(new DataPoint(xThreshold, yMax));
            plotModel.Series.Add(lineSeriesThresholdShiny);

            var lineSeriesThreshold = new LineSeries() {
                Color = OxyColors.Black,
                StrokeThickness = .8,
                LineStyle = LineStyle.Solid,
            };
            lineSeriesThreshold.Points.Add(new DataPoint(xThreshold, ymin));
            lineSeriesThreshold.Points.Add(new DataPoint(xThreshold, yMax));
            plotModel.Series.Add(lineSeriesThreshold);

            // x-axis
            var xAxis = new LogarithmicAxis() {
                Minimum = xLeftMargin,
                Maximum = xRightMargin,
                MajorGridlineStyle = LineStyle.Dash,
                MinorGridlineStyle = LineStyle.None,
                MinorTickSize = 0,
                Title = _section.RiskMetric.GetDisplayName(),
                Position = AxisPosition.Bottom,
                UseSuperExponentialFormat = false
            };
            plotModel.Axes.Add(xAxis);

            // y-axis
            var categoryAxis = new CategoryAxis() {
                MinorStep = 1,
                GapWidth = 0.1,
                Position = AxisPosition.Left,
                IsTickCentered = true
            };
            plotModel.Axes.Add(categoryAxis);

            // Boxplot series
            var boxPlotSeries = new HorizontalBoxPlotSeries() {
                Fill = OxyColors.Gray,
            };
            plotModel.Series.Add(boxPlotSeries);

            var idx = 0;
            foreach (var model in models) {
                categoryAxis.Labels.Add(model.Name);
                var records = _section.CombinedPercentileRecords.Where(c => c.IdModel == model.Id).ToList();
                var hasUncertainty = records.Any(r => r.HasUncertainty);

                var lowerPercentileRecord = _section.GetPercentileRecord(model.Id, _section.LowerPercentile);
                var upperPercentileRecord = _section.GetPercentileRecord(model.Id, _section.UpperPercentile);
                var medianRecord = _section.GetPercentileRecord(model.Id, _section.MedianPercentile);

                var boxLow = lowerPercentileRecord?.Value?? xLeftMargin;
                var boxHigh = upperPercentileRecord?.Value ?? xLeftMargin;
                var median = medianRecord?.Value ?? xLeftMargin;
                var wiskerLow = boxLow;
                var wiskerHigh = boxHigh;

                if (hasUncertainty) {
                    median = medianRecord?.UncertaintyMedian ?? median;
                    boxLow = lowerPercentileRecord?.UncertaintyMedian ?? boxLow;
                    boxHigh = upperPercentileRecord?.UncertaintyMedian ?? boxHigh;
                    wiskerLow = lowerPercentileRecord?.UncertaintyLowerBound ?? boxLow;
                    wiskerHigh = upperPercentileRecord?.UncertaintyUpperBound ?? boxHigh;
                }

                boxHigh = double.IsInfinity(boxHigh) ? xRightMargin : boxHigh;
                wiskerHigh = double.IsInfinity(wiskerHigh) ? boxHigh : wiskerHigh;
                EnforceBoxPlotOrder(ref wiskerLow, ref boxLow, ref median, ref boxHigh, ref wiskerHigh);

                var bpItem = new BoxPlotItem(idx++, wiskerLow, boxLow, median, boxHigh, wiskerHigh);
                boxPlotSeries.Items.Add(bpItem);
            }

            return plotModel;
        }

        private static void EnforceBoxPlotOrder(ref double wiskerLow, ref double boxLow, ref double median, ref double boxHigh, ref double wiskerHigh) {
            if (boxLow < wiskerLow) {
                boxLow = wiskerLow;
            }
            if (median < boxLow) {
                median = boxLow;
            }
            if (boxHigh < median) {
                boxHigh = median;
            }
            if (wiskerHigh < boxHigh) {
                wiskerHigh = boxHigh;
            }
        }
    }
}
