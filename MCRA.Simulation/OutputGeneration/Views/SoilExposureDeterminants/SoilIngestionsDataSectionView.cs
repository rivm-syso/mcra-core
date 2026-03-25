using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class SoilIngestionsDataSectionView : SectionView<SoilIngestionsDataSection> {

        public override void RenderSectionHtml(StringBuilder sb) {

            if (Model.Records.Any()) {
                // Description
                var totalRecords = Model.Records.Count;
                sb.AppendDescriptionParagraph($"Total {totalRecords} soil ingestion records.");

                var hiddenProperties = new List<string>();
                if (Model.Records.All(c => !c.AgeLower.HasValue) &&
                    Model.Records.All(c => string.IsNullOrEmpty(c.Sex))) {
                    hiddenProperties.Add(nameof(SoilIngestionsDataRecord.idSubgroup));
                }
                if (Model.Records.All(c => string.IsNullOrEmpty(c.DistributionType))) {
                    hiddenProperties.Add(nameof(SoilIngestionsDataRecord.DistributionType));
                    hiddenProperties.Add(nameof(SoilIngestionsDataRecord.CvVariability));
                }
                if (Model.Records.All(c => string.IsNullOrEmpty(c.Sex))) {
                    hiddenProperties.Add(nameof(SoilIngestionsDataRecord.Sex));
                }
                if (Model.Records.All(c => !c.AgeLower.HasValue))  {
                    hiddenProperties.Add(nameof(SoilIngestionsDataRecord.AgeLower));
                }
                // Table
                sb.AppendTable(
                    Model,
                    Model.Records,
                    "SoilIngestionDataTable",
                    ViewBag,
                    caption: "Daily amount of soil ingested.",
                    saveCsv: true,
                    hiddenProperties: hiddenProperties
                );
            } else {
                sb.AppendDescriptionParagraph("No soil ingestion data available.");
            }
        }

    }
}
