using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class OccupationalScenarioTasksDataSectionView : SectionView<OccupationalScenarioTasksDataSection> {

        public override void RenderSectionHtml(StringBuilder sb) {

            if (Model.Records.Any()) {
                // Description
                var totalRecords = Model.Records.Count;
                sb.AppendDescriptionParagraph($"Total {totalRecords} occupational scenario activity records.");

                // Table
                sb.AppendTable(
                    Model,
                    Model.Records,
                    "OccupationalScenarioTasksDataTable",
                    ViewBag,
                    caption: "Occupational scenario tasks.",
                    saveCsv: true
                );
            } else {
                sb.AppendDescriptionParagraph("No Occupational scenario tasks available.");
            }
        }
    }
}
