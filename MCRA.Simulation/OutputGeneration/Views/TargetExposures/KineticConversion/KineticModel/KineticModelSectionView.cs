using System.Text;
using MCRA.Simulation.OutputGeneration.Helpers;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class KineticModelSectionView : SectionView<KineticModelSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            sb.AppendTable(
               Model,
               Model.KineticModelRecords,
               "KineticModelsSummaryRecordTable",
               ViewBag,
               caption: "Kinetic model summary.",
               saveCsv: true,
               header: true,
               rotate: true
            );
        }
    }
}
