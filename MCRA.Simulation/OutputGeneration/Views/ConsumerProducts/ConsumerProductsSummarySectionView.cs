using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class ConsumerProductsSummarySectionView : SectionView<ConsumerProductsSummarySection> {
        public override void RenderSectionHtml(StringBuilder sb) {

            // Description
            var description = $"The scope contains {Model.Records.Count} consumer products.";
            sb.AppendDescriptionParagraph(description);

            // Table
            var hiddenProperties = new List<string>();
            if (Model.Records.All(c => string.IsNullOrEmpty(c.Description))) {
                hiddenProperties.Add("Description");
            }
            sb.AppendTable(
               Model,
               Model.Records,
               "ConsumerProductsTable",
               ViewBag,
               caption: "Consumer products.",
               saveCsv: true,
               header: true,
               sortable: true,
               displayLimit: 20,
               hiddenProperties: hiddenProperties
            );
        }
    }
}
