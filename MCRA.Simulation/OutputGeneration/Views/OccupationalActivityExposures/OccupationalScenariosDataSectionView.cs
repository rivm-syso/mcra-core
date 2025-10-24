using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class OccupationalScenariosDataSectionView : SectionView<OccupationalScenariosDataSection> {

        public override void RenderSectionHtml(StringBuilder sb) {
            var hiddenProperties = new List<string>();
            if (Model.Records.All(c => string.IsNullOrEmpty(c.Description))) {
                hiddenProperties.Add("Description");
            }
            
            if (Model.Records.Any()) {
                // Description
                sb.AppendDescriptionParagraph($"Total {Model.Records.Count} occupational scenario records.");

                // Table
                sb.AppendTable(
                    Model,
                    Model.Records,
                    "OccupationalScenariosDataTable",
                    ViewBag,
                    caption: "Occupational scenarios.",
                    saveCsv: true,
                    hiddenProperties: hiddenProperties
                );
            } else {
                sb.AppendDescriptionParagraph("No occupational scenarios available.");
            }
        }
    }
}
