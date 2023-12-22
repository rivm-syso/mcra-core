using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Core.Drawing;

namespace MCRA.Utils.Charting.OxyPlot {
    /// <summary>
    /// General class which can be adapted for violin plots
    /// </summary>
    public class ViolinCreator : ViolinChartCreatorBase {

        private IDictionary<string, List<double>> _data;
        private bool _horizontal;
        private bool _boxPlotItem;
        private bool _equalSize;
        private string _title;
        private double _lowerBound = 5;
        private double _upperBound = 95;
        private double _minimum = double.PositiveInfinity;
        private double _maximum = double.NegativeInfinity;

        public void CreateToFile(PlotModel plotModel, string filename) {
            plotModel.Background = OxyColors.White; 
            PngExporter.Export(plotModel, filename, 500, 350, 96);
        }

        public override string ChartId => throw new NotImplementedException();

        public ViolinCreator(IDictionary<string, List<double>> data,
                string title,
                bool horizontal = true,
                bool boxplotItem = true,
                bool equalSize = true
            ) {
            _data = data;
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
            var (yKernel, xKernel, maximumY, numberOfValuesRef) = ComputeKernel(_data);

            var counter = 0;
            foreach (var item in _data) {
                var areaSeries = CreateEnvelope(
                    item.Value,
                    item.Key,
                    yKernel,
                    xKernel,
                    palette.Colors[counter],
                    counter,
                    maximumY,
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
                            item.Value,
                            palette.Colors[counter],
                            axis,
                            counter,
                            _lowerBound,
                            _upperBound,
                            _minimum,
                            _maximum
                        ));
                    } else {
                        plotModel.Series.Add(CreateBoxPlotItem(
                            item.Value,
                            palette.Colors[counter],
                            axis,
                            counter,
                            _lowerBound,
                            _upperBound,
                            _minimum,
                            _maximum
                        ));
                    }
                } else {
                    plotModel.Series.Add(CreateMeanSeries(
                        counter,
                        item.Value,
                        _horizontal
                    ));
                    var percentages = new List<double>() { 25, 50, 75 };
                    foreach (var percentage in percentages) {
                        plotModel.Series.Add(CreatePercentileSeries(
                            yKernel[item.Key],
                            xKernel[item.Key],
                            maximumY,
                            numberOfValuesRef,
                            counter,
                            item.Value,
                            percentage,
                            _horizontal,
                            _equalSize,
                            axis,
                            _minimum,
                            _maximum
                        ));
                    }
                }
                categoryAxis.Labels.Add(item.Key);
                counter++;
            }
            plotModel.Axes.Add(axis);
            plotModel.Axes.Add(categoryAxis);
            return plotModel;
        }
    }
}
