using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class MolecularDockingModelsSummarySectionView : SectionView<MolecularDockingModelsSummarySection> {
        public override void RenderSectionHtml(StringBuilder sb) {

            //Render HTML
            sb.AppendTable(
               Model,
               Model.Records,
               "MolecularDockingModelsSummaryTable",
               ViewBag,
               caption: "Molecular docking models.",
               saveCsv: true,
               header: true
            );
        }
    }
}
