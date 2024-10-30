using System.Text;
using MCRA.Simulation.OutputGeneration.Helpers;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class AttributableEbdSummarySectionView : SectionView<AttributableEbdSummarySection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            sb.AppendTable(
                Model,
                Model.Records,
                "EffectsTable",
                ViewBag,
                header: true,
                caption: "Attributable Environmental Burden of Disease.",
                saveCsv: true,
                sortable: true
            );
        }
    }
}
