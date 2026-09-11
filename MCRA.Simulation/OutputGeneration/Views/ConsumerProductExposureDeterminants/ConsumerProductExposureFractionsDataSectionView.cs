using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class ConsumerProductExposureFractionsDataSectionView : SectionView<ConsumerProductExposureFractionsDataSection> {

        public override void RenderSectionHtml(StringBuilder sb) {

            if (Model.Records.Any()) {
                // Description
                var totalRecords = Model.Records.Count;
                sb.AppendDescriptionParagraph($"Total {totalRecords} consumer product exposure fraction records.");

                var hiddenProperties = new List<string>();
                if (Model.Records.All(r => r.ParentName == null)) {
                    hiddenProperties.Add(nameof(ConsumerProductExposureFractionRecord.ParentName));
                }
                if (Model.Records.All(r => r.AgeLower == null)) {
                    hiddenProperties.Add(nameof(ConsumerProductExposureFractionRecord.AgeLower));
                }
                if (Model.Records.All(r => r.Sex == null)) {
                    hiddenProperties.Add(nameof(ConsumerProductExposureFractionRecord.Sex));
                }
                // Table
                sb.AppendTable(
                    Model,
                    Model.Records,
                    "ConsumerProductExposureFractionsDataTable",
                    ViewBag,
                    caption: "Percentage exposure fraction.",
                    saveCsv: true,
                    displayLimit: 20,
                    hiddenProperties: hiddenProperties
                );
            } else {
                sb.AppendDescriptionParagraph("No consumer product exposure fraction data available.");
            }
        }
    }
}
