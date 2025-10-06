using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class SampleOriginsSummarySectionView : SectionView<SampleOriginsSummarySection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var distinctFoods = Model.Records.Select(r => r.FoodCode).Distinct().Count();
            var distinctOrigins = Model.Records.Select(r => r.Origin).Distinct().Count();
            sb.AppendDescriptionParagraph($"Number of samples per modelled foods x origin: {Model.Records.Count}.");
            sb.AppendDescriptionParagraph($"Number of different foods: {distinctFoods}.");
            sb.AppendDescriptionParagraph($"Number of different origins: {distinctOrigins}.");
            //Render HTML
            sb.AppendTable(
               Model,
               Model.Records,
               "SampleOriginTable",
               ViewBag,
               caption: "Food sample origin statistics.",
               saveCsv: true,
               header: true
            );
        }
    }
}
