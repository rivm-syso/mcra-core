using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class OccupationalScenarioTasksExposuresSectionView : SectionView<OccupationalScenarioTasksExposuresSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            if (Model.Records.Any()) {

                var hiddenProperties = new List<string>();
                if (Model.Records.All(r => string.IsNullOrEmpty(r.EstimateType))) {
                    hiddenProperties.Add(nameof(OccupationalScenarioTaskExposureRecord.EstimateType));
                }
                if (Model.Records.All(c => string.IsNullOrEmpty(c.RpeType))) {
                    hiddenProperties.Add(nameof(OccupationalScenarioTaskExposureRecord.RpeType));
                }
                if (Model.Records.All(c => string.IsNullOrEmpty(c.HandProtectionType))) {
                    hiddenProperties.Add(nameof(OccupationalScenarioTaskExposureRecord.HandProtectionType));
                }
                if (Model.Records.All(c => string.IsNullOrEmpty(c.ProtectiveClothingType))) {
                    hiddenProperties.Add(nameof(OccupationalScenarioTaskExposureRecord.ProtectiveClothingType));
                }

                sb.AppendDescriptionParagraph($"Total {Model.Records.Count} records.");
                sb.AppendTable(
                    Model,
                    Model.Records,
                    "OccupationalScenarioTaskExposuresTable",
                    ViewBag,
                    caption: "Occupational scenario task exposures.",
                    saveCsv: true,
                    hiddenProperties: hiddenProperties
                );
            } else {
                sb.AppendDescriptionParagraph("No occupational scenario exposures available.");
            }
        }
    }
}
