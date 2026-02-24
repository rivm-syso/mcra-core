using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class UpperDistributionFoodCompoundSectionView : SectionView<UpperDistributionFoodCompoundSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var hiddenProperties = new List<string>();

            if (Model.Records.All(c => c.CompoundCode == null)) {
                hiddenProperties.Add(nameof(DistributionFoodCompoundRecord.CompoundCode));
            }
            if (Model.Records.All(c => c.FoodCode == null)) {
                hiddenProperties.Add(nameof(DistributionFoodCompoundRecord.FoodCode));
            }
            var result = new List<DistributionFoodCompoundRecord>();
            var isUncertainty = Model.Records.Any() && Model.Records.First().Contributions.Any();
            if (!isUncertainty) {
                hiddenProperties.Add(nameof(DistributionFoodCompoundRecord.LowerContributionPercentage));
                hiddenProperties.Add(nameof(DistributionFoodCompoundRecord.UpperContributionPercentage));
                hiddenProperties.Add(nameof(DistributionFoodCompoundRecord.MeanContribution));
                result = Model.Records.Where(c => c.Contribution > 0)
                    .OrderByDescending(r => r.Contribution)
                    .ThenBy(r => r.FoodName, StringComparer.OrdinalIgnoreCase)
                    .ThenBy(r => r.FoodCode, StringComparer.OrdinalIgnoreCase)
                    .ThenBy(r => r.CompoundName, StringComparer.OrdinalIgnoreCase)
                    .ThenBy(r => r.CompoundCode, StringComparer.OrdinalIgnoreCase)
                    .ToList();
            } else {
                result = Model.Records.Where(c => c.Contribution > 0 || c.MeanContribution > 0)
                    .OrderByDescending(r => r.MeanContribution)
                    .ThenBy(r => r.FoodName, StringComparer.OrdinalIgnoreCase)
                    .ThenBy(r => r.FoodCode, StringComparer.OrdinalIgnoreCase)
                    .ThenBy(r => r.CompoundName, StringComparer.OrdinalIgnoreCase)
                    .ThenBy(r => r.CompoundCode, StringComparer.OrdinalIgnoreCase)
                    .ToList();
                hiddenProperties.Add("ContributionPercentage");
            }

            //Render HTML
            if (Model.Records.Sum(r => r.MeanAll) > 0) {
                var description = $"Total {result.Count} combinations of foods and substance with contribution in the upper tail. "
                    + $"Exposure: upper tail {Model.CalculatedUpperPercentage:F1}% ({Model.NRecords} records), "
                    + $"minimum value {Model.LowPercentileValue:G4} {ViewBag.GetUnit("IntakeUnit")}, "
                    + $"maximum value {Model.HighPercentileValue:G4} {ViewBag.GetUnit("IntakeUnit")}.";
                sb.AppendDescriptionParagraph(description);

                if (result.Count > 1) {
                    var chartCreator = new UpperDistributionFoodCompoundPieChartCreator(Model, isUncertainty);
                    sb.AppendChart(
                        "UpperDistributionFoodSubstanceChart",
                        chartCreator,
                        ChartFileType.Svg,
                        Model,
                        ViewBag,
                        chartCreator.Title,
                        true
                    );
                }

                sb.AppendTable(
                    Model,
                    result,
                    "UpperDistributionFoodCompoundTable",
                    ViewBag,
                    caption: $"Exposure statistics by food x substance for the upper tail of the distribution (estimated {Model.CalculatedUpperPercentage:F1}%). RPFs are not applied except for exposure contribution.",
                    saveCsv: true,
                    displayLimit: 20,
                    hiddenProperties: hiddenProperties
                );
            } else {
                sb.AppendNotification("No positive exposures.");
            }
        }
    }
}
