using System.Text;
using MCRA.Simulation.OutputGeneration.Helpers;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class PbkModelsSummarySectionView : SectionView<PbkModelsSummarySection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            sb.AppendTable(
               Model,
               Model.Records,
               "PbkModelInstancesTable",
               ViewBag,
               caption: "PBK model instances.",
               saveCsv: true,
               header: true
            );
        }
    }
}
