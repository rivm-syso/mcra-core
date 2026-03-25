using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class DustAvailabilityFractionsDataSectionView : SectionView<DustAvailabilityFractionsDataSection> {

        public override void RenderSectionHtml(StringBuilder sb) {

            if (Model.Records.Any()) {
                // Description
                var totalRecords = Model.Records.Count;
                sb.AppendDescriptionParagraph($"Total {totalRecords} dust availability fraction records.");

                var hiddenProperties = new List<string>();
                if (Model.Records.All(c => !c.AgeLower.HasValue) &&
                    Model.Records.All(c => string.IsNullOrEmpty(c.Sex))) {
                    hiddenProperties.Add(nameof(DustAvailabilityFractionsDataRecord.idSubgroup));
                }
                if (Model.Records.All(c => string.IsNullOrEmpty(c.DistributionType))) {
                    hiddenProperties.Add(nameof(DustAvailabilityFractionsDataRecord.DistributionType));
                    hiddenProperties.Add(nameof(DustAvailabilityFractionsDataRecord.CvVariability));
                }
                if (Model.Records.All(c => !c.AgeLower.HasValue)) {
                    hiddenProperties.Add(nameof(DustAvailabilityFractionsDataRecord.AgeLower));
                }
                if (Model.Records.All(c => string.IsNullOrEmpty(c.Sex))) {
                    hiddenProperties.Add(nameof(DustAvailabilityFractionsDataRecord.Sex));
                }

                // Table
                sb.AppendTable(
                    Model,
                    Model.Records,
                    "DustAvailabilityFractionDataTable",
                    ViewBag,
                    caption: "Fraction of dust available for dermal contact.",
                    saveCsv: true,
                    hiddenProperties: hiddenProperties
                );
            } else {
                sb.AppendDescriptionParagraph("No dust availability fraction data available.");
            }
        }

    }
}
