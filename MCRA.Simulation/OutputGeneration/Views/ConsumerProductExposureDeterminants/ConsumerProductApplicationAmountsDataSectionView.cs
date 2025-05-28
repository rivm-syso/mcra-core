using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class ConsumerProductApplicationAmountsDataSectionView : SectionView<ConsumerProductApplicationAmountsDataSection> {

        public override void RenderSectionHtml(StringBuilder sb) {

            if (Model.Records.Any()) {
                // Description
                var totalRecords = Model.Records.Count;
                sb.AppendDescriptionParagraph($"Total {totalRecords} consumer product application amount records.");

                var hiddenProperties = new List<string>();
                if (Model.Records.All(c => string.IsNullOrEmpty(c.DistributionType))) {
                    hiddenProperties.Add("DistributionType");
                    hiddenProperties.Add("CvVariability");
                }
                if (Model.Records.All(c => string.IsNullOrEmpty(c.Sex))) {
                    hiddenProperties.Add("Sex");
                }
                if (Model.Records.All(c => c.AgeLower == null)) {
                    hiddenProperties.Add("AgeLower");
                }

                // Table
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
