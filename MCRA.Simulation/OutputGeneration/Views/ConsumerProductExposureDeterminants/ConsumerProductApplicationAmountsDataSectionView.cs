using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class ConsumerProductApplicationAmountsDataSectionView : SectionView<ConsumerProductApplicationAmountsDataSection> {

        public override void RenderSectionHtml(StringBuilder sb) {
            if (Model.Records.Count > 0) {
                var totalRecords = Model.Records.Count;
                sb.AppendDescriptionParagraph($"Total {totalRecords} consumer product application amount records.");

                var hiddenProperties = new List<string>();
                if (!Model.Records.Any(c => c.AgeLower.HasValue)){
                    hiddenProperties.Add("AgeLower");
                }
                if (Model.Records.All(c => string.IsNullOrEmpty(c.ParentName))) {
                    hiddenProperties.Add("ParentName");
                }
                if (Model.Records.All(c => string.IsNullOrEmpty(c.Sex))) {
                    hiddenProperties.Add("Sex");
                }
                if (Model.Records.All(c => string.IsNullOrEmpty(c.DistributionType))) {
                    hiddenProperties.Add("DistributionType");
                    hiddenProperties.Add("CvVariability");
                }
                sb.AppendTable(
                    Model,
                    Model.Records,
                    "ConsumerProductApplicationAmountsDataTable",
                    ViewBag,
                    caption: "Consumer product application amounts.",
                    saveCsv: true,
                    hiddenProperties: hiddenProperties
                );
            } else {
                sb.AppendDescriptionParagraph("No consumer product application amounts data available.");
            }
        }
    }
}
