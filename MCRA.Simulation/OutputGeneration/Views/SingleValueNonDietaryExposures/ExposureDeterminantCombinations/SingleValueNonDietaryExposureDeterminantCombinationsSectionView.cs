using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class SingleValueNonDietaryExposureDeterminantCombinationsSectionView : SectionView<SingleValueNonDietaryExposureDeterminantCombinationsSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            if (Model.DeterminantCombinationValueRecords?.Count > 0) {
                var hiddenProperties = new List<string>();
                var descriptions = new List<string>();
                var description = "Single value exposure determinants from non-dietary exposure sources.";
                descriptions.AddDescriptionItem(description);
                sb.AppendDescriptionList(descriptions);

                sb.AppendTable(
                    Model,
                    Model.DeterminantCombinationValueRecords,
                    "SingleValueNonDietaryExposureDeterminantCombinationValuesTable",
                    ViewBag,
                    header: true,
                    caption: "Exposure determinants.",
                    saveCsv: true,
                    hiddenProperties: hiddenProperties
                );
            } else {
                sb.AppendParagraph("Error: failed to compute single value non-dietary determinants.", "warning");
            }
        }
    }
}
