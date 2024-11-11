using MCRA.Utils.ExtensionMethods;
using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class ModelThenAddSummarySectionView : SectionView<ModelThenAddSummarySection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            sb.AppendParagraph(Model.IntakeModel.GetDisplayName());
            sb.AppendParagraph($"Category: {string.Join(", ", Model.FoodNames)}");
        }
    }
}
