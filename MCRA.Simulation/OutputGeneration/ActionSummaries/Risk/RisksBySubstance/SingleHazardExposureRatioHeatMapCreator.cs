﻿using MCRA.Utils.Charting.OxyPlot;
using MCRA.Utils.ExtensionMethods;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class SingleHazardExposureRatioHeatMapCreator : ReportChartCreatorBase {

        private readonly SingleHazardExposureRatioSection _section;
        private readonly bool _isUncertainty;

        public override string ChartId {
            get {
                var pictureId = "19ED95E6-7E89-4386-81D8-3147B936DCDC";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId);
            }
        }

        public SingleHazardExposureRatioHeatMapCreator(
            SingleHazardExposureRatioSection section, 
            bool isUncertainty
        ) {
            _section = section;
            _isUncertainty = isUncertainty;
            Height = 200;
            Width = 500;
        }

        public override PlotModel Create() {
            var threshold = _section.Threshold;
            var xlow = _section.LeftMargin;
            var xhigh = _section.RightMargin;
            var riskRecord = _section.RiskRecord;
            var p1 = _isUncertainty
                ? riskRecord.PLowerRiskUncP50
                : riskRecord.PLowerRiskNom;
            var p99 = _isUncertainty
                ? riskRecord.PUpperRiskUncP50
                : riskRecord.PUpperRiskNom;
            var p1_lower95 = p1;
            var p99_upper95 = p99;
            var p50 = (!double.IsNaN(riskRecord.RiskP50UncP50)) ? riskRecord.RiskP50UncP50 : p99;
            if (_isUncertainty) {
                p1_lower95 = riskRecord.PLowerRiskUncLower;
                p99_upper95 = riskRecord.PUpperRiskUncUpper;
            }
            if (p99 > xhigh) {
                var xx = Math.Floor(Math.Log10(p99) + 0);
                xhigh = Math.Exp(xx * Math.Log(10));
            }
            if (p1 < xlow && !_isUncertainty) {
                var xx = Math.Floor(Math.Log10(p1) - 1);
                xlow = Math.Pow(10, xx);
            }
            if (p1_lower95 < xlow && _isUncertainty) {
                var xx = Math.Floor(Math.Log10(p1_lower95) - 1);
                xlow = Math.Pow(10, xx);
            }
            return create(
                xlow, 
                xhigh, 
                p50, 
                p1, 
                p99, 
                p1_lower95, 
                p99_upper95,
                threshold,
                _section.RiskMetricType.GetDisplayName(),
                _section.ReferenceDose,
                _section.TargetUnit?.GetShortDisplayName(),
                true
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
            double referenceDose,
            string intakeUnit,
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
                Title = $"Risk characterisation ratio ({riskMetric})"
            };
            plotModel.Axes.Add(horizontalAxis);

            // Exposure axis: only when reference dose is available
            if (!double.IsNaN(referenceDose) && setExposuresAxis) {
                var horizontalUpperAxis = new LogarithmicAxis() {
                    Minimum = referenceDose / xHigh,
                    Maximum = referenceDose / xLow,
                    StartPosition = 1,
                    EndPosition = 0,
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
