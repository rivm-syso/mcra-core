using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class HbmConcentrationModelsSectionView : SectionView<HbmConcentrationModelsSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var hiddenProperties = new List<string>();
            //Render HTML

            if (Model.Records.Count > 0) {
                //Render HTML
                sb.AppendTable(
                    Model,
                    Model.Records,
                    "HbmConcentrationModelsTable",
                    ViewBag,
                    caption: $"Human biomonitoring concentration models per biological matrix and substance.",
                    saveCsv: true,
                    header: true,
                    hiddenProperties: hiddenProperties
                );
            } else {
                sb.AppendParagraph($"No concentration models available");
            }
        }
    }
}
