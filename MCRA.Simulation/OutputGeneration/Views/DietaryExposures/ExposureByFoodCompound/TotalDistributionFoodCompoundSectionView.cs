using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class TotalDistributionFoodCompoundSectionView : SectionView<TotalDistributionFoodCompoundSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var hiddenProperties = new List<string>();

            if (Model.Records.All(c => double.IsNaN(c.Contribution))) {
                hiddenProperties.Add("Contribution");
                hiddenProperties.Add("MeanContribution");
                hiddenProperties.Add("ContributionPercentage");
            }
            if (Model.Records.All(c => c.CompoundCode == null)) {
                hiddenProperties.Add("CompoundCode");
            }
            if (Model.Records.All(c => c.FoodCode == null)) {
                hiddenProperties.Add("FoodCode");
            }
            var tableRecords = new List<DistributionFoodCompoundRecord>();
            var isUncertainty = Model.Records.FirstOrDefault()?.Contributions.Any() ?? false;
            if (!isUncertainty) {
                hiddenProperties.Add("LowerContributionPercentage");
                hiddenProperties.Add("UpperContributionPercentage");
                hiddenProperties.Add("MeanContribution");
                tableRecords = Model.Records
                    .Where(c => double.IsNaN(c.Contribution) || c.Contribution > 0)
                    .OrderByDescending(r => r.Contribution)
                    .ThenBy(r => r.FoodName, StringComparer.OrdinalIgnoreCase)
                    .ThenBy(r => r.FoodCode, StringComparer.OrdinalIgnoreCase)
                    .ThenBy(r => r.CompoundName, StringComparer.OrdinalIgnoreCase)
                    .ThenBy(r => r.CompoundCode, StringComparer.OrdinalIgnoreCase)
                    .ToList();
            } else {
                tableRecords = Model.Records
                    .Where(c => double.IsNaN(c.Contribution) || c.Contribution > 0 || c.MeanContribution > 0)
                    .OrderByDescending(r => r.MeanContribution)
                    .ThenBy(r => r.FoodName, StringComparer.OrdinalIgnoreCase)
                    .ThenBy(r => r.FoodCode, StringComparer.OrdinalIgnoreCase)
                    .ThenBy(r => r.CompoundName, StringComparer.OrdinalIgnoreCase)
                    .ThenBy(r => r.CompoundCode, StringComparer.OrdinalIgnoreCase)
                    .ToList();
                hiddenProperties.Add("ContributionPercentage");
            }

            //Render HTML
            if (Model.Records.Any()) {
                sb.AppendDescriptionParagraph($"Total {tableRecords.Count} combinations of food and substance.");
                if (Model.Records.Any(r => !double.IsNaN(r.Contribution) && (r.Contribution > 0 || r.MeanContribution > 0))) {
                    var chartCreator = new TotalDistributionFoodCompoundPieChartCreator(Model, isUncertainty);
                    sb.AppendChart(
                        "TotalDistributionFoodSubstanceChart",
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
                    tableRecords,
                    "TotalDistributionFoodCompoundTable",
                    ViewBag,
                    caption: "Exposure statistics by food x substance (total distribution), RPFs are not applied except for exposure contribution.",
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
