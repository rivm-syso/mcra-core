using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class ConsumerProductExposureSetSummarySectionView : SectionView<ConsumerProductExposureSetSummarySection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var hiddenProperties = new List<string>();
            if (Model.Records.All(c => string.IsNullOrEmpty(c.Minimum))) {
                hiddenProperties.Add(nameof(ConsumerProductExposureSetRecord.Minimum));
                hiddenProperties.Add(nameof(ConsumerProductExposureSetRecord.Maximum));
            }

            //Render HTML
            sb.AppendTable(
               Model,
               Model.Records,
               "ConsumerProductExposureSetTable",
               ViewBag,
               caption: "Consumer product exposure set summary.",
               saveCsv: true,
               header: true,
               displayLimit: 20,
               hiddenProperties: hiddenProperties
            );
        }
    }
}
