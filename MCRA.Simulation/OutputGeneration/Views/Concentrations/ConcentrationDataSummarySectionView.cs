using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class ConcentrationDataSummarySectionView : SectionView<ConcentrationDataSummarySection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            if (Model.TotalNumberOfSamples > 0) {
                sb.AppendDescriptionParagraph("Summary statistics for samples.");
                sb.Append("<table>");
                if (Model.TotalNumberOfSamples > 0) {
                    sb.AppendTableRow("Total number of samples", Model.TotalNumberOfSamples);
                }
                sb.AppendTableRow("Total number of analysed foods", Model.TotalNumberOfAnalysedFoods);
                sb.AppendTableRow("Total number of analysed substances", Model.TotalNumberOfAnalysedSubstances);
                sb.Append("</table>");
            } else {
                sb.AppendDescriptionParagraph("No concentration data available.");
            }
        }
    }
}
