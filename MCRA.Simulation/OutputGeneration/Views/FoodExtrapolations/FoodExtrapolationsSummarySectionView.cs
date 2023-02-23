using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class FoodExtrapolationsSummarySectionView : SectionView<FoodExtrapolationsSummarySection> {
        public override void RenderSectionHtml(StringBuilder sb) {

            //Render HTML
            if(!Model.Records.Any()) {
                sb.AppendParagraph("No food extrapolations available.");
            } else {
                sb.AppendTable(
                    Model,
                    Model.Records,
                    "FoodExtrapolationsTable",
                    ViewBag,
                    caption: "Food extrapolations.",
                    header: true,
                    saveCsv: true
                );
            }
        }
    }
}
