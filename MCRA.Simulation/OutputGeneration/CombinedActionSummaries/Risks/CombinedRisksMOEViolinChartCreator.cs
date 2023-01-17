using MCRA.Utils.Charting.OxyPlot;
using MCRA.Utils.ExtensionMethods;
using OxyPlot;
using OxyPlot.Axes;

namespace MCRA.Simulation.OutputGeneration {
    public class CombinedRisksMOEViolinChartCreator : ViolinChartCreatorBase {

        private readonly CombinedRiskPercentilesSection _section;
        private bool _horizontal;
        private bool _boxPlotItem;
        private bool _equalSize;
        private double _lowerBound = 5;
        private double _upperBound = 95;
        private double _minimum = double.PositiveInfinity;
        private double _maximum = double.NegativeInfinity;
        private readonly double _percentile;

        public CombinedRisksMOEViolinChartCreator(
           CombinedRiskPercentilesSection section,
           double percentile,
           bool horizontal,
           bool boxplotItem,
           bool equalSize
       ) {
            Width = 700;
            Height = 100 + section.ExposureModelSummaryRecords.Count * 18;
            _section = section;
            _percentile = percentile;
            _horizontal = horizontal;
            _boxPlotItem = boxplotItem;
            _equalSize = equalSize;
            _lowerBound = section.UncertaintyLowerLimit;
            _upperBound = section.UncertaintyUpperLimit;
        }

        public override string ChartId {
            get {
                var pictureId = "d9808ef6-d58f-44a0-8f52-f72302438447";
                return StringExtensions.CreateFingerprint(_section.SectionId + _percentile + pictureId);
            }
        }

        public override string Title => !double.IsNaN(_percentile) ? $"Margin of exposure percentiles with uncertainty for p{_percentile}" : $"Margin of exposure percentile: p{_percentile.ToString("F2")}";


        /// <summary>
        /// Kernel density estimation in R"
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

            var categoryAxis = new CategoryAxis() {
                MinorStep = 1,
                Position = _horizontal ? AxisPosition.Left : AxisPosition.Bottom,
            };

            var axis = CreateLogarithmicAxis(_horizontal);
            axis.Title = "Margin of exposure";
            var models = _section.ExposureModelSummaryRecords
                .OrderBy(r => _section.GetPercentile(r.Id, _percentile)?.Exposure ?? double.NaN)
                .ThenByDescending(r => r.Name);
            var data = new Dictionary<string, List<double>>();
            var items = models
                .Select(r => _section.GetPercentile(r.Id, _percentile))
                .OrderByDescending(c => c.Name)
                .ToList();

            if (items.Any(r => r.HasUncertainty())) {
                for (int i = 0; i < items.Count; i++) {
                    var r = items[i];
                    data[r.Name] = r.UncertaintyValues;
                }

                var palette = CustomPalettes.DistinctTone(data.Count);
                //Do this for all distributions using a dictionary, otherwise RDotNetEngine should be initialized each time
                var (yKernel, xKernel, maximumY, numberOfValuesRef) = ComputeKernel(data);
                var counter = 0;
                foreach (var item in data) {
                    var areaSeries = CreateEnvelope(
                        item.Value,
                        item.Key,
                        yKernel,
                        xKernel,
                        palette,
                        counter,
                        maximumY,
                        numberOfValuesRef,
                        _horizontal,
                        _equalSize
                    );
                    plotModel.Series.Add(areaSeries);
                    counter++;
                }

                counter = 0;
                foreach (var item in data) {
                    if (_boxPlotItem) {
                        if (_horizontal) {
                            plotModel.Series.Add(CreateHorizontalBoxPlotItem(
                                item.Value,
                                palette,
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
                                palette,
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
                    categoryAxis.Labels.Add($"{item.Key}");
                    counter++;
                }
            }
            plotModel.Axes.Add(axis);
            plotModel.Axes.Add(categoryAxis);
            return plotModel;
        }
    }
}

