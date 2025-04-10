using System.Text;
using MCRA.Simulation.OutputGeneration.Helpers;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class PopulationCharacteristicsSummarySectionView : SectionView<PopulationCharacteristicsSummarySection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            //Render HTML
            var hiddenProperties = new List<string>();
            if (Model.Records.All(r => r.CV == null || double.IsNaN((double)r.CV))) {
                hiddenProperties.Add("CV");
            }

            if (Model.Records.Count != 0) {
                sb.AppendTable(
                    Model,
                    Model.Records,
                    "PopulationCharacteristicsTable",
                    ViewBag,
                    caption: "Summary population characteristics.",
                    saveCsv: true,
                    hiddenProperties: hiddenProperties
                );
            } else {
                sb.AppendDescriptionParagraph($"No population characteristics available.");
            }
        }
    }
}
