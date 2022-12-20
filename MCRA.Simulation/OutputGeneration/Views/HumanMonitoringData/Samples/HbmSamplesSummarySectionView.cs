using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class HbmSamplesSummarySectionView : SectionView<HbmSamplesSummarySection> {
        public override void RenderSectionHtml(StringBuilder sb) {

            //Render HTML
            sb.AppendTable(
                Model,
                Model.Records,
                "HumanMonitoringSamplesSummaryTable",
                ViewBag,
                caption: "Human monitoring samples summary.",
                saveCsv: true,
                header: true
            );
        }
    }
}
