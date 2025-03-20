using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class AirIndoorFractionsDataSectionView : SectionView<AirIndoorFractionsDataSection> {

        public override void RenderSectionHtml(StringBuilder sb) {

            if (Model.Records.Any()) {
                // Description
                var totalRecords = Model.Records.Count;
                sb.AppendDescriptionParagraph($"Total {totalRecords} air indoor fraction records.");

                var hiddenProperties = new List<string>();
                if (Model.Records.All(c => c.AgeLower == null)) {
                    hiddenProperties.Add("idSubgroup");
                }

                // Table
                sb.AppendTable(
                    Model,
                    Model.Records,
                    "AirIndoorFractionsDataTable",
                    ViewBag,
                    caption: "Percentage indoor fraction.",
                    saveCsv: true,
                    hiddenProperties: hiddenProperties
                );
            } else {
                sb.AppendDescriptionParagraph("No indoor air fraction data available.");
            }
        }
    }
}
