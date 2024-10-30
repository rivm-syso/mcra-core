using System.Text;
using MCRA.Simulation.OutputGeneration.Helpers;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class EefSummaryTableSectionView : SectionView<EefSummaryTableSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            sb.AppendTable(
                Model,
                Model.Records,
                "ExposureEffectFunctionsSummaryTable",
                ViewBag,
                header: true,
                caption: "Exposure effect functions information.",
                saveCsv: true,
                sortable: true
            );
        }
    }
}
