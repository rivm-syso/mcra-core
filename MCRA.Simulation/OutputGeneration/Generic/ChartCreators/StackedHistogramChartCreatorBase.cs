using MCRA.Utils.Charting.OxyPlot;
using MCRA.Utils.Statistics.Histograms;
using OxyPlot;
using OxyPlot.Legends;

namespace MCRA.Simulation.OutputGeneration {
    public abstract class StackedHistogramChartCreatorBase : HistogramChartCreatorBase {

        public bool ShowContributions { get; set; }

        protected virtual PlotModel create<T>(
            List<CategorizedHistogramBin<T>> binsTransformed,
            double fractionPositives,
            string titleX
        ) {
            var plotModel = createDefaultPlotModel();
            plotModel.IsLegendVisible = true;
            var legend = new Legend {
                LegendPlacement = LegendPlacement.Outside,
                LegendTitle = new string(' ', 20),
            };
            plotModel.Legends.Add(legend);
            var horizontalAxis = createLog10HorizontalAxis(titleX);
            horizontalAxis.LabelFormatter = (x) => $"{x:G4}";

            plotModel.Axes.Add(horizontalAxis);

            var categorizedHistogramBins = backTransformedBins(binsTransformed);

            if (ShowContributions) {
                var verticalAxis = createLinearVerticalAxis("Contribution", 100);
                plotModel.Axes.Add(verticalAxis);
            } else {
                var verticalAxis = createLinearVerticalAxis("Frequency", categorizedHistogramBins.Select(c => c.Frequency).Max() * 1.1);
                plotModel.Axes.Add(verticalAxis);
            }

            var selectedCategories = categorizedHistogramBins
                .SelectMany(c => c.ContributionFractions, (c, f) => (
                    category: f.Category,
                    contribution: f.Contribution,
                    frequency: c.Frequency,
                    midPoint: c.XMidPointValue))
                .GroupBy(c => c.category)
                .Select(r => r.Key)
                .ToList();

            var nColors = selectedCategories.Count == 1 ? 2 : selectedCategories.Count;
            var stackedHistogramSeries = new StackedHistogramSeries<T>() {
                Palette = CustomPalettes.DietaryNonDietaryColors(nColors),
                ShowContributions = ShowContributions,
                LegendaLabels = selectedCategories.Select(c => c.ToString()).ToList(),
            };
            stackedHistogramSeries.Items = categorizedHistogramBins;

            plotModel.Series.Add(stackedHistogramSeries);
            return plotModel;
        }

        /// <summary>
        /// Back-transforms the x-axis boundaries of the histogram bins, specified on
        /// a log10 scale, back to the original scale.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="binsTransformed"></param>
        /// <returns></returns>
        protected List<CategorizedHistogramBin<T>> backTransformedBins<T>(
            List<CategorizedHistogramBin<T>> binsTransformed
        ) {
            var categorizedHistogramBins = binsTransformed
                .Select(r => new CategorizedHistogramBin<T>() {
                    ContributionFractions = r.ContributionFractions,
                    Frequency = r.Frequency,
                    XMinValue = Math.Pow(10, r.XMinValue),
                    XMaxValue = Math.Pow(10, r.XMaxValue),
                })
                .ToList();
            return categorizedHistogramBins;
        }

        /// <summary>
        /// Returns the absolute total contribution fraction per category.
        /// </summary>
        /// <param name="categorizedHistogramBins"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        protected Dictionary<T, double> getAbsoluteTotalContributionFractions<T>(
            IEnumerable<CategorizedHistogramBin<T>> categorizedHistogramBins
        ) {
            var groupedCategoryContributions = categorizedHistogramBins
                .SelectMany(c => c.ContributionFractions, (c, f) => (
                    category: f.Category,
                    contribution: f.Contribution,
                    frequency: c.Frequency,
                    midPoint: c.XMidPointValue))
                .GroupBy(c => c.category)
                .Select(g => (
                    category: g.Key,
                    contribution: g.Sum(c => c.contribution * c.frequency * c.midPoint)
                ));
            var sum = groupedCategoryContributions.Sum(g => g.contribution);
            var totalContributionPerCategory = groupedCategoryContributions
                .OrderByDescending(c => c.contribution)
                .ToDictionary(c => c.category, c => c.contribution / sum);
            return totalContributionPerCategory;
        }

        /// <summary>
        /// Returns the relative total contribution fraction per category.
        /// </summary>
        /// <param name="categorizedHistogramBins"></param>
        /// <returns></returns>
        protected Dictionary<T, double> getRelativeTotalContributionFractions<T>(
            IEnumerable<CategorizedHistogramBin<T>> categorizedHistogramBins
        ) {
            var groupedCategoryContributions = categorizedHistogramBins
                .SelectMany(c => c.ContributionFractions, (c, f) => (category: f.Category, contribution: f.Contribution))
                .GroupBy(c => c.category)
                .Select(g => (
                    category: g.Key,
                    contribution: g.Sum(c => c.contribution)
                ));
            var sum = groupedCategoryContributions.Sum(g => g.contribution);
            var totalContributionPerCategory = groupedCategoryContributions
                .OrderByDescending(c => c.contribution)
                .ToDictionary(c => c.category, c => c.contribution / sum);
            return totalContributionPerCategory;
        }
    }
}
