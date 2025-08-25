using System.Text;
using MCRA.Simulation.OutputGeneration.Helpers;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class BurdensOfDiseaseSummarySectionView : SectionView<BurdensOfDiseaseSummarySection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var hiddenProperties = new List<string>();
            sb.AppendTable(
                Model,
                Model.Records,
                "BurdensOfDiseaseSummaryTable",
                ViewBag,
                header: true,
                caption: "Burden of disease indicators.",
                saveCsv: true,
                sortable: true,
                hiddenProperties: hiddenProperties
            );
        }
    }
}
