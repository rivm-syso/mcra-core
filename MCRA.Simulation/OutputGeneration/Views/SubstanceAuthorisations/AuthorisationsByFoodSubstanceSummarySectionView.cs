using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class AuthorisationsByFoodSubstanceSummarySectionView : SectionView<AuthorisationsByFoodSubstanceSummarySection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            //Render HTML
            if (Model.Records.Any()) {
                sb.AppendTable(
                    Model,
                    Model.Records,
                    "SubstanceAuthorisationsDataTable",
                    ViewBag,
                    caption: "Substance authorisations.",
                    header: true,
                    saveCsv: true
                );
            } else {
                sb.AppendParagraph("No substance authorisations found for any combination of measured food and substance.", "warning");
            }
        }
    }
}
