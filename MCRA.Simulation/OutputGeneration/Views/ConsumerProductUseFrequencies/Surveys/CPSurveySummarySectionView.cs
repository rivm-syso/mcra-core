using System.Text;
using MCRA.Simulation.OutputGeneration.Helpers;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class CPSurveySummarySectionView : SectionView<CPSurveySummarySection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            {
                var hiddenProperties = new List<string>();
                if (Model.Records.All(r => string.IsNullOrEmpty(r.Description))) {
                    hiddenProperties.Add(nameof(CPSurveySummaryRecord.Description));
                }
                if (Model.Records.All(r => string.IsNullOrEmpty(r.StartDate))) {
                    hiddenProperties.Add(nameof(CPSurveySummaryRecord.StartDate));
                }
                if (Model.Records.All(r => string.IsNullOrEmpty(r.EndDate))) {
                    hiddenProperties.Add(nameof(CPSurveySummaryRecord.EndDate));
                }
                sb.AppendTable(
                    Model,
                    Model.Records,
                    "ConsumerProductSurveySummaryTable",
                    ViewBag,
                    hiddenProperties: hiddenProperties,
                    caption: "Consumer product use frequency survey summary.",
                    saveCsv: true,
                    header: true
                );
            }
        }
    }
}
