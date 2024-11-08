using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class SingleValueNonDietaryExposuresSectionView : SectionView<SingleValueNonDietaryExposuresSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            if (Model.Records?.Count > 0) {
                var hiddenProperties = new List<string>();
                var descriptions = new List<string>();
                var description = "Single value exposure estimates from non-dietary sources.";
                descriptions.AddDescriptionItem(description);
                sb.AppendDescriptionList(descriptions);

                sb.AppendTable(
                    Model,
                    Model.Records,
                    "SingleValueNonDietaryExposuresTable",
                    ViewBag,
                    header: true,
                    caption: "Single-value exposure estimates.",
                    saveCsv: true,
                    hiddenProperties: hiddenProperties
                );
            } else {
                sb.AppendParagraph("Error: failed to compute single value non-dietary exposures.", "warning");
            }
        }
    }
}
