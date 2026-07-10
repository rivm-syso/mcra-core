using MCRA.General;
using MCRA.Utils.Charting.OxyPlot;
using MCRA.Utils.ExtensionMethods;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Legends;
using OxyPlot.Series;
using static MCRA.Simulation.OutputGeneration.CategorizedBoxPlotChartCreatorBase;

namespace MCRA.Simulation.OutputGeneration {
    public abstract class StratifiedRiskRatioHeatMapCreatorBase : RiskRatioHeatMapCreatorBase {

        protected static PlotModel create(
            List<SubstanceRiskDistributionRecord> records,
            RiskMetricType riskMetric,
            double xLow,
            double xHigh,
            double xThreshold,
            bool isUncertainty,
            TargetUnit targetUnit,
            double referenceDose,
            bool setExposuresAxis,
            List<OutputStratificationVariable> stratificationVariables = null
        ) {
            records = [.. records.Where(c => c.PercentagePositives > 0).OrderByDescending(c => c.Stratification)];
            var RPFweightedRecord = records.FirstOrDefault(c => c.IsCumulativeRecord);
            if (RPFweightedRecord != null) {
                records.Remove(RPFweightedRecord);
            }
            if (riskMetric == RiskMetricType.HazardExposureRatio) {
                if (records.Any(c => c.RiskP50UncP50 > 0)) {
                    records = [.. records.Where(c => c.PLowerRiskUncP50 < xHigh)];
                } else {
                    records = [.. records.Where(c => c.PLowerRiskNom < xHigh)];
                }
            } else {
                if (records.Any(c => c.RiskP50UncP50 > 0)) {
                    records = [.. records.Where(c => c.PUpperRiskUncP50 > xLow)];
                } else {
                    records = [.. records.Where(c => c.PUpperRiskNom > xLow)];
                }
            }
            if (RPFweightedRecord != null) {
                records.Add(RPFweightedRecord);
            }
            var yMin = -.5;
            var stratificationDimensions = stratificationVariables?.Count ?? 0;
            var descriptorStratificationIndexes = GetDescriptorStratificationIndices(stratificationVariables);
            var categoryStratificationIndexes = Enumerable
                .Range(0, stratificationDimensions)
                .Except(descriptorStratificationIndexes ?? [])
                .ToList();

            var stratificationGroups = records
                .GroupBy(r => stratificationDimensions > 0
                    ? string.Join("_", categoryStratificationIndexes.Select(ix => r.StratificationSubLevelCodes[ix]))
                    : string.Empty
                ).ToList();

            var descriptorValues = records
                .Select(r => {
                    var descriptorStratificationLevelCodes = stratificationDimensions > 0
                        ? descriptorStratificationIndexes?.Select(ix => r.StratificationSubLevelCodes[ix]).ToList()
                        : [];
                    var descriptorStratificationLevelNames = stratificationDimensions > 0
                        ? descriptorStratificationIndexes?.Select(ix => r.StratificationSubLevelNames[ix]).ToList()
                        : [];
                    return (
                        code: getLabelDescriptorString(r, descriptorStratificationLevelCodes),
                        name: getLabelDescriptorString(r, descriptorStratificationLevelNames)
                    );
                })
                .ToHashSet();
            descriptorValues = [.. descriptorValues.OrderByDescending(r => getSortKey(r.name))];

            var descriptorIndexes = descriptorValues
                .Select((r, ix) => (r, ix))
                .ToDictionary(r => r.r.code, r => (index: r.ix, label: r.r.name));
            var yMax = descriptorValues.Count - .5;

            var palette = CustomPalettes.DistinctTone(stratificationGroups.Count);

            var plotModel = new PlotModel() {
                PlotMargins = new OxyThickness(250, double.NaN, double.NaN, double.NaN),
            };
            var axisMultiplier = 0.999;
            var maximum = xHigh;
            var xx = Math.Floor(Math.Log10(maximum) + 0);
            xHigh = Math.Exp(xx * Math.Log(10)) * axisMultiplier;

            // Create heatmap series
            var heatMapSeries = createRiskHeatmapSeries(riskMetric, xLow, xHigh, xThreshold, yMin, yMax);
            plotModel.Series.Add(heatMapSeries);

            // Create risk threshold line
            (var lineSeriesShiny, var lineSeriesRisk) = createThresholdLineSeries(xThreshold, yMin, yMax);
            plotModel.Series.Add(lineSeriesShiny);
            plotModel.Series.Add(lineSeriesRisk);

            // Vertical line at 1
            // create vertical line at 1
            var lineSeriesOne = createVerticalReferenceLine(yMin, yMax, plotModel);
            plotModel.Series.Add(lineSeriesOne);

            var horizontalAxis = defaultLogaritmicAxis(xLow, xHigh, riskMetric);
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
                        Title = $"Exposure ({targetUnit?.GetShortDisplayName()})"
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
                        Title = $"Exposure ({targetUnit?.GetShortDisplayName()})"
                    };
                    plotModel.Axes.Add(horizontalUpperAxis);
                }
            }

            var counter = 0;
            maximum = xHigh * 1.2;
            foreach (var group in stratificationGroups) {
                // Create boxplot series for category
                var title = stratificationDimensions > 0
                    ? string.Join(", ", categoryStratificationIndexes.Select(ix => group.First().StratificationSubLevelNames[ix]))
                    : string.Empty;

                var series = new MultipleWhiskerHorizontalBoxPlotSeries() {
                    Title = title,
                    Fill = OxyColor.FromAColor(200, palette.Colors[counter]),
                    StrokeThickness = .50,
                    Stroke = OxyColors.Black,
                    BoxWidth = 0.8,
                    WhiskerWidth = 1.1,
                };
                foreach (var record in group) {
                    var descriptorStratificationLevelCodes = stratificationDimensions > 0
                        ? descriptorStratificationIndexes?.Select(ix => record.StratificationSubLevelCodes[ix]).ToList()
                        : [];
                    var descriptorLabelCode = getLabelDescriptorString(record, descriptorStratificationLevelCodes);
                    var ix = descriptorIndexes[descriptorLabelCode].index;

                    var boxLow = record.PLowerRiskNom;
                    var boxHigh = riskMetric == RiskMetricType.HazardExposureRatio
                            ? !double.IsInfinity(record.PUpperRiskNom) ? record.PUpperRiskNom : maximum
                            : record.PUpperRiskNom;
                    if (isUncertainty) {
                        boxLow = record.PLowerRiskUncP50;
                        boxHigh = riskMetric == RiskMetricType.HazardExposureRatio
                            ? !double.IsInfinity(record.PUpperRiskUncP50) ? record.PUpperRiskUncP50 : maximum
                            : record.PUpperRiskUncP50;
                    }
                    var wiskerLow = boxLow;
                    var wiskerHigh = boxHigh;
                    var median = !double.IsNaN(record.RiskP50Nom) ? record.RiskP50Nom : boxHigh;
                    if (isUncertainty) {
                        median = !double.IsNaN(record.RiskP50UncP50) ? record.RiskP50UncP50 : boxHigh;
                        wiskerLow = record.PLowerRiskUncLower;
                        wiskerHigh = riskMetric == RiskMetricType.HazardExposureRatio
                            ? !double.IsInfinity(record.PUpperRiskUncUpper) ? record.PUpperRiskUncUpper : maximum
                            : record.PUpperRiskUncUpper;
                    }
                    var multipleWhiskerBoxPlotItem = new MultipleWhiskerBoxPlotItem(
                        new BoxPlotItem(ix, wiskerLow, boxLow, median, boxHigh, wiskerHigh),
                        boxLow,
                        boxHigh
                    );
                    series.Items.Add(multipleWhiskerBoxPlotItem);
                }
                counter++;
                plotModel.Series.Add(series);
            }

            var categoryAxis = new CategoryAxis() {
                MinorStep = 1,
                MajorStep = 1,
                Position = AxisPosition.Left,
                IsTickCentered = false,
                MajorGridlineStyle = LineStyle.Solid,
                MajorGridlineColor = OxyColors.Black,
                GapWidth = 0.1
            };

            foreach (var category in descriptorValues) {
                categoryAxis.Labels.Add(category.name);
            }
            plotModel.Axes.Add(categoryAxis);

            var legend = new PositionLegend {
                LegendBackground = OxyColors.Transparent,
                LegendBorder = OxyColors.Undefined,
                LegendOrientation = LegendOrientation.Vertical,
                LegendPlacement = LegendPlacement.Outside,
                LegendPosition = LegendPosition.RightTop,
                LegendItemOrder = LegendItemOrder.Reverse,
            };
            plotModel.Legends.Add(legend);
            plotModel.IsLegendVisible = true;
            return plotModel;
        }

        private static string getLabelDescriptorString(
            SubstanceRiskDistributionRecord c,
            List<string> descriptorStratificationLevels
        ) {
            if (descriptorStratificationLevels != null && descriptorStratificationLevels.Count != 0) {
                var descriptorString = string.Join(", ", descriptorStratificationLevels);
                return $"{c.GetLabel()}{Environment.NewLine}{descriptorString}";
            }
            return c.GetLabel();
        }
    }
}
