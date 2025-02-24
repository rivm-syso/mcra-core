using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class CoExposureTotalDistributionSectionView : SectionView<CoExposureTotalDistributionSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var hiddenProperties = new List<string> { "Frequency" };

            //Render HTML
            if (Model.UpperFullExposureRecords.Any()) {
                sb.AppendParagraph("Co-exposure of substances");
                sb.AppendTable(
                    Model,
                    Model.AggregatedExposureRecords,
                    "AggregatedCoExposureTable",
                    ViewBag,
                    caption: "Exposure frequencies by number of contributing substances.",
                    header: true,
                    saveCsv: true,
                    hiddenProperties: hiddenProperties,
                    displayLimit: 15
                );
                sb.AppendTable(
                    Model,
                    Model.UpperFullExposureRecords,
                    "UpperFullCoExposureTable",
                    ViewBag,
                    caption: "Exposure frequencies for exposures with the highest number of contributing substances.",
                    header: true,
                    saveCsv: true,
                    hiddenProperties: hiddenProperties,
                    displayLimit: 15
                );

                sb.AppendTable(
                    Model,
                    Model.LowerFullExposureRecords,
                    "LowerFullCoExposureTable",
                    ViewBag,
                    caption: "Exposure frequencies for exposures with the lowest number of contributing substances.",
                    header: true,
                    saveCsv: true,
                    hiddenProperties: hiddenProperties,
                    displayLimit: 15
                );

                sb.AppendTable(
                    Model,
                    Model.UpperFullExposureRecordsExtended.OrderByDescending(r => r.Percentage).ToList(),
                    "UpperFullCoExposureTableExtended",
                    ViewBag,
                    caption: "Highest exposure frequencies for exposures containing at least the specified contributing substance(s).",
                    header: true,
                    saveCsv: true,
                    hiddenProperties: hiddenProperties,
                    displayLimit: 15
                );

            } else {
                sb.AppendParagraph("No co-exposures.");
            }
        }
    }
}
