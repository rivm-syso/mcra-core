using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class ConditionalIntakePercentileSectionView : SectionView<ConditionalIntakePercentileSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            //Render HTML
            foreach (var item in Model.ConditionalIntakePercentileSections) {
                string covariableLabel = string.Empty;
                string cofactorLabel = string.Empty;
                if (!double.IsNaN(item.CovariatesCollection.OverallCovariable)) {
                    covariableLabel = $"{item.CovariatesCollection.CovariableName}: {item.CovariatesCollection.OverallCovariable:F1}";
                }
                if (item.CovariatesCollection.OverallCofactor != string.Empty) {
                    cofactorLabel = $"{item.CovariatesCollection.CofactorName}: {item.CovariatesCollection.OverallCofactor}";
                }
                sb.AppendParagraph($"{cofactorLabel}");
                sb.AppendParagraph($"{covariableLabel}");
                renderSectionView(sb, "IntakePercentileSection", item.IntakePercentileSection);
            }
        }
    }
}
