using MCRA.Simulation.Calculators.IntakeModelling.ModelThenAddIntakeModelCalculation;
using MCRA.Utils.Charting.OxyPlot;
using MCRA.Utils.Statistics.Histograms;
using OxyPlot;

namespace MCRA.Simulation.OutputGeneration {
    public abstract class MtaDistributionByCategoryChartCreatorBase : StackedHistogramChartCreatorBase {

        public int DefaultNumber { get; set; } = 8;
        public bool OrderByRelativeContributions { get; set; } = false;

        public MtaDistributionByCategoryChartCreatorBase() {
        }

        protected PlotModel create(
            List<CategorizedIndividualExposure> positiveIntakeIndividualAmounts,
            List<Category> categories,
            string exposureUnit
        ) {
            var plotModel = createDefaultPlotModel();
            if (!positiveIntakeIndividualAmounts.Any()) {
                plotModel.Subtitle = "No positive exposures";
                plotModel.SubtitleColor = OxyColors.Red;
                plotModel.SubtitleFontWeight = 600;
                return plotModel;
            }

            plotModel.IsLegendVisible = true;
            plotModel.Legends.Add(new CustomStackedHistogramLegend<string>());

            var horizontalAxis = createLog10HorizontalAxis(
                !string.IsNullOrEmpty(exposureUnit) ? $"Exposure ({exposureUnit})" : "Exposure"
            );
            horizontalAxis.LabelFormatter = (x) => $"{x:G4}";

            plotModel.Axes.Add(horizontalAxis);

            var binsTransformed = positiveIntakeIndividualAmounts.MakeCategorizedHistogramBins(
                positiveIntakeIndividualAmounts.Select(dia => dia.SamplingWeight).ToList(),
                x => x.CategoryExposures.Select(g => new CategoryContribution<string>(g.IdCategory, g.Exposure)).ToList(),
                x => Math.Log10(x.TotalExposure)
            );

            var categorizedHistogramBins = backTransformedBins(binsTransformed);

            var verticalAxis = ShowContributions
                ? createLinearVerticalAxis("contribution", 100)
                : createLinearVerticalAxis("Frequency", categorizedHistogramBins.Select(c => c.Frequency).Max() * 1.1);
            plotModel.Axes.Add(verticalAxis);

            var totalContributionPerCategory = OrderByRelativeContributions
                ? getRelativeTotalContributionFractions(categorizedHistogramBins)  //Use this option to order by relative total contribution fractions, right plot (100% stacked histogram)
                : getAbsoluteTotalContributionFractions(categorizedHistogramBins); //Use this option to order by absolute total contribution fractions, left plot (histogram)

            var categoryNamesLookup = categories.ToDictionary(r => r.Id, r => r.Name, StringComparer.OrdinalIgnoreCase);
            var selectedCategoryIds = totalContributionPerCategory.Take(DefaultNumber).Select(c => c.Key).ToList();
            var selectedCategoryNames = selectedCategoryIds.Select(r => categoryNamesLookup.TryGetValue(r, out var name) ? name : r).ToList();
            var othersLabel = string.Empty;
            if (selectedCategoryIds.Count < totalContributionPerCategory.Count) {
                othersLabel = $"others (n={totalContributionPerCategory.Count - selectedCategoryIds.Count})";
                selectedCategoryIds.Add(othersLabel);
                selectedCategoryNames.Add(othersLabel);
            }

            var reorderedCategorizedHistogramBins = getReOrderedCategorizedBins(categorizedHistogramBins, selectedCategoryIds, selectedCategoryNames, othersLabel);
            var nColors = selectedCategoryIds.Count == 1 ? 2 : selectedCategoryIds.Count;
            var stackedHistogramSeries = new StackedHistogramSeries<string>() {
                Palette = CustomPalettes.DistinctTone(nColors),
                ShowContributions = ShowContributions,
                LegendaLabels = selectedCategoryNames
            };

            stackedHistogramSeries.Items = reorderedCategorizedHistogramBins;

            plotModel.Series.Add(stackedHistogramSeries);

            return plotModel;
        }

        /// <summary>
        /// Reorder according to selected categories
        /// </summary>
        /// <param name="categorizedHistogramBins"></param>
        /// <param name="selectedCategoryIds"></param>
        /// <param name="selectedCategoryNames"></param>
        /// <param name="othersCategoryId"></param>
        /// <returns></returns>
        protected List<CategorizedHistogramBin<string>> getReOrderedCategorizedBins(
            List<CategorizedHistogramBin<string>> categorizedHistogramBins,
            List<string> selectedCategoryIds,
            List<string> selectedCategoryNames,
            string othersCategoryId
        ) {
            var reorderedCategorizedHistogramBins = new List<CategorizedHistogramBin<string>>();
            foreach (var item in categorizedHistogramBins) {
                var rr = new CategorizedHistogramBin<string>() {
                    Frequency = item.Frequency,
                    XMaxValue = item.XMaxValue,
                    XMinValue = item.XMinValue,
                };

                var categoryContributions = new List<CategoryContribution<string>>(selectedCategoryIds.Count);
                for (int i = 0; i < selectedCategoryIds.Count; i++) {
                    categoryContributions.Add(null);
                }
                var othersContribution = new CategoryContribution<string>();
                var countOthers = 0;
                var contributionOthers = 0d;
                foreach (var cat in item.ContributionFractions) {
                    if (selectedCategoryIds.Contains(cat.Category)) {
                        var index = selectedCategoryIds.IndexOf(cat.Category);
                        var categoryContribution = new CategoryContribution<string>() {
                            Category = selectedCategoryNames[index],
                            Contribution = cat.Contribution
                        };
                        categoryContributions[index] = categoryContribution;
                    } else {
                        countOthers++;
                        contributionOthers += cat.Contribution;
                    }
                }
                if (countOthers > 0) {
                    othersContribution.Category = othersCategoryId;
                    othersContribution.Contribution = contributionOthers;
                    categoryContributions.Add(othersContribution);
                }
                while (categoryContributions.Contains(null)) {
                    categoryContributions.Remove(null);
                }
                rr.ContributionFractions.AddRange(categoryContributions);
                reorderedCategorizedHistogramBins.Add(rr);
            }
            return reorderedCategorizedHistogramBins;
        }
    }
}