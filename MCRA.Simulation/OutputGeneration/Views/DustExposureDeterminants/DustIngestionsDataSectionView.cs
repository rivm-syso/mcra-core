using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class DustIngestionsDataSectionView : SectionView<DustIngestionsDataSection> {

        public override void RenderSectionHtml(StringBuilder sb) {

            if (Model.Records.Any()) {
                // Description
                var totalRecords = Model.Records.Count;
                sb.AppendDescriptionParagraph($"Total {totalRecords} dust ingestion records.");

                var hiddenProperties = new List<string>();
                if (Model.Records.All(c => !c.AgeLower.HasValue) &&
                    Model.Records.All(c => string.IsNullOrEmpty(c.Sex))) {
                    hiddenProperties.Add(nameof(DustIngestionsDataRecord.idSubgroup));
                }
                if (Model.Records.All(c => string.IsNullOrEmpty(c.DistributionType))) {
                    hiddenProperties.Add(nameof(DustIngestionsDataRecord.DistributionType));
                    hiddenProperties.Add(nameof(DustIngestionsDataRecord.CvVariability));
                }
                if (Model.Records.All(c => !c.AgeLower.HasValue)) {
                    hiddenProperties.Add(nameof(DustIngestionsDataRecord.AgeLower));
                }
                if (Model.Records.All(c => string.IsNullOrEmpty(c.Sex))) {
                    hiddenProperties.Add(nameof(DustIngestionsDataRecord.Sex));
                }

                // Table
                sb.AppendTable(
                    Model,
                    Model.Records,
                    "DustIngestionDataTable",
                    ViewBag,
                    caption: "Daily amount of dust ingested.",
                    saveCsv: true,
                    hiddenProperties: hiddenProperties
                );
            } else {
                sb.AppendDescriptionParagraph("No dust ingestion data available.");
            }
        }
    }
}
