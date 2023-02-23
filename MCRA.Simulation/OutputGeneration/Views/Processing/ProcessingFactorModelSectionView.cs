using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class ProcessingFactorModelSectionView : SectionView<ProcessingFactorModelSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            if (Model.Records?.Any() ?? false) {
                var hiddenProperties = new List<string>();
                if (Model.Records.All(r => string.IsNullOrEmpty(r.Distribution))) {
                    hiddenProperties.Add("Mu");
                    hiddenProperties.Add("Sigma");
                }
                sb.AppendDescriptionParagraph($"Total {Model.Records.Count} processing factor models.");
                sb.AppendTable(
                    Model,
                    Model.Records,
                    "ProcessingFactorModelsTable",
                    ViewBag,
                    header: true,
                    caption: "Processing factors models summary.",
                    saveCsv: true,
                    hiddenProperties: hiddenProperties
                );
            } else {
                sb.AppendDescriptionParagraph("No processing factors models available.");
            }
        }
    }
}
