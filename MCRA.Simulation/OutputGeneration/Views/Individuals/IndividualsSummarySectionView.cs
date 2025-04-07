using System.Text;
using MCRA.Simulation.OutputGeneration.Helpers;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class IndividualsSummarySectionView : SectionView<IndividualsSummarySection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            sb.Append("<p class=\"description\">");
            sb.Append($"Individuals");
            sb.Append("</p>");

            if (Model.SelectedPropertyRecords?.Count > 0) {
                sb.AppendTable(
                    Model,
                    Model.SelectedPropertyRecords,
                    "SelectedPropertiesTable",
                    ViewBag,
                    caption: "Selected population properties and levels.",
                    saveCsv: true,
                    header: true
                );
            } else {
                sb.AppendDescriptionParagraph($"No population properties selected (full population)");
            }

            if (Model.Records?.Count > 0) {
                sb.AppendTable(
                    Model,
                    Model.Records,
                    "IndividualsCharacteristicsDataTable",
                    ViewBag,
                    caption: "Individuals statistics.",
                    saveCsv: true,
                    header: true
                );
            }
        }
    }
}
