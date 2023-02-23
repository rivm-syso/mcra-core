using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class FoodRecipesSummarySectionView : SectionView<FoodRecipesSummarySection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var distinctCompositeFoods = Model?.Records?.Select(r => r.AsEatenRecipeCode).Distinct().Count() ?? 0;
            var distinctBaseFoods = Model?.Records?.Select(r => r.ConvertedRecipeCode).Distinct().Count() ?? 0;
            sb.AppendDescriptionParagraph($"Total {Model.Records.Count} food recipe records for {distinctCompositeFoods} distinct composite foods and {distinctBaseFoods} distinct base foods.");
            sb.AppendTable(
                Model,
                Model.Records,
                "FoodRecipesTable",
                ViewBag,
                header: true,
                caption: "Recipe information by foods-as-eaten",
                saveCsv: true,
                sortable: true
            );
        }
    }
}
