using MCRA.Utils.Charting.OxyPlot;
using MCRA.Utils.ExtensionMethods;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class MultipleMarginOfExposureHeatMapCreator : OxyPlotChartCreator {

        private MultipleMarginOfExposureSection _section;
        private bool _isUncertainty;
        private string _intakeUnit;

        public MultipleMarginOfExposureHeatMapCreator(MultipleMarginOfExposureSection section, bool isUncertainty, string intakeUnit) {
            _section = section;
            _isUncertainty = isUncertainty;
            var xHigh = _section.RightMargin;
            var records = _section.MoeRecords
                .Where(c => c.PLowerMOENom < xHigh)
                .ToList();
            Height = 150 + records.Count * 20;
            _intakeUnit = intakeUnit;
        }

        public override string ChartId {
            get {
                var pictureId = "6f72093c-a9ef-410b-9ba4-561cd1c88321";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId);
            }
        }

        public override PlotModel Create() {
            var threshold = _section.ThresholdMarginOfExposure;
            var xlow = _section.LeftMargin;
            var xhigh = _section.RightMargin;

            return create(xlow, xhigh, threshold, _section.MoeRecords, _isUncertainty, _intakeUnit, _section.CED);
        }

        private static PlotModel create(double xLow, double xHigh, double xNeutral, List<MarginOfExposureRecord> moeStatistics, bool isUncertainty, string intakeUnit, double CED = double.NaN) {
            var moeStatisticsPositives = moeStatistics.Where(c => c.PercentagePositives > 0).ToList();
            var RPFweightedRecord = moeStatisticsPositives.Where(c => c.IsCumulativeRecord).FirstOrDefault();
            if (RPFweightedRecord != null) {
                moeStatisticsPositives.Remove(RPFweightedRecord);
            }
            if (moeStatisticsPositives.Any(c => c.MOEP50UncP50 > 0)) {
                moeStatisticsPositives = moeStatisticsPositives.OrderByDescending(c => c.PLowerMOE_UncLower).Where(c => c.PLowerMOEUncP50 < xHigh).ToList();
            } else {
                moeStatisticsPositives = moeStatisticsPositives.OrderByDescending(c => c.PLowerMOENom).Where(c => c.PLowerMOENom < xHigh).ToList();
            }
            if (RPFweightedRecord != null) {
                moeStatisticsPositives.Add(RPFweightedRecord);
            }
            var ymin = -.5;
            var yMax = moeStatisticsPositives.Count - .5;

            var plotModel = new PlotModel() {
                PlotMargins = new OxyThickness(250, double.NaN, double.NaN, double.NaN),
            };
            var axisMultiplier = 0.999;
            var maximum = xHigh;
            //if (maximum > xHigh) {
            var xx = Math.Floor(Math.Log10(maximum) + 0);
            xHigh = Math.Exp(xx * Math.Log(10)) * axisMultiplier;
            //}
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

            var lineSeriesMoe = new LineSeries() {
                Color = OxyColors.Black,
                StrokeThickness = .8,
                LineStyle = LineStyle.Solid,
            };
            lineSeriesMoe.Points.Add(new DataPoint(xNeutral, ymin));
            lineSeriesMoe.Points.Add(new DataPoint(xNeutral, yMax));
            plotModel.Series.Add(lineSeriesMoe);

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
                Title = "Margin of exposure (MOE)"
            };
            plotModel.Axes.Add(horizontalAxis);

            if (!double.IsNaN(CED)) {
                var horizontalUpperAxis = new LogarithmicAxis() {
                    Minimum = CED / xHigh,
                    Maximum = CED / xLow,
                    MinorTickSize = 0,
                    Base = 10,
                    Position = AxisPosition.Top,
                    UseSuperExponentialFormat = false,
                    StartPosition = 1,
                    EndPosition = 0,
                    Title = $"Exposure ({intakeUnit})"
                };
                plotModel.Axes.Add(horizontalUpperAxis);
            }
            var counter = 0;
            maximum = xHigh * 1.2;
            foreach (var item in moeStatisticsPositives) {
                categoryAxis.Labels.Add(item.CompoundName);
                var boxLow = item.PLowerMOENom;
                var boxHigh = !double.IsInfinity(item.PUpperMOENom) ? item.PUpperMOENom : maximum;
                if (isUncertainty) {
                    boxLow = item.PLowerMOEUncP50;
                    boxHigh = !double.IsInfinity(item.PUpperMOEUncP50) ? item.PUpperMOEUncP50 : maximum;
                }
                var wiskerLow = boxLow;
                var wiskerHigh = boxHigh;
                var median = !double.IsNaN(item.MOEP50Nom) ? item.MOEP50Nom : boxHigh;
                median = !double.IsInfinity(median) ? median : median;
                if (isUncertainty) {
                    median = !double.IsNaN(item.MOEP50UncP50) ? item.MOEP50UncP50 : boxHigh;
                    wiskerLow = item.PLowerMOE_UncLower;
                    wiskerHigh = !double.IsInfinity(item.PUpperMOE_UncUpper) ? item.PUpperMOE_UncUpper : maximum;
                }
                var bpItem = new BoxPlotItem(counter, wiskerLow, boxLow, median, boxHigh, wiskerHigh);
                boxPlotSeries.Items.Add(bpItem);
                counter++;
            }
            return plotModel;
        }
    }
}
