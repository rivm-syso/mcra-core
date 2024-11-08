using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class ChronicSingleValueEstimatesSectionView : SectionView<ChronicSingleValueEstimatesSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            if (Model.Records?.Count > 0) {
                var hiddenProperties = new List<string>();
                sb.AppendDescriptionParagraph($"Number of records: {Model.Records.Count}.");
                sb.AppendTable(
                    Model,
                    Model.Records,
                    "SingleValueEstimatesTable",
                    ViewBag,
                    header: true,
                    caption: "Chronic estimates summary.",
                    saveCsv: true,
                    hiddenProperties: hiddenProperties
                );
            } else {
                sb.AppendParagraph("Error: failed to compute single value exposures .", "warning");
            }
        }
    }
}
