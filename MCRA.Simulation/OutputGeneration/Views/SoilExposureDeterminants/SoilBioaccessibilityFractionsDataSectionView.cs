using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class SoilBioaccessibilityFractionsDataSectionView : SectionView<SoilBioaccessibilityFractionsDataSection> {

        public override void RenderSectionHtml(StringBuilder sb) {

            if (Model.Records.Any()) {
                // Description
                var totalRecords = Model.Records.Count;
                sb.AppendDescriptionParagraph($"Total {totalRecords} soil bioaccessibility fraction records.");

                var hiddenProperties = new List<string>();
  
                if (Model.Records.All(c => string.IsNullOrEmpty(c.DistributionType))) {
                    hiddenProperties.Add(nameof(BioaccessibilityFractionsDataRecord.DistributionType));
                    hiddenProperties.Add(nameof(BioaccessibilityFractionsDataRecord.CvVariability));
                }

                // Table
                sb.AppendTable(
                    Model,
                    Model.Records,
                    "SoilBioaccessibilityFractionDataTable",
                    ViewBag,
                    caption: "Fraction of soil bioaccessibility for ingestion.",
                    saveCsv: true,
                    hiddenProperties: hiddenProperties
                );
            } else {
                sb.AppendDescriptionParagraph("No soil bioaccessibility fraction data available.");
            }
        }

    }
}
