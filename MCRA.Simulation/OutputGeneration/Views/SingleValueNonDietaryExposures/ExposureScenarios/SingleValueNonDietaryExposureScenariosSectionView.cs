using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class SingleValueNonDietaryExposureScenariosSectionView : SectionView<SingleValueNonDietaryExposureScenariosSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            if (Model.Records?.Count > 0) {
                var hiddenProperties = new List<string>();
                if (Model.Records.All(r => string.IsNullOrEmpty(r.ExposureRoutes))) {
                    hiddenProperties.Add("ExposureRoutes");
                }

                var descriptions = new List<string>();
                var description = "The different sets of conditions for which the single-value estimates of non-dietary exposures were collected.";
                descriptions.AddDescriptionItem(description);
                sb.AppendDescriptionList(descriptions);

                sb.AppendTable(
                    Model,
                    Model.Records,
                    "SingleValueNonDietaryExposureScenariosTable",
                    ViewBag,
                    header: true,
                    caption: "Exposure scenarios covered by the single-value exposure estimates.",
                    saveCsv: true,
                    hiddenProperties: hiddenProperties
                );
            } else {
                sb.AppendParagraph("Error: failed to compute single value non-dietary exposure scenarios.", "warning");
            }
        }
    }
}
