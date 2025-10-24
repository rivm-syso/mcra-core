using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class OccupationalScenarioTasksExposuresSectionView : SectionView<OccupationalScenarioTasksExposuresSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            if (Model.Records.Any()) {

                var hiddenProperties = new List<string>();
                if (Model.Records.All(r => string.IsNullOrEmpty(r.EstimateType))) {
                    hiddenProperties.Add("EstimateType");
                }

                sb.AppendDescriptionParagraph($"Total {Model.Records.Count} records.");
                sb.AppendTable(
                    Model,
                    Model.Records,
                    "OccupationalScenarioTaskExposuresTable",
                    ViewBag,
                    caption: "Occupational scenario task exposures.",
                    saveCsv: true,
                    hiddenProperties: hiddenProperties
                );
            } else {
                sb.AppendDescriptionParagraph("No occupational scenario exposures available.");
            }
        }
    }
}
