using System.Text;
using MCRA.Simulation.OutputGeneration.Helpers;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class BurdensOfDiseaseSummarySectionView : SectionView<BurdensOfDiseaseSummarySection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var hiddenProperties = new List<string>();
            if (Model.Records.All(r => string.IsNullOrEmpty(r.BodUncertaintyDistribution))) {
                hiddenProperties.Add(nameof(BurdensOfDiseaseSummaryRecord.BodUncertaintyDistribution));
                hiddenProperties.Add(nameof(BurdensOfDiseaseSummaryRecord.UncLower));
                hiddenProperties.Add(nameof(BurdensOfDiseaseSummaryRecord.UncUpper));
            }
            if (Model.Records.All(r => r.UncLower == null)) {
                hiddenProperties.Add(nameof(BurdensOfDiseaseSummaryRecord.UncLower));
            }
            if (Model.Records.All(r => r.UncUpper == null)) {
                hiddenProperties.Add(nameof(BurdensOfDiseaseSummaryRecord.UncUpper));
            }
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
