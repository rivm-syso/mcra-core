using MCRA.Utils.Charting.OxyPlot;
using MCRA.Utils.ExtensionMethods;
using OxyPlot;
using OxyPlot.Axes;

namespace MCRA.Simulation.OutputGeneration {
    public class CombinedRisksViolinChartCreator : ViolinChartCreatorBase {

        private readonly CombinedRiskPercentilesSection _section;
        private bool _horizontal;
        private bool _boxPlotItem;
        private bool _equalSize;
        private string _riskType;
        private double _lowerBound = 5;
        private double _upperBound = 95;
        private double _minimum = double.PositiveInfinity;
        private double _maximum = double.NegativeInfinity;
        private readonly double _percentile;

        public CombinedRisksViolinChartCreator(
           CombinedRiskPercentilesSection section,
           double percentile,
           bool horizontal,
           bool boxplotItem,
           bool equalSize,
           bool riskType
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
            _riskType = riskType ? "Threshold value/exposure" : "Exposure/threshold value";
        }

        public override string ChartId {
            get {
                var pictureId = "d9808ef6-d58f-44a0-8f52-f72302438447";
                return StringExtensions.CreateFingerprint(_section.SectionId + _percentile + pictureId);
            }
        }

        public override string Title {
            get {
                return $"Violin plots of the uncertainty distribution of the {_riskType.ToLower()} at the p{_percentile:F2} percentile of the population risk distributions. " +
                    $"The vertical lines represent the median and the lower p{_lowerBound} and upper p{_upperBound} bound of the uncertainty distribution. " +
                    $"The nominal run is indicated by the black dot.";
            }
        }

        /// <summary>
        /// Kernel density estimation in R"
        /// d <- density(data)
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
            axis.Title = _riskType;
            var models = _section.ExposureModelSummaryRecords
                .OrderBy(r => _section.GetPercentile(r.Id, _percentile)?.Risk ?? double.NaN)
                .ThenByDescending(r => r.Name);
            var data = new Dictionary<string, List<double>>();
            var items = models
                .Select(r => _section.GetPercentile(r.Id, _percentile))
                .OrderByDescending(c => c.Name)
                .ToList();
            var palette = CustomPalettes.DistinctTone(4);
            var paletteNr = 2;
            if (items.Any(r => r.HasUncertainty())) {
                for (int i = 0; i < items.Count; i++) {
                    var r = items[i];
                    data[r.Name] = r.UncertaintyValues;
                }

                //Do this for all distributions using a dictionary, otherwise RDotNetEngine should be initialized each time
                var (yKernel, xKernel, maximumY, numberOfValuesRef) = ComputeKernel(data);
                var counter = 0;
                foreach (var item in data) {
                    var areaSeries = CreateEnvelope(
                        item.Value,
                        item.Key,
                        yKernel,
                        xKernel,
                        palette.Colors[paletteNr],
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
                                palette.Colors[paletteNr],
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
                                palette.Colors[paletteNr],
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
            }
            plotModel.Axes.Add(axis);
            plotModel.Axes.Add(categoryAxis);
            return plotModel;
        }
    }
}

