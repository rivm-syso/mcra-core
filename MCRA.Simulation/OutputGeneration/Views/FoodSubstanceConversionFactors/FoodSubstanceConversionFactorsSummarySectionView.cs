using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class FoodSubstanceConversionFactorsSummarySectionView : SectionView<FoodSubstanceConversionFactorsSummarySection> {
        public override void RenderSectionHtml(StringBuilder sb) {

            //Render HTML
            if(!Model.Records.Any()) {
                sb.AppendParagraph("No food substance conversion factors available.");
            } else {
                sb.AppendTable(
                    Model,
                    Model.Records,
                    "FoodSubstanceConversionFactorsTable",
                    ViewBag,
                    caption: "Food substance conversion factors.",
                    header: true,
                    saveCsv: true
                );
            }
        }
    }
}
