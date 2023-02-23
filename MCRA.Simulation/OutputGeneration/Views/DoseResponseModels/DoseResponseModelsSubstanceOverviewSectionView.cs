using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class DoseResponseModelsSubstanceOverviewSectionView : SectionView<DoseResponseModelsSubstanceOverviewSection> {
        public override void RenderSectionHtml(StringBuilder sb) {

            //Render HTML
            sb.AppendTable(
                Model,
                Model.SummaryRecords,
                "DoseResponseModelsSubstanceOverviewSectionTable",
                ViewBag,
                header: true,
                saveCsv: true
            );
        }
    }
}
