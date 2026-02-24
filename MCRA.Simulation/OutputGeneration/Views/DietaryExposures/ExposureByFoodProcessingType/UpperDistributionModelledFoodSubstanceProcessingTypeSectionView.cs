using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class UpperDistributionModelledFoodSubstanceProcessingTypeSectionView : SectionView<UpperDistributionModelledFoodSubstanceProcessingTypeSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var hiddenProperties = new List<string>();
            var result = new List<FoodAsMeasuredSubstanceProcessingTypeRecord>();
            if (Model.Records?.Count > 0) {
                var isUncertainty = Model.Records.Any(r => r.Contributions?.Count > 0);
                if (!isUncertainty) {
                    hiddenProperties.Add(nameof(FoodAsMeasuredSubstanceProcessingTypeRecord.LowerContributionPercentage));
                    hiddenProperties.Add(nameof(FoodAsMeasuredSubstanceProcessingTypeRecord.UpperContributionPercentage));
                    hiddenProperties.Add(nameof(FoodAsMeasuredSubstanceProcessingTypeRecord.MeanContribution));
                    result = Model.Records.Where(c => c.Contribution > 0)
                        .OrderByDescending(r => r.Contribution)
                        .ThenBy(r => r.FoodName, StringComparer.OrdinalIgnoreCase)
                        .ThenBy(r => r.FoodCode, StringComparer.OrdinalIgnoreCase)
                        .ThenBy(r => r.SubstanceName, StringComparer.OrdinalIgnoreCase)
                        .ThenBy(r => r.SubstanceCode, StringComparer.OrdinalIgnoreCase)
                        .ThenBy(r => r.ProcessingTypeName, StringComparer.OrdinalIgnoreCase)
                        .ThenBy(r => r.ProcessingTypeCode, StringComparer.OrdinalIgnoreCase)
                        .ToList();
                } else {
                    hiddenProperties.Add(nameof(FoodAsMeasuredSubstanceProcessingTypeRecord.ContributionPercentage));
                    result = Model.Records.Where(c => c.Contribution > 0 || c.MeanContribution > 0)
                        .OrderByDescending(r => r.MeanContribution)
                        .ThenBy(r => r.FoodName, StringComparer.OrdinalIgnoreCase)
                        .ThenBy(r => r.FoodCode, StringComparer.OrdinalIgnoreCase)
                        .ThenBy(r => r.SubstanceName, StringComparer.OrdinalIgnoreCase)
                        .ThenBy(r => r.SubstanceCode, StringComparer.OrdinalIgnoreCase)
                        .ThenBy(r => r.ProcessingTypeName, StringComparer.OrdinalIgnoreCase)
                        .ThenBy(r => r.ProcessingTypeCode, StringComparer.OrdinalIgnoreCase)
                        .ToList();
                }

                if (Model.Records.Sum(r => r.MeanAll) > 0) {
                    var description = $"Total {result.Count} combinations of processed foods and substance with contribution in the upper tail. "
                        + $"Exposure: upper tail {Model.CalculatedUpperPercentage:F1}% ({Model.NRecords} records), "
                        + $"minimum value {Model.LowPercentileValue:G4} {ViewBag.GetUnit("IntakeUnit")}, "
                        + $"maximum value {Model.HighPercentileValue:G4} {ViewBag.GetUnit("IntakeUnit")}.";
                    sb.AppendDescriptionParagraph(description);
                    var chartCreator = new UpperDistributionModelledFoodSubstanceProcessingTypePieChartCreator(Model, isUncertainty);
                    sb.AppendChart(
                        "UpperDistributionModelledFoodSubstanceProcessingTypeChart",
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
                        "UpperDistributionModelledFoodSubstanceProcessingTypeTable",
                        ViewBag,
                        saveCsv: true,
                        displayLimit: 20,
                        caption: $"Exposure statistics by modelled food, substance and processing types for the upper tail of the distribution (estimated {Model.CalculatedUpperPercentage:F1}%).",
                        hiddenProperties: hiddenProperties
                    );
                } else {
                    sb.AppendNotification("No positive exposures.");
                }
            } else {
                sb.AppendParagraph("No records found.");
            }
        }
    }
}
