using System.Text;
using MCRA.Simulation.OutputGeneration.Helpers;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class DustBioaccessibilityFractionsDataSectionView : SectionView<DustBioaccessibilityFractionsDataSection> {

        public override void RenderSectionHtml(StringBuilder sb) {

            if (Model.Records.Any()) {
                // Description
                var totalRecords = Model.Records.Count;
                sb.AppendDescriptionParagraph($"Total {totalRecords} dust bioaccessibility fraction records.");

                var hiddenProperties = new List<string>();

                if (Model.Records.All(c => string.IsNullOrEmpty(c.DistributionType))) {
                    hiddenProperties.Add(nameof(BioaccessibilityFractionsDataRecord.DistributionType));
                    hiddenProperties.Add(nameof(BioaccessibilityFractionsDataRecord.CvVariability));
                }

                // Table
                sb.AppendTable(
                    Model,
                    Model.Records,
                    "DustBioaccessibilityFractionDataTable",
                    ViewBag,
                    caption: "Fraction of dust bioaccessibility for ingestion.",
                    saveCsv: true,
                    hiddenProperties: hiddenProperties
                );
            } else {
                sb.AppendDescriptionParagraph("No dust bioaccessibility fraction data available.");
            }
        }

    }
}
