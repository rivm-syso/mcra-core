using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class OccupationalTaskExposureModelsSectionView : SectionView<OccupationalTaskExposureModelsSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            if (Model.Records.Any()) {
                var hiddenProperties = new List<string>();
                if (Model.Records.All(c => string.IsNullOrEmpty(c.RpeType))) {
                    hiddenProperties.Add(nameof(OccupationalTaskExposureModelRecord.RpeType));
                }
                if (Model.Records.All(c => string.IsNullOrEmpty(c.HandProtectionType))) {
                    hiddenProperties.Add(nameof(OccupationalTaskExposureModelRecord.HandProtectionType));
                }
                if (Model.Records.All(c => string.IsNullOrEmpty(c.ProtectiveClothingType))) {
                    hiddenProperties.Add(nameof(OccupationalTaskExposureModelRecord.ProtectiveClothingType));
                }

                sb.AppendDescriptionParagraph($"Total {Model.Records.Count} records.");
                sb.AppendTable(
                    Model,
                    Model.Records,
                    "OccupationalTaskExposureModelsTable",
                    ViewBag,
                    hiddenProperties: hiddenProperties,
                    caption: "Occupational task exposure models.",
                    saveCsv: true
                );
            } else {
                sb.AppendDescriptionParagraph("No occupational task exposure models available.");
            }
        }
    }
}
