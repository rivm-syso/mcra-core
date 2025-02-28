using System.Text;
using MCRA.Simulation.OutputGeneration.Helpers;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class EnvironmentalBurdenOfDiseaseSummarySectionView : SectionView<EnvironmentalBurdenOfDiseaseSummarySection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            sb.AppendTable(
                Model,
                new List<EnvironmentalBurdenOfDiseaseSummaryRecord>() { Model.Record },
                "EnvironmentalBurdenOfDiseaseSummaryTable",
                ViewBag,
                caption: "Environmental burden of disease summary table.",
                saveCsv: true,
                sortable: false,
                rotate: true
            );
        }
    }
}
