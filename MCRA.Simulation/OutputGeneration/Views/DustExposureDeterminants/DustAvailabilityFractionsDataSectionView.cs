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
                if (Model.Records.All(c => c.AgeLower == null) &&
                    Model.Records.All(c => string.IsNullOrEmpty(c.Sex))) {
                    hiddenProperties.Add("idSubgroup");
                }
                if (Model.Records.All(c => string.IsNullOrEmpty(c.DistributionType))) {
                    hiddenProperties.Add("DistributionType");
                    hiddenProperties.Add("CvVariability");
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
