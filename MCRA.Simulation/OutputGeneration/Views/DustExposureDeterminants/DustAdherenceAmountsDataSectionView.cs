using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class DustAdherenceAmountsDataSectionView : SectionView<DustAdherenceAmountsDataSection> {

        public override void RenderSectionHtml(StringBuilder sb) {

            if (Model.Records.Any()) {
                // Description
                var totalRecords = Model.Records.Count;
                sb.AppendDescriptionParagraph($"Total {totalRecords} dust adherence amount records.");

                var hiddenProperties = new List<string>();
                if (Model.Records.All(c => c.AgeLower == null) &
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
                    "DustAdherenceAmountsDataTable",
                    ViewBag,
                    caption: "Amount of dust adhering to the skin (g/m^2/day).",
                    saveCsv: true,
                    hiddenProperties: hiddenProperties
                );
            } else {
                sb.AppendDescriptionParagraph("No dust adherence amount data available.");
            }
        }

    }
}
