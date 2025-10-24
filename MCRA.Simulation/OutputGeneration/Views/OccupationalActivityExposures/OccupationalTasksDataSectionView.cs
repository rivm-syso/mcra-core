using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class OccupationalTasksDataSectionView : SectionView<OccupationalTasksDataSection> {

        public override void RenderSectionHtml(StringBuilder sb) {
            var hiddenProperties = new List<string>();
            if (Model.Records.All(c => string.IsNullOrEmpty(c.Description))) {
                hiddenProperties.Add("Description");
            }

            if (Model.Records.Any()) {
                // Description
                var totalRecords = Model.Records.Count;
                sb.AppendDescriptionParagraph($"Total {totalRecords} occupational activity records.");

                // Table
                sb.AppendTable(
                    Model,
                    Model.Records,
                    "OccupationalTasksDataTable",
                    ViewBag,
                    caption: "Occupational tasks.",
                    saveCsv: true,
                    hiddenProperties: hiddenProperties
                );
            } else {
                sb.AppendDescriptionParagraph("No Occupational tasks available.");
            }
        }
    }
}
