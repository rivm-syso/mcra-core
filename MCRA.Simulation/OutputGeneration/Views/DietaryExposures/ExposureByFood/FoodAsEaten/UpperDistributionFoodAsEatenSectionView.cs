using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class UpperDistributionFoodAsEatenSectionView : SectionView<UpperDistributionFoodAsEatenSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var hiddenProperties = new List<string>();
            if (Model.Records.All(c => c.NumberOfSubstances <= 1)) {
                hiddenProperties.Add("NumberOfSubstances");
            }
            var result = new List<DistributionFoodRecord>();
            var isUncertainty = Model.Records.Any() && Model.Records.First().Contributions.Any();
            if (!isUncertainty) {
                hiddenProperties.Add("LowerContributionPercentage");
                hiddenProperties.Add("UpperContributionPercentage");
                hiddenProperties.Add("MeanContribution");
                result = Model.Records.Where(c => c.Contribution > 0).ToList();
            } else {
                result = Model.Records.Where(c => c.Contribution > 0 || c.MeanContribution > 0).ToList();
                hiddenProperties.Add("ContributionPercentage");
            }
            //Render HTML
            if (Model.Records.Sum(r => r.Total) > 0) {
                if (Model.HasOthers) {
                    sb.AppendParagraph("In this table, each row only summarizes risk driver components selected in the screening", "note");
                }
                sb.Append($"Exposure: upper tail {Model.CalculatedUpperPercentage:F1}% ({Model.NRecords} records), " +
                                   $"minimum value {Model.LowPercentileValue:G4} {ViewBag.GetUnit("IntakeUnit")}, " +
                                   $"maximum value {Model.HighPercentileValue:G4} {ViewBag.GetUnit("IntakeUnit")}");
                if (result.Count > 1) {
                    var chartCreator = new UpperDistributionFoodAsEatenPieChartCreator(Model, isUncertainty);
                    sb.AppendChart(
                        "UpperDistributionFoodAsEatenChart",
                        chartCreator,
                        ChartFileType.Svg,
                        Model,
                        ViewBag,
                        chartCreator.Title,
                        true
                    );
                }
            } else {
                sb.AppendNotification("No positive exposures.");
            }
            sb.AppendDescriptionParagraph($"Number of foods as eaten: {result.Count}");
            sb.AppendTable(
               Model,
               result.OrderBy(r => r.FoodName, StringComparer.OrdinalIgnoreCase)
                     .ThenBy(r => r.FoodCode, StringComparer.OrdinalIgnoreCase)
                     .ToList(),
               "UpperDistributionFoodAsEatenTable",
               ViewBag,
               caption: $"Exposure statistics by foods as eaten for the upper tail of the distribution (estimated {Model.CalculatedUpperPercentage:F1}%).",
               saveCsv: true,
               header: true,
               hiddenProperties: hiddenProperties
            );
        }
    }
}
