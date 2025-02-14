using System.Text;
using MCRA.Simulation.OutputGeneration.Helpers;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class AttributableBodSummarySectionView : SectionView<AttributableBodSummarySection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            sb.AppendTable(
                Model,
                Model.Records,
                "AttributableBodTable",
                ViewBag,
                header: true,
                caption: "Attributable burden of disease.",
                saveCsv: true,
                sortable: true
            );
        }
    }
}
