using MCRA.General;
using MCRA.Simulation.Constants;
using MCRA.Utils.Charting.OxyPlot;
using MCRA.Utils.ExtensionMethods;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class SingleRiskRatioHeatMapCreator : ReportChartCreatorBase {

        private readonly SingleRiskRatioSection _section;
        private readonly bool _isUncertainty;

        public override string ChartId {
            get {
                var pictureId = "19ED95E6-7E89-4386-81D8-3147B936DCDC";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId);
            }
        }

        public SingleRiskRatioHeatMapCreator(
            SingleRiskRatioSection section,
            bool isUncertainty
        ) {
            _section = section;
            _isUncertainty = isUncertainty;
            Height = 200;
            Width = 500;
        }

        public override PlotModel Create() {
            var threshold = _section.Threshold;
            var riskRecord = _section.SingleRiskRecord;
            var boxLow = _isUncertainty
                ? riskRecord.PLowerRiskUncP50
                : riskRecord.PLowerRiskNom;
            var boxHigh = _isUncertainty
                ? riskRecord.PUpperRiskUncP50
                : riskRecord.PUpperRiskNom;
            var wiskerLow = boxLow;
            var wiskerHigh = boxHigh;
            var p50 = !double.IsNaN(riskRecord.RiskP50UncP50)
                ? riskRecord.RiskP50UncP50
                : riskRecord.RiskP50Nom;

            if (_isUncertainty) {
                wiskerLow = riskRecord.PLowerRiskUncLower;
                wiskerHigh = riskRecord.PUpperRiskUncUpper;
            }
            var maximum = SimulationConstants.MOE_eps;
            p50 = !double.IsInfinity(p50) ? p50 : maximum;
            boxLow = !double.IsInfinity(boxLow) ? boxLow : maximum;
            wiskerLow = !double.IsInfinity(wiskerLow) ? wiskerLow : maximum;
            boxHigh = !double.IsInfinity(boxHigh) ? boxHigh : maximum;
            wiskerHigh = !double.IsInfinity(wiskerHigh) ? wiskerHigh : maximum;

            var xlow = _section.LeftMargin;
            var xhigh = _section.RightMargin;
            if (_section.RiskMetric == RiskMetricType.HazardExposureRatio) {
                if (boxLow < xlow && !_isUncertainty) {
                    var xx = Math.Floor(Math.Log10(boxLow) - 1);
                    xlow = Math.Pow(10, xx);
                }
                if (wiskerLow < xlow && _isUncertainty) {
                    var xx = Math.Floor(Math.Log10(wiskerLow) - 1);
                    xlow = Math.Pow(10, xx);
                }
            } else {
                if (boxHigh > xhigh && !_isUncertainty) {
                    var xx = Math.Floor(Math.Log10(boxHigh) + 1);
                    xhigh = Math.Pow(10, xx);
                }
                if (wiskerHigh > xhigh && _isUncertainty) {
                    var xx = Math.Floor(Math.Log10(wiskerHigh) + 1);
                    xhigh = Math.Pow(10, xx);
                }
            }
            return create(
                _section.SingleRiskRecord?.SubstanceName,
                xlow,
                xhigh,
                p50,
                boxLow,
                boxHigh,
                wiskerLow,
                wiskerHigh,
                threshold,
                _section.RiskMetric,
                _section.ReferenceDose,
                _section.TargetUnit?.GetShortDisplayName(),
                true
            );
        }

        private static PlotModel create(
            string substanceName,
            double xLow,
            double xHigh,
            double median,
            double boxLow,
            double boxHigh,
            double wiskerLow,
            double wiskerHigh,
            double xNeutral,
            RiskMetricType riskMetric,
            double referenceDose,
            string exposureUnit,
            bool setExposuresAxis
        ) {
            var ymin = -.5;
            var yMax = .5;
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
                    var transformValue = riskMetric == RiskMetricType.HazardExposureRatio
                        ? 2 / (1 + Math.Exp(-(transform(x) - transform(xNeutral)))) - 1
                        : -(2 / (1 + Math.Exp(-(transform(x) - transform(xNeutral)))) - 1);
                    return transformValue;
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

            var categoryAxis = new CategoryAxis() {
                MinorStep = 1,
                GapWidth = 0.1,
                IsTickCentered = true,
                TextColor = OxyColors.Black,
                Position = AxisPosition.Left,
            };
            categoryAxis.Labels.Add(substanceName);
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
                Title = $"Risk characterisation ratio ({riskMetric.GetDisplayName()})"
            };
            plotModel.Axes.Add(horizontalAxis);

            // Exposure axis: only when reference dose is available
            if (!double.IsNaN(referenceDose) && setExposuresAxis) {
                if (riskMetric == RiskMetricType.HazardExposureRatio) {
                    var horizontalUpperAxis = new LogarithmicAxis() {
                        Minimum = referenceDose / xHigh,
                        Maximum = referenceDose / xLow,
                        StartPosition = 1,
                        EndPosition = 0,
                        MinorTickSize = 0,
                        Base = 10,
                        Position = AxisPosition.Top,
                        UseSuperExponentialFormat = false,
                        Title = $"Exposure ({exposureUnit})"
                    };
                    plotModel.Axes.Add(horizontalUpperAxis);
                } else {
                    var horizontalUpperAxis = new LogarithmicAxis() {
                        Minimum = xLow * referenceDose,
                        Maximum = xHigh * referenceDose,
                        MinorTickSize = 0,
                        Base = 10,
                        Position = AxisPosition.Top,
                        UseSuperExponentialFormat = false,
                        Title = $"Exposure ({exposureUnit})"
                    };
                    plotModel.Axes.Add(horizontalUpperAxis);
                }
            }

            var boxPlotSeries = new HorizontalBoxPlotSeries() {
                Fill = OxyColors.Gray,
            };

            var item = new BoxPlotItem(0, wiskerLow, boxLow, median, boxHigh, wiskerHigh);
            boxPlotSeries.Items.Add(item);

            plotModel.Series.Add(boxPlotSeries);

            return plotModel;
        }
    }
}
