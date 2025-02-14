using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class PbkModelDefinitionParametersSummarySectionView : SectionView<PbkModelDefinitionParametersSummarySection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            if (Model.Records.Any()) {
                sb.AppendTable(
                   Model,
                   Model.Records,
                   $"PbkModelParametersTable_{Model.ModelCode}",
                   ViewBag,
                   caption: $"PBK model parameters {Model.ModelCode}.",
                   saveCsv: true
                );
            } else {
                sb.AppendDescriptionParagraph($"PBK model {Model.ModelCode} does not have any parameters.");
            }
        }
    }
}

