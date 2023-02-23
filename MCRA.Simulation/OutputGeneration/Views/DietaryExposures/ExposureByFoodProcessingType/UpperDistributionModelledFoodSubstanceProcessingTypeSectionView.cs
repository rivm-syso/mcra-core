using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class UpperDistributionModelledFoodSubstanceProcessingTypeSectionView : SectionView<UpperDistributionModelledFoodSubstanceProcessingTypeSection> {
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
                        .ThenBy(r => r.ProcessingTypeName, StringComparer.OrdinalIgnoreCase)
                        .ToList();
                } else {
                    hiddenProperties.Add("ContributionPercentage");
                    result = Model.Records.Where(c => c.Contribution > 0 || c.MeanContribution > 0)
                        .OrderByDescending(r => r.MeanContribution)
                        .ThenBy(r => r.FoodName, StringComparer.OrdinalIgnoreCase)
                        .ThenBy(r => r.SubstanceName, StringComparer.OrdinalIgnoreCase)
                        .ThenBy(r => r.ProcessingTypeName, StringComparer.OrdinalIgnoreCase)
                        .ToList();
                }

                if (Model.Records.Sum(r => r.MeanAll) > 0) {
                    var description = $"Total {result.Count} combinations of processed foods and substance with contribution in the upper tail. "
                        + $"Exposure: upper percentage {Model.UpperPercentage:F2}% ({Model.NRecords} records), "
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
                        caption: "Exposure statistics by modelled food, substance and processing types (upper distribution).",
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
