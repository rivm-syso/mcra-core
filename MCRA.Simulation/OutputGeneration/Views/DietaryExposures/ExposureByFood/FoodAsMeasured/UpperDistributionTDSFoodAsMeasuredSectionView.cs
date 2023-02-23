using MCRA.Simulation.OutputGeneration.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class UpperDistributionTDSFoodAsMeasuredSectionView : SectionView<UpperDistributionTDSFoodAsMeasuredSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var hiddenProperties = new List<string>();
            if (Model.UpperDistributionTDSFoodAsMeasuredRecords.Count > 0 && Model.UpperDistributionTDSFoodAsMeasuredRecords.First().Contributions.Count == 0) {
                hiddenProperties.Add("LowerContributionPercentage");
                hiddenProperties.Add("UpperContributionPercentage");
            }

            //Render HTML
            if (Model.UpperDistributionTDSFoodAsMeasuredRecords.Count > 1) {
                sb.AppendParagraph($"Exposure: upper percentage {Model.UpperPercentage:F2} % ({Model.NRecords} records), " +
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
            sb.AppendDescriptionParagraph($"Number of TDS modelled foods: {Model.UpperDistributionTDSFoodAsMeasuredRecords.Count}");
            sb.AppendTable(
                Model,
                Model.UpperDistributionTDSFoodAsMeasuredRecords.OrderBy(r => r.FoodName, StringComparer.OrdinalIgnoreCase).ThenBy(r => r.TDSFoodName, StringComparer.OrdinalIgnoreCase).ToList(),
                "UpperDistributionTDSFoodAsMeasuredTable",
                ViewBag, 
                caption: "Exposure statistics by tds modelled food (upper tail distribution).",
                saveCsv: true,
                hiddenProperties: hiddenProperties
             );
        }
    }
}
