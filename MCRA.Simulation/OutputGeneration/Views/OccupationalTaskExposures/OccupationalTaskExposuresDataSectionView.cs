using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class OccupationalTaskExposuresDataSectionView : SectionView<OccupationalTaskExposuresDataSection> {

        public override void RenderSectionHtml(StringBuilder sb) {
            if (Model.Records.Any()) {
                var totalRecords = Model.Records.Count;
                sb.AppendDescriptionParagraph($"Total {totalRecords} occupational activity exposure records.");

                var hiddenProperties = new List<string>();
                if (Model.Records.All(c => string.IsNullOrEmpty(c.Reference))) {
                    hiddenProperties.Add(nameof(OccupationalTaskExposuresDataRecord.Reference));
                }
                if (Model.Records.All(c => string.IsNullOrEmpty(c.RpeType))) {
                    hiddenProperties.Add(nameof(OccupationalTaskExposuresDataRecord.RpeType));
                }
                if (Model.Records.All(c => string.IsNullOrEmpty(c.HandProtectionType))) {
                    hiddenProperties.Add(nameof(OccupationalTaskExposuresDataRecord.HandProtectionType));
                }
                if (Model.Records.All(c => string.IsNullOrEmpty(c.ProtectiveClothingType))) {
                    hiddenProperties.Add(nameof(OccupationalTaskExposuresDataRecord.ProtectiveClothingType));
                }

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
