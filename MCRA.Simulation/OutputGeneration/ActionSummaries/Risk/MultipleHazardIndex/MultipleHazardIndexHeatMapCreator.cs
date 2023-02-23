using MCRA.Utils.Charting.OxyPlot;
using MCRA.Utils.ExtensionMethods;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class MultipleHazardIndexHeatMapCreator : OxyPlotChartCreator {

        private MultipleHazardIndexSection _section;
        private bool _isUncertainty;
        private string _intakeUnit;

        public MultipleHazardIndexHeatMapCreator(MultipleHazardIndexSection section, bool isUncertainty, string intakeUnit) {
            _section = section;
            _isUncertainty = isUncertainty;
            var xLow = _section.LeftMargin;
            var records = _section.HazardIndexRecords
                .Where(c => c.PUpperHINom > xLow)
                .ToList();
            Height = 150 + records.Count * 20;
            _intakeUnit = intakeUnit;
        }

        public override string ChartId {
            get {
                var pictureId = "53bfec34-8109-4872-ad36-8e77c18e7131";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId);
            }
        }

        public override PlotModel Create() {
            var threshold = _section.ThresholdHazardIndex;
            var xlow = _section.LeftMargin;
            var xhigh = _section.RightMargin;

            return create(xlow, xhigh, threshold, _section.HazardIndexRecords, _isUncertainty, _intakeUnit, _section.CED);
        }

        private static PlotModel create(double xLow, double xHigh, double xNeutral, List<HazardIndexRecord> hiStatistics, bool isUncertainty, string intakeUnit, double CED = double.NaN) {
            var hiStatisticsPositives = hiStatistics
                .Where(c => c.PercentagePositives > 0)
                .ToList();
            var RPFweightedRecord = hiStatisticsPositives
                .Where(c => c.IsCumulativeRecord)
                .FirstOrDefault();
            if (RPFweightedRecord != null) {
                hiStatisticsPositives.Remove(RPFweightedRecord);
            }
            if (hiStatisticsPositives.Any(c => c.HIP50UncP50 > 0)) {
                hiStatisticsPositives = hiStatisticsPositives.OrderBy(c => c.PUpperHI_UncUpper).Where(c => c.PUpperHIUncP50 > xLow).ToList();
            } else {
                hiStatisticsPositives = hiStatisticsPositives.OrderBy(c => c.PUpperHINom).Where(c => c.PUpperHINom > xLow).ToList();
            }
            if (RPFweightedRecord != null) {
                hiStatisticsPositives.Add(RPFweightedRecord);
            }
            var ymin = -.5;
            var yMax = hiStatisticsPositives.Count - 0.5;

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
                YHigh = yMax + 0.5,
                ResolutionX = 100,
                HeatMapMappingFunction = (x, y) => {
                    double transform(double val) => Math.Log10(val);
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

            var lineSeriesHI = new LineSeries() {
                Color = OxyColors.Black,
                StrokeThickness = .8,
                LineStyle = LineStyle.Solid,
            };
            lineSeriesHI.Points.Add(new DataPoint(xNeutral, ymin));
            lineSeriesHI.Points.Add(new DataPoint(xNeutral, yMax));
            plotModel.Series.Add(lineSeriesHI);

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
                //Minimum = ymin,
                //Maximum = yMax,
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
                Title = "Hazard Index (HI)"
            };
            plotModel.Axes.Add(horizontalAxis);

            if (!double.IsNaN(CED)) {
                var horizontalUpperAxis = new LogarithmicAxis() {
                    Minimum = xLow * CED,
                    Maximum = xHigh * CED,
                    MinorTickSize = 0,
                    Base = 10,
                    Position = AxisPosition.Top,
                    UseSuperExponentialFormat = false,
                    Title = $"Exposure ({intakeUnit})"
                };
                plotModel.Axes.Add(horizontalUpperAxis);
            }
            var counter = 0;

            foreach (var item in hiStatisticsPositives) {
                categoryAxis.Labels.Add(item.CompoundName);
                var boxLow = item.PLowerHINom;
                var boxHigh = item.PUpperHINom;
                if (isUncertainty) {
                    boxLow = item.PLowerHIUncP50;
                    boxHigh = item.PUpperHIUncP50;
                }
                var wiskerLow = boxLow;
                var wiskerHigh = boxHigh;
                var median = (!double.IsNaN(item.HIP50Nom)) ? item.HIP50Nom : boxHigh;
                if (isUncertainty) {
                    median = (!double.IsNaN(item.HIP50UncP50)) ? item.HIP50UncP50 : boxHigh;
                    wiskerLow = item.PLowerHI_UncLower;
                    wiskerHigh = item.PUpperHI_UncUpper;
                }
                var bpItem = new BoxPlotItem(counter, wiskerLow, boxLow, median, boxHigh, wiskerHigh);
                boxPlotSeries.Items.Add(bpItem);
                counter++;
            }
            return plotModel;
        }
    }
}
