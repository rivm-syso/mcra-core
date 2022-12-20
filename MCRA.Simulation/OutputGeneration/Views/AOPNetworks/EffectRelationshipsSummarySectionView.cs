using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class EffectRelationshipsSummarySectionView : SectionView<EffectRelationshipsSummarySection> {
        public override void RenderSectionHtml(StringBuilder sb) {

            //Render HTML
            sb.AppendTable(
                Model,
                Model.Records,
                "EffectRelationshipsSectionTable",
                ViewBag,
                caption: "Effect relationships.",
                header: true,
                saveCsv: true
            );
        }
    }
}
