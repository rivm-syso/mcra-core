using System.Text;
using MCRA.Simulation.OutputGeneration.Helpers;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class EnvironmentalBurdenOfDiseaseSummarySectionView : SectionView<EnvironmentalBurdenOfDiseaseSummarySection> {
        public override void RenderSectionHtml(StringBuilder sb) {

            var hiddenProperties = new List<string>();
            if (Model.Records.All(c => double.IsInfinity(c.StandardizedTotalAttributableBod))) {
                hiddenProperties.Add("StandardizedTotalAttributableBod");
            }
            sb.AppendTable(
                Model,
                Model.Records,
                "EnvironmentalBurdenOfDiseaseSummaryTable",
                ViewBag,
                caption: "Environmental burden of disease summary table.",
                saveCsv: true,
                sortable: false,
                hiddenProperties: hiddenProperties
            );
        }
    }
}
