using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class NonDietaryExposureSourcesSummarySectionView : SectionView<NonDietaryExposureSourcesSummarySection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var hiddenProperties = new List<string>();
            if (!Model.Records?.Any(r => !string.IsNullOrEmpty(r.CodeParent)) ?? true) {
                hiddenProperties.Add("CodeParent");
            }

            // Description paragraph
            sb.AppendDescriptionParagraph($"Number of records: { Model.Records?.Count ?? 0}");

            // Create table
            if (Model.Records?.Count > 0) {
                sb.AppendTable(
                    Model,
                    Model.Records,
                    "NonDietaryExposureSourcesTable",
                    ViewBag,
                    header: true,
                    caption: "Non-dietary exposure sources",
                    saveCsv: true,
                    sortable: true,
                    hiddenProperties: hiddenProperties
                );
            }
        }
    }
}
