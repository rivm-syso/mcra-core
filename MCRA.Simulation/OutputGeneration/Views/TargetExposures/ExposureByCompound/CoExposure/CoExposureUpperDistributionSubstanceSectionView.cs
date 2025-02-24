using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class CoExposureUpperDistributionSubstanceSectionView : SectionView<CoExposureUpperDistributionSubstanceSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var hiddenProperties = new List<string> { "Frequency" };
            if (Model.UpperFullExposureRecords.Any()) {
                sb.AppendParagraph("Co-exposure of substances in the upper tail");
                sb.AppendTable(
                    Model,
                    Model.AggregatedExposureRecords.OrderBy(r => r.NumberOfSubstances).ToList(),
                    "AggregatedCoExposureTailTable",
                    ViewBag,
                    caption: "Exposure frequencies for exposures with the highest number of contributing substances.",
                    header: true,
                    saveCsv: true,
                    hiddenProperties: hiddenProperties,
                    displayLimit: 15
                );
                sb.AppendTable(
                    Model,
                    Model.UpperFullExposureRecords.OrderByDescending(r => r.Percentage).ToList(),
                    "UpperFullCoExposureTailTable",
                    ViewBag,
                    caption: "Exposure frequencies for exposures with the highest number of contributing substances.",
                    header: true,
                    saveCsv: true,
                    hiddenProperties: hiddenProperties,
                    displayLimit: 15
                );
                sb.AppendTable(
                    Model,
                    Model.LowerFullExposureRecords.OrderByDescending(r => r.Percentage).ToList(),
                    "LowerFullCoExposureTailTable",
                    ViewBag,
                    caption: "Exposure frequencies for exposures with the lowest number of contributing substances.",
                    header: true,
                    saveCsv: true,
                    hiddenProperties: hiddenProperties,
                    displayLimit: 15
                );
                sb.AppendParagraph("");
                sb.AppendTable(
                    Model,
                    Model.UpperFullExposureRecordsExtended.OrderByDescending(r => r.Percentage).ToList(),
                    "UpperFullCoExposureTailTableExtended",
                    ViewBag,
                    caption: "Highest exposure frequencies for exposures containing at least the specified contributing substance(s).",
                    header: true,
                    saveCsv: true,
                    hiddenProperties: hiddenProperties,
                    displayLimit: 15
                );
            } else {
                sb.AppendParagraph("No co-exposures in the upper tail.");
            }
        }
    }
}
