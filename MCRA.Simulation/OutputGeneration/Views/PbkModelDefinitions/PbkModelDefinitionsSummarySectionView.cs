using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class PbkModelDefinitionsSummarySectionView : SectionView<PbkModelDefinitionsSummarySection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            if (Model.Records.Any()) {
                var hiddenProperties = new List<string>();
                if (Model.Records.All(r => string.IsNullOrEmpty(r.Description))) {
                    hiddenProperties.Add("Description");
                }
                sb.AppendTable(
                   Model,
                   Model.Records,
                   "PbkModelDefinitionsTable",
                   ViewBag,
                   caption: "Linked (SBML) PBK models.",
                   hiddenProperties: hiddenProperties,
                   saveCsv: true
                );
            } else {
                sb.AppendDescriptionParagraph("No additional (SBML) PBK models were linked. Only the embedded PBK models of MCRA were used.");
            }
        }
    }
}
