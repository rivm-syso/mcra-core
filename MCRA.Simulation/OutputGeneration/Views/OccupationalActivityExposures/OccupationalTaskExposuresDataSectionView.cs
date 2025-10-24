using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class OccupationalTaskExposuresDataSectionView : SectionView<OccupationalTaskExposuresDataSection> {

        public override void RenderSectionHtml(StringBuilder sb) {
            var hiddenProperties = new List<string>();

            if (Model.Records.All(c => string.IsNullOrEmpty(c.Reference))) {
                hiddenProperties.Add("Reference");
            }
            if (Model.Records.Any()) {
                // Description
                var totalRecords = Model.Records.Count;
                sb.AppendDescriptionParagraph($"Total {totalRecords} occupational activity exposure records.");

                // Table
                sb.AppendTable(
                    Model,
                    Model.Records,
                    "OccupationalTaskExposuresDataTable",
                    ViewBag,
                    caption: "Occupational task exposures.",
                    saveCsv: true,
                    hiddenProperties: hiddenProperties
                );
            } else {
                sb.AppendDescriptionParagraph("No occupational task exposures available.");
            }
        }
    }
}
