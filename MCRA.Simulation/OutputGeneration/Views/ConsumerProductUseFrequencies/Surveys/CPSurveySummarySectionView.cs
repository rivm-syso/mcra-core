using System.Text;
using MCRA.Simulation.OutputGeneration.Helpers;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class CPSurveySummarySectionView : SectionView<CPSurveySummarySection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            {
                var hiddenProperties = new List<string>();
                if (string.IsNullOrEmpty(Model.Record.Description)) {
                    hiddenProperties.Add("Description");
                }
                _ = sb.AppendTable(
                    Model,
                    [Model.Record],
                    "ConsumerProductSurveySummaryTable",
                    ViewBag,
                    hiddenProperties: hiddenProperties,
                    caption: "Consumer product study summary.",
                    saveCsv: true,
                    header: true
                );
            }
        }
    }
}
