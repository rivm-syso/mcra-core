using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class SubstanceConversionsSummarySectionView : SectionView<SubstanceConversionsSummarySection> {
        public override void RenderSectionHtml(StringBuilder sb) {

            string description;
            var partNormal = $"{Model.SubstanceConversionsCount} substance conversion rules";
            var partDeterministic = $"{Model.DeterministicSubstanceConversionsCount} determinstic conversion factors";
            if (Model.SubstanceConversionsCount > 0 && Model.DeterministicSubstanceConversionsCount > 0) {
                description = $"Total {partNormal} and {partDeterministic}.";
            } else if (Model.SubstanceConversionsCount > 0) {
                description = $"Total {partNormal}.";
            } else if (Model.DeterministicSubstanceConversionsCount > 0) {
                description = $"Total {partDeterministic}.";
            } else {
                description = "No substance conversions available.";
            }
            sb.AppendDescriptionParagraph(description);
        }
    }
}
