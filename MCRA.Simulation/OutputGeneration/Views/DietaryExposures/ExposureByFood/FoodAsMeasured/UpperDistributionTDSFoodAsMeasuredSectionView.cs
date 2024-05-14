using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class UpperDistributionTDSFoodAsMeasuredSectionView : SectionView<UpperDistributionTDSFoodAsMeasuredSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var hiddenProperties = new List<string>();
            if (Model.Records.Count > 0 && Model.Records.First().Contributions.Count == 0) {
                hiddenProperties.Add("LowerContributionPercentage");
                hiddenProperties.Add("UpperContributionPercentage");
            }

            //Render HTML
            if (Model.Records.Count > 1) {
                sb.AppendParagraph($"Exposure: upper tail {Model.CalculatedUpperPercentage:F1}% ({Model.NRecords} records), " +
                    $"minimum value {Model.LowPercentileValue:G4} {ViewBag.GetUnit("IntakeUnit")}, " +
                    $"maximum value {Model.HighPercentileValue:G4} {ViewBag.GetUnit("IntakeUnit")}");
                var chartCreator = new UpperDistributionTDSFoodAsMeasuredPieChartCreator(Model);
                sb.AppendChart(
                    "TotalDistributionTDSFoodAsMeasuredChart",
                    chartCreator,
                    ChartFileType.Svg,
                    Model,
                    ViewBag,
                    chartCreator.Title,
                    true
                );
            } else {
                sb.AppendParagraph("Only TDS compositions available");
            }
            sb.AppendDescriptionParagraph($"Number of TDS modelled foods: {Model.Records.Count}");
            sb.AppendTable(
                Model,
                Model.Records
                    .OrderBy(r => r.FoodName, StringComparer.OrdinalIgnoreCase)
                    .ThenBy(r => r.FoodCode, StringComparer.OrdinalIgnoreCase)
                    .ThenBy(r => r.TDSFoodName, StringComparer.OrdinalIgnoreCase)
                    .ThenBy(r => r.TDSFoodCode, StringComparer.OrdinalIgnoreCase)
                    .ToList(),
                "UpperDistributionTDSFoodAsMeasuredTable",
                ViewBag, 
                caption: $"Exposure statistics of TDS modelled foods to the upper tail of the distribution (estimated {Model.CalculatedUpperPercentage:F1}%).",
                saveCsv: true,
                hiddenProperties: hiddenProperties
             );
        }
    }
}
