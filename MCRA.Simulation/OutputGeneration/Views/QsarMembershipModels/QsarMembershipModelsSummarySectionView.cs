using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class QsarMembershipModelsSummarySectionView : SectionView<QsarMembershipModelsSummarySection> {
        public override void RenderSectionHtml(StringBuilder sb) {

            //Render HTML
            sb.AppendTable(
               Model,
               Model.Records,
               "QsarMembershipModelsSummaryTable",
               ViewBag,
               caption: "Qsar membership models.",
               saveCsv: true,
               header: true
            );
        }
    }
}
