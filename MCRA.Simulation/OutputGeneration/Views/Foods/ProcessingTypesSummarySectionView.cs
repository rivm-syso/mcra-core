using MCRA.Simulation.OutputGeneration.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class ProcessingTypesSummarySectionView : SectionView<ProcessingTypesSummarySection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var hiddenProperties = new List<string>();

            //Render HTML
            sb.AppendDescriptionParagraph($"Number of records: {Model.Records?.Count ?? 0}");

            if (Model.Records?.Any() ?? false) {
                sb.AppendTable(
                   Model,
                   Model.Records,
                   "ProcessingTypesTable",
                   ViewBag,
                   caption: "Processing type names and codes, bulking/blending information and distribution types.",
                   saveCsv: true,
                   header: true,
                   hiddenProperties: hiddenProperties
                );
            }
        }
    }
}
