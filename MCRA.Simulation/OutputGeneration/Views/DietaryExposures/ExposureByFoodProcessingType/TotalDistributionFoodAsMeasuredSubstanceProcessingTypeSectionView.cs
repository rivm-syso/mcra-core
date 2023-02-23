using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class TotalDistributionFoodAsMeasuredSubstanceProcessingTypeSectionView : SectionView<TotalDistributionFoodAsMeasuredSubstanceProcessingTypeSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var hiddenProperties = new List<string>();
            var result = new List<FoodAsMeasuredSubstanceProcessingTypeRecord>();
            if (Model.Records?.Any() ?? false) {
                var isUncertainty = Model.Records.Any(r => r.Contributions?.Any() ?? false);
                if (!isUncertainty) {
                    hiddenProperties.Add("LowerContributionPercentage");
                    hiddenProperties.Add("UpperContributionPercentage");
                    hiddenProperties.Add("MeanContribution");
                    result = Model.Records.Where(c => c.Contribution > 0)
                        .OrderByDescending(r => r.Contribution)
                        .ThenBy(r => r.FoodName, StringComparer.OrdinalIgnoreCase)
                        .ThenBy(r => r.SubstanceName, StringComparer.OrdinalIgnoreCase)
                        .ThenBy(r => r.ProcessingTypeName)
                        .ToList();
                } else {
                    hiddenProperties.Add("ContributionPercentage");
                    result = Model.Records.Where(c => c.Contribution > 0 || c.MeanContribution > 0)
                        .OrderByDescending(r => r.MeanContribution)
                        .ThenBy(r => r.FoodName, StringComparer.OrdinalIgnoreCase)
                        .ThenBy(r => r.SubstanceName, StringComparer.OrdinalIgnoreCase)
                        .ThenBy(r => r.ProcessingTypeName)
                        .ToList();
                }

                if (Model.Records.Sum(r => r.MeanAll) > 0) {
                    var chartCreator = new TotalDistributionFoodAsMeasuredSubstanceProcessingTypePieChartCreator(Model, isUncertainty);
                    sb.AppendChart(
                        "TotalDistributionFoodAsMeasuredSubstanceProcessingTypeChart",
                        chartCreator,
                        ChartFileType.Svg,
                        Model,
                        ViewBag,
                        chartCreator.Title,
                        true
                    );

                    sb.AppendDescriptionParagraph($"Number of processed foods (modelled) and substances: {result.Count}");
                    sb.AppendTable(
                        Model,
                        result,
                        "TotalDistributionFoodAsMeasuredSubstanceProcessingTypeTable",
                        ViewBag,
                        saveCsv: true,
                        displayLimit: 20,
                        caption: "Exposure statistics by modelled food, substance and processing types (total distribution).",
                        hiddenProperties: hiddenProperties
                    );
                } else {
                    sb.AppendParagraph("No positive exposures found");
                }
            } else {
                sb.AppendParagraph("No records found.");
            }
        }
    }
}
