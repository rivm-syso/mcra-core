using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Core.Drawing;

namespace MCRA.Utils.Charting.OxyPlot {
    /// <summary>
    /// General class which can be adapted for violin plots
    /// </summary>
    public class ViolinCreator : ViolinChartCreatorBase {

        private readonly Dictionary<string, (List<double> x, bool skip)> _data;
        private readonly bool _horizontal;
        private readonly bool _boxPlotItem;
        private readonly bool _equalSize;
        private readonly string _title;
        private readonly double _lowerBound = 5;
        private readonly double _upperBound = 95;

        public void CreateToFile(PlotModel plotModel, string filename) {
            plotModel.Background = OxyColors.White;
            PngExporter.Export(plotModel, filename, 500, 350, 96);
        }

        public ViolinCreator(IDictionary<string, List<double>> data,
                string title,
                bool horizontal = true,
                bool boxplotItem = true,
                bool equalSize = true
            ) {
            _data = data.ToDictionary(c => c.Key, c => (c.Value, false));
            _title = title;
            _horizontal = horizontal;
            _boxPlotItem = boxplotItem;
            _equalSize = equalSize;
        }

        /// <summary>
        /// Kernel density estimation in R
        /// d < -density(data)
        /// https://r-charts.com/distribution/kernel-density-plot/
        /// </summary>
        /// <returns></returns>
        public override PlotModel Create() {
            var plotModel = new PlotModel() {
                TitleFontSize = 13,
                TitleFontWeight = FontWeights.Bold,
                IsLegendVisible = false,
            };
            var axis = CreateLogarithmicAxis(_horizontal);
            var palette = CustomPalettes.DistinctTone(_data.Count);
            var (yKernel, xKernel, numberOfValuesRef) = ComputeKernel(_data);
            var maximum = xKernel.Max(k => k.Value.Max());
            var minimum = xKernel.Min(k => k.Value.Min());
            var counter = 0;
            foreach (var item in _data) {
                var areaSeries = CreateEnvelope(
                    item.Value.x,
                    item.Key,
                    yKernel,
                    xKernel,
                    palette.Colors[counter],
                    counter,
                    numberOfValuesRef,
                    _horizontal,
                    _equalSize
                );
                plotModel.Series.Add(areaSeries);
                counter++;
            }

            var categoryAxis = new CategoryAxis() {
                MinorStep = 1,
                Title = _title,
                Position = _horizontal ? AxisPosition.Left : AxisPosition.Bottom,
            };

            counter = 0;
            foreach (var item in _data) {
                if (_boxPlotItem) {
                    if (_horizontal) {
                        plotModel.Series.Add(CreateHorizontalBoxPlotItem(
                            item.Value.x,
                            palette.Colors[counter],
                            counter,
                            _lowerBound,
                            _upperBound
                        ));
                    } else {
                        plotModel.Series.Add(CreateBoxPlotItem(
                            item.Value.x,
                            palette.Colors[counter],
                            counter,
                            _lowerBound,
                            _upperBound
                        ));
                    }
                } else {
                    plotModel.Series.Add(CreateMeanSeries(
                        counter,
                        item.Value.x,
                        _horizontal
                    ));
                    var percentages = new List<double>() { 25, 50, 75 };
                    foreach (var percentage in percentages) {
                        plotModel.Series.Add(CreatePercentileSeries(
                            yKernel[item.Key],
                            xKernel[item.Key],
                            numberOfValuesRef,
                            counter,
                            item.Value.x,
                            percentage,
                            _horizontal,
                            _equalSize
                        ));
                    }
                }
                categoryAxis.Labels.Add(item.Key);
                counter++;
            }

            setSmartAxis(axis, minimum, maximum);
            plotModel.Axes.Add(axis);
            plotModel.Axes.Add(categoryAxis);
            return plotModel;
        }
    }
}
