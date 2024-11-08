using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class TotalDietStudyCompositionsSummarySectionView : SectionView<TotalDietStudyCompositionsSummarySection> {
        public override void RenderSectionHtml(StringBuilder sb) {

            //Render HTML
            sb.AppendDescriptionParagraph($"Number of TDS foods: {Model.Records.Count}");

            sb.AppendTable(
               Model,
               Model.Records,
               "TDSTable",
               ViewBag,
               caption: "Summary of the number of TDS foods with the number of sub foods found for it.",
               saveCsv: true,
               header: true
            );
        }
    }
}
