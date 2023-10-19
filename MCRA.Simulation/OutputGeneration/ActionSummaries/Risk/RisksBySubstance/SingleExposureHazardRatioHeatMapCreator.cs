using MCRA.Utils.Charting.OxyPlot;
using MCRA.Utils.ExtensionMethods;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class SingleExposureHazardRatioHeatMapCreator : OxyPlotChartCreator {
        private readonly double _eps = 1 / 10E7D;

        private readonly SingleExposureHazardRatioSection _section;
        private readonly bool _isUncertainty;

        public SingleExposureHazardRatioHeatMapCreator(SingleExposureHazardRatioSection section, bool isUncertainty) {
            _section = section;
            _isUncertainty = isUncertainty;
            Height = 200;
            Width = 500;
        }

        public override string ChartId {
            get {
                var pictureId = "b1186e36-dbcf-4f9c-aeca-cf452d3a7bb8";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId);
            }
        }

        public override PlotModel Create() {
            var threshold = _section.Threshold;
            var xlow = _section.LeftMargin;
            var xhigh = _section.RightMargin;
            var riskRecord = _section.RiskRecord;
            var p1 = _isUncertainty
                ? (riskRecord.PLowerRiskUncP50 != 0 ? riskRecord.PLowerRiskUncP50 : _eps)
                : (riskRecord.PLowerRiskNom != 0 ? riskRecord.PLowerRiskNom : _eps);
            var p99 = _isUncertainty
                ? riskRecord.PUpperRiskUncP50
                : riskRecord.PUpperRiskNom;

            var p1_lower95 = p1;
            var p99_upper95 = p99;
            var p50 = (!double.IsNaN(riskRecord.RiskP50UncP50)) ? riskRecord.RiskP50UncP50 : p99;
            if (_isUncertainty) {
                p1_lower95 = riskRecord.PLowerRiskUncLower != 0 ? riskRecord.PLowerRiskUncLower : _eps;
                p99_upper95 = riskRecord.PUpperRiskUncUpper;
            }
            if (p99 > xhigh && !_isUncertainty) {
                var xx = Math.Floor(Math.Log10(p99) + 1);
                xhigh = Math.Pow(10, xx);
            }
            if (p99_upper95 > xhigh && _isUncertainty) {
                var xx = Math.Floor(Math.Log10(p99_upper95) + 1);
                xhigh = Math.Pow(10, xx);
            }
            return create(
                xLow: xlow,
                xHigh: xhigh,
                median: p50,
                boxLow: p1,
                boxHigh: p99,
                wiskerLow: p1_lower95,
                wiskerHigh: p99_upper95,
                xNeutral: threshold,
                riskMetric: _section.RiskMetricType.GetDisplayName(),
                referenceDose: _section.ReferenceDose,
                intakeUnit: _section.TargetUnit?.GetShortDisplayName(),
                setExposuresAxis: true
           );
        }

        private static PlotModel create(
            double xLow,
            double xHigh,
            double median,
            double boxLow,
            double boxHigh,
            double wiskerLow,
            double wiskerHigh,
            double xNeutral,
            string riskMetric,
            string intakeUnit,
            double referenceDose,
            bool setExposuresAxis
        ) {
            var ymin = -.5;
            var yMax = 0.5;
            var plotModel = new PlotModel() {
                IsLegendVisible = false,
            };
            var heatMapSeries = new HorizontalHeatMapSeries() {
                XLow = xLow,
                XHigh = xHigh,
                YLow = -1,
                YHigh = 1,
                ResolutionX = 100,
                ResolutionY = 2,
                HeatMapMappingFunction = (x, y) => {
                    Func<double, double> transform = (val) => Math.Log10(val);
                    return -(2 / (1 + Math.Exp(-(transform(x) - transform(xNeutral)))) - 1);
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
                TextColor = OxyColors.Red,
                Position = AxisPosition.Left
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
                Title = $"Risk ratio ({riskMetric})",
            };
            plotModel.Axes.Add(horizontalAxis);

            // Exposure axis: only when reference dose is available
            if (!double.IsNaN(referenceDose) && setExposuresAxis) {
                var horizontalUpperAxis = new LogarithmicAxis() {
                    Minimum = xLow * referenceDose,
                    Maximum = xHigh * referenceDose,
                    MinorTickSize = 0,
                    Base = 10,
                    Position = AxisPosition.Top,
                    UseSuperExponentialFormat = false,
                    Title = $"Exposure ({intakeUnit})"
                };
                plotModel.Axes.Add(horizontalUpperAxis);
            }

            var item = new BoxPlotItem(0, wiskerLow, boxLow, median, boxHigh, wiskerHigh);
            boxPlotSeries.Items.Add(item);

            return plotModel;
        }
    }
}
