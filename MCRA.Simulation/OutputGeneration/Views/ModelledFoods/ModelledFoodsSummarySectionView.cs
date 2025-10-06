using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class ModelledFoodsSummarySectionView : SectionView<ModelledFoodsSummarySection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            if (Model.Records?.Count > 0) {
                var distinctFoods = Model.Records.Select(r => r.FoodCode).Distinct().Count();
                var description = $"Total {distinctFoods} modelled foods.";
                sb.AppendDescriptionParagraph(description);
                if (Model.Records?.Count > 0) {
                    sb.AppendTable(
                        Model,
                        Model.Records,
                        "ModelledFoodsTable",
                        ViewBag,
                        header: true,
                        caption: "Modelled foods",
                        saveCsv: true,
                        sortable: true
                    );
                }
            } else {
                sb.AppendDescriptionParagraph("Could not identify any modelled foods.");
            }
        }
    }
}
