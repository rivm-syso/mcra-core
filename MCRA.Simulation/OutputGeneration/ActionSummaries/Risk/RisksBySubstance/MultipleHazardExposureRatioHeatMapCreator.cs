using MCRA.General;
using MCRA.Utils.Charting.OxyPlot;
using MCRA.Utils.ExtensionMethods;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class MultipleHazardExposureRatioHeatMapCreator : ReportChartCreatorBase {

        private readonly MultipleHazardExposureRatioSection _section;
        private readonly TargetUnit _targetUnit;
        private readonly bool _isUncertainty;

        public override string ChartId {
            get {
                var pictureId = "6f72093c-a9ef-410b-9ba4-561cd1c88321";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId);
            }
        }

        public MultipleHazardExposureRatioHeatMapCreator(
            MultipleHazardExposureRatioSection section,
            TargetUnit targetUnit,
            bool isUncertainty
        ) {
            _section = section;
            _targetUnit = targetUnit;
            _isUncertainty = isUncertainty;
            var xHigh = _section.RightMargin;
            var records = _section.RiskRecords
                .SingleOrDefault(c => c.Target == _targetUnit?.Target)
                .Records
                .Where(c => c.PLowerRiskNom < xHigh)
                .ToList();
            Height = 150 + records.Count * 20;
        }

        public override PlotModel Create() {
            var threshold = _section.Threshold;
            var xlow = _section.LeftMargin;
            var xhigh = _section.RightMargin;
            return create(
                xlow,
                xhigh,
                threshold,
                _section.RiskRecords.SingleOrDefault(c => c.Target == _targetUnit?.Target).Records,
                _isUncertainty,
                _section.RiskMetricType.GetDisplayName()
            );
        }

        private static PlotModel create(
            double xLow,
            double xHigh,
            double xNeutral,
            List<SubstanceRiskDistributionRecord> terStatistics,
            bool isUncertainty,
            string riskMetric
        ) {
            var riskStatisticsPositives = terStatistics.Where(c => c.PercentagePositives > 0).ToList();
            var RPFweightedRecord = riskStatisticsPositives.FirstOrDefault(c => c.IsCumulativeRecord);
            if (RPFweightedRecord != null) {
                riskStatisticsPositives.Remove(RPFweightedRecord);
            }
            if (riskStatisticsPositives.Any(c => c.RiskP50UncP50 > 0)) {
                riskStatisticsPositives = riskStatisticsPositives.OrderByDescending(c => c.PLowerRiskUncLower).Where(c => c.PLowerRiskUncP50 < xHigh).ToList();
            } else {
                riskStatisticsPositives = riskStatisticsPositives.OrderByDescending(c => c.PLowerRiskNom).Where(c => c.PLowerRiskNom < xHigh).ToList();
            }
            if (RPFweightedRecord != null) {
                riskStatisticsPositives.Add(RPFweightedRecord);
            }
            var ymin = -.5;
            var yMax = riskStatisticsPositives.Count - .5;

            var plotModel = new PlotModel() {
                PlotMargins = new OxyThickness(250, double.NaN, double.NaN, double.NaN),
            };
            var axisMultiplier = 0.999;
            var maximum = xHigh;
            var xx = Math.Floor(Math.Log10(maximum) + 0);
            xHigh = Math.Exp(xx * Math.Log(10)) * axisMultiplier;
            var heatMapSeries = new HorizontalHeatMapSeries() {
                XLow = xLow,
                XHigh = xHigh * 1.2,
                YLow = ymin,
                YHigh = yMax + .5,
                ResolutionX = 100,
                HeatMapMappingFunction = (x, y) => {
                    double transform(double val) => Math.Log10(val);
                    return 2 / (1 + Math.Exp(-(transform(x) - transform(xNeutral)))) - 1;
                },
            };
            plotModel.Series.Add(heatMapSeries);

            var lineSeriesShiny = new LineSeries() {
                Color = OxyColors.White,
                StrokeThickness = 4,
                LineStyle = LineStyle.Solid,
            };
            lineSeriesShiny.Points.Add(new DataPoint(xNeutral, ymin));
            lineSeriesShiny.Points.Add(new DataPoint(xNeutral, yMax));
            plotModel.Series.Add(lineSeriesShiny);

            var lineSeriesRisk = new LineSeries() {
                Color = OxyColors.Black,
                StrokeThickness = .8,
                LineStyle = LineStyle.Solid,
            };
            lineSeriesRisk.Points.Add(new DataPoint(xNeutral, ymin));
            lineSeriesRisk.Points.Add(new DataPoint(xNeutral, yMax));
            plotModel.Series.Add(lineSeriesRisk);

            // Vertical line at 1
            var lineSeriesOne = new LineSeries() {
                Color = OxyColors.Black,
                StrokeThickness = .8,
                LineStyle = LineStyle.Solid,
            };
            lineSeriesOne.Points.Add(new DataPoint(1D, ymin));
            lineSeriesOne.Points.Add(new DataPoint(1D, yMax));
            plotModel.Series.Add(lineSeriesOne);

            var boxPlotSeries = new HorizontalBoxPlotSeries() {
                Fill = OxyColors.Gray,
            };

            plotModel.Series.Add(boxPlotSeries);

            var categoryAxis = new CategoryAxis() {
                MinorStep = 1,
                GapWidth = 0.1,
                IsTickCentered = true,
                TextColor = OxyColors.Black,
                Position = AxisPosition.Left,
                Minimum = ymin,
                Maximum = yMax,
            };
            plotModel.Axes.Add(categoryAxis);

            var horizontalAxis = new LogarithmicAxis() {
                Minimum = xLow,
                Maximum = xHigh,
                MajorGridlineStyle = LineStyle.Dash,
                MinorGridlineStyle = LineStyle.None,
                MinorTickSize = 0,
                Base = 10,
                Position = AxisPosition.Bottom,
                UseSuperExponentialFormat = false,
                Title = $"Risk characterisation ratio ({riskMetric})"
            };
            plotModel.Axes.Add(horizontalAxis);

            var counter = 0;
            maximum = xHigh * 1.2;
            foreach (var item in riskStatisticsPositives) {
                categoryAxis.Labels.Add(item.SubstanceName);
                var boxLow = item.PLowerRiskNom;
                var boxHigh = !double.IsInfinity(item.PUpperRiskNom) ? item.PUpperRiskNom : maximum;
                if (isUncertainty) {
                    boxLow = item.PLowerRiskUncP50;
                    boxHigh = !double.IsInfinity(item.PUpperRiskUncP50) ? item.PUpperRiskUncP50 : maximum;
                }
                var wiskerLow = boxLow;
                var wiskerHigh = boxHigh;
                var median = !double.IsNaN(item.RiskP50Nom) ? item.RiskP50Nom : boxHigh;
                median = !double.IsInfinity(median) ? median : median;
                if (isUncertainty) {
                    median = !double.IsNaN(item.RiskP50UncP50) ? item.RiskP50UncP50 : boxHigh;
                    wiskerLow = item.PLowerRiskUncLower;
                    wiskerHigh = !double.IsInfinity(item.PUpperRiskUncUpper) ? item.PUpperRiskUncUpper : maximum;
                }
                var bpItem = new BoxPlotItem(counter, wiskerLow, boxLow, median, boxHigh, wiskerHigh);
                boxPlotSeries.Items.Add(bpItem);
                counter++;
            }
            return plotModel;
        }
    }
}
