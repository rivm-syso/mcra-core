using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class OccupationalScenarioTasksDataSectionView : SectionView<OccupationalScenarioTasksDataSection> {

        public override void RenderSectionHtml(StringBuilder sb) {
            if (Model.Records.Any()) {
                var totalRecords = Model.Records.Count;
                sb.AppendDescriptionParagraph($"Total {totalRecords} occupational scenario activity records.");

                var hiddenProperties = new List<string>();
                if (Model.Records.All(c => string.IsNullOrEmpty(c.RpeType))) {
                    hiddenProperties.Add(nameof(OccupationalScenarioTasksDataRecord.RpeType));
                }
                if (Model.Records.All(c => string.IsNullOrEmpty(c.HandProtectionType))) {
                    hiddenProperties.Add(nameof(OccupationalScenarioTasksDataRecord.HandProtectionType));
                }
                if (Model.Records.All(c => string.IsNullOrEmpty(c.ProtectiveClothingType))) {
                    hiddenProperties.Add(nameof(OccupationalScenarioTasksDataRecord.ProtectiveClothingType));
                }

                sb.AppendTable(
                    Model,
                    Model.Records,
                    "OccupationalScenarioTasksDataTable",
                    ViewBag,
                    caption: "Occupational scenario tasks.",
                    hiddenProperties: hiddenProperties,
                    saveCsv: true
                );
            } else {
                sb.AppendDescriptionParagraph("No Occupational scenario tasks available.");
            }
        }
    }
}
