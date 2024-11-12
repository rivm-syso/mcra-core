using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class PbkModelDefinitionsSummarySectionView : SectionView<PbkModelDefinitionsSummarySection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            //Render HTML
            sb.AppendTable(
               Model,
               Model.Records,
               "PbkModelDefinitionsTable",
               ViewBag,
               caption: "PBK model definitions.",
               saveCsv: true,
               header: true
            );
        }
    }
}
