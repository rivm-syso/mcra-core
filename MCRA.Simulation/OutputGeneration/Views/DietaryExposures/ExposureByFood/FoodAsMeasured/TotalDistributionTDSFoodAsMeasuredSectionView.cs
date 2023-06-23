using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class TotalDistributionTDSFoodAsMeasuredSectionView : SectionView<TotalDistributionTDSFoodAsMeasuredSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var hiddenProperties = new List<string>();
            if (Model.Records.First().Contributions.Count == 0) {
                hiddenProperties.Add("LowerContributionPercentage");
                hiddenProperties.Add("UpperContributionPercentage");
            }

            //Render HTML
            if (Model.Records.Count > 1) {
                var chartCreator = new TotalDistributionTDSFoodAsMeasuredPieChartCreator(Model);
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
                Model.Records.OrderBy(r => r.FoodName, StringComparer.OrdinalIgnoreCase).ThenBy(r => r.TDSFoodName, StringComparer.OrdinalIgnoreCase).ToList(),
                "TotalDistributionTDSFoodAsMeasuredTable",
                ViewBag,
                caption: "Exposure statistics by tds modelled food (total distribution).",
                saveCsv: true,
                hiddenProperties: hiddenProperties
            );
        }
    }
}
