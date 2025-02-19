using System.Text;
using MCRA.Simulation.OutputGeneration.Helpers;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class BaselineBodIndicatorsSummarySectionView : SectionView<BaselineBodIndicatorsSummarySection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            sb.AppendTable(
                Model,
                Model.Records,
                "BaselineBodIndicatorsSummaryTable",
                ViewBag,
                header: true,
                caption: "Baseline BoD indicators.",
                saveCsv: true,
                sortable: true
            );
        }
    }
}
