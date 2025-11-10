using MCRA.Simulation.OutputGeneration.CombinedActionSummaries;
using MCRA.Utils.Charting.OxyPlot;
using OxyPlot;
using OxyPlot.Axes;

namespace MCRA.Simulation.OutputGeneration {
    public abstract class CombinedViolinChartCreatorBase : ReportViolinChartCreatorBase {
        private readonly bool _horizontal;
        private readonly bool _boxPlotItem;
        private readonly bool _equalSize;
        private readonly double _minimum = double.PositiveInfinity;
        private readonly double _maximum = double.NegativeInfinity;

        protected readonly double _lowerBound = 5;
        protected readonly double _upperBound = 95;
        protected readonly double _percentile;
        protected readonly CombinedPercentilesSectionBase _section;

        public CombinedViolinChartCreatorBase(
           CombinedPercentilesSectionBase section,
           double percentile,
           bool horizontal,
           bool boxplotItem,
           bool equalSize
       ) {
            Width = 700;
            Height = 100 + section.ModelSummaryRecords.Count * 18;
            _section = section;
            _percentile = percentile;
            _horizontal = horizontal;
            _boxPlotItem = boxplotItem;
            _equalSize = equalSize;
            _lowerBound = section.UncertaintyLowerLimit;
            _upperBound = section.UncertaintyUpperLimit;
        }

        /// <summary>
        /// Kernel density estimation in R"
        /// d <- density(data)
        /// https://r-charts.com/distribution/kernel-density-plot/
        /// </summary>
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
            axis.Title = "Exposure";
            var models = _section.ModelSummaryRecords
                .OrderBy(r => _section.GetPercentileRecord(r.Id, _percentile)?.Value ?? double.NaN)
                .ThenByDescending(r => r.Name);
            var data = new Dictionary<string, (List<double> x, bool skip)>();
            var items = models
                .Select(r => _section.GetPercentileRecord(r.Id, _percentile))
                .OrderByDescending(c => c.Name)
                .ToList();

            var palette = CustomPalettes.DistinctTone(4);
            var paletteNr = 2;
            if (items.Any(r => r.HasUncertainty)) {
                for (int i = 0; i < items.Count; i++) {
                    var r = items[i];
                    var skip = double.IsInfinity((double)r.UncertaintyLowerBound) || double.IsInfinity((double)r.UncertaintyUpperBound);
                    data[r.Name] = (r.UncertaintyValues, skip);
                }

                //Do this for all distributions using a dictionary, otherwise RDotNetEngine should be initialized each time
                var (yKernel, xKernel, maximumY, numberOfValuesRef) = ComputeKernel(data);
                var counter = 0;
                foreach (var item in data) {
                    if (!item.Value.skip) {
                        var areaSeries = CreateEnvelope(
                            item.Value.x,
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
                    }
                    counter++;
                }

                counter = 0;
                foreach (var item in data) {
                    if (_boxPlotItem) {
                        if (_horizontal) {
                            plotModel.Series.Add(CreateHorizontalBoxPlotItem(
                                item.Value.x,
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
                                item.Value.x,
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
                            item.Value.x,
                            _horizontal
                        ));
                        if (!item.Value.skip) {
                            var percentages = new List<double>() { _lowerBound, 50, _upperBound };
                            foreach (var percentage in percentages) {
                                plotModel.Series.Add(CreatePercentileSeries(
                                    yKernel[item.Key],
                                    xKernel[item.Key],
                                    maximumY,
                                    numberOfValuesRef,
                                    counter,
                                    item.Value.x,
                                    percentage,
                                    _horizontal,
                                    _equalSize,
                                    axis,
                                    _minimum,
                                    _maximum
                                ));
                            }
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
