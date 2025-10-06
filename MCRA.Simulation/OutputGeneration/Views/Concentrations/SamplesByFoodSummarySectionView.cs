using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class SamplesByFoodSummarySectionView : SectionView<SamplesByFoodSummarySection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var foodsCount = Model.Records.Count();
            sb.AppendDescriptionParagraph($"Number of foods: {foodsCount}.");
            sb.AppendTable(
               Model,
               Model.Records,
               "SamplesByFoodTable",
               ViewBag,
               caption: "Samples by food.",
               saveCsv: true,
               header: true
            );
        }
    }
}
