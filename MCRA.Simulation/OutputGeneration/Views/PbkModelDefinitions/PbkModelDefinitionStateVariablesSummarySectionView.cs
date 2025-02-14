using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class PbkModelDefinitionStateVariablesSummarySectionView : SectionView<PbkModelDefinitionStateVariablesSummarySection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            if (Model.Records.Any()) {
                sb.AppendTable(
                   Model,
                   Model.Records,
                   $"PbkModelStateVariablesTable_{Model.ModelCode}",
                   ViewBag,
                   caption: $"PBK model state variables {Model.ModelCode}.",
                   saveCsv: true
                );
            } else {
                sb.AppendDescriptionParagraph($"PBK model {Model.ModelCode} does not have any state variables.");
            }
        }
    }
}