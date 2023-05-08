using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class ConditionalIntakePercentageSectionView : SectionView<ConditionalIntakePercentageSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            //Render HTML
            foreach (var item in Model.ConditionalIntakePercentageSections) {
                string covariableLabel = string.Empty;
                string cofactorLabel = string.Empty;
                if (!double.IsNaN(item.CovariatesCollection.OverallCovariable)) {
                    covariableLabel = $"{item.CovariatesCollection.CovariableName}: {item.CovariatesCollection.OverallCovariable:F1}";
                }
                if (item.CovariatesCollection.OverallCofactor != string.Empty) {
                    cofactorLabel = $"{item.CovariatesCollection.CofactorName}: {item.CovariatesCollection.OverallCofactor}";
                }
                sb.AppendParagraph(cofactorLabel);
                sb.AppendParagraph(covariableLabel);
                renderSectionView(sb, "IntakePercentageSection", item.IntakePercentageSection);
            }
        }
    }
}
