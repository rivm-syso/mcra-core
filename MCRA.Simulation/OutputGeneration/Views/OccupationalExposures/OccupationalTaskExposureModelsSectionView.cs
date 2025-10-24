using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class OccupationalTaskExposureModelsSectionView : SectionView<OccupationalTaskExposureModelsSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            if (Model.Records.Any()) {
                sb.AppendDescriptionParagraph($"Total {Model.Records.Count} records.");
                sb.AppendTable(
                    Model,
                    Model.Records,
                    "OccupationalTaskExposureModelsTable",
                    ViewBag,
                    caption: "Occupational task exposure models.",
                    saveCsv: true
                );
            } else {
                sb.AppendDescriptionParagraph("No occupational task exposure models available.");
            }
        }
    }
}
