using MCRA.Simulation.OutputGeneration.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class UnMatchedFoodAsEatenSummarySectionView : SectionView<UnMatchedFoodAsEatenSummarySection> {
        public override void RenderSectionHtml(StringBuilder sb) {

            //Render HTML
            if (Model.FoodsNotFound > 0) {
                var hiddenProperties = new List<string>() {
                    "FoodAsMeasuredName",
                    "FoodAsMeasuredCode",
                    "CompoundCode",
                    "CompoundName",
                    "ConversionSteps"
                };
                if (Model.Records.Select(r => r.CompoundCode).Distinct().Count() == 1) {
                    hiddenProperties.Add("CompoundCode");
                    hiddenProperties.Add("FoodAsMeasuredName");
                }
                sb.AppendDescriptionParagraph($"Number of unmatched foods as eaten: {Model.FoodsNotFound}");
                if (!Model.SubstanceIndependent) {
                    sb.AppendParagraph("Note: food conversion was done by substance. The table below shows all foods as eaten for which no modelled food was found for any substance.", "note");
                }
                sb.AppendTable(
                    Model,
                    Model.Records,
                    "UnmatchedFoodsAsEatenTable",
                    ViewBag,
                    caption: "Unmatched foods as eaten.",
                    header: true,
                    saveCsv: true,
                    displayLimit: 20,
                    hiddenProperties: hiddenProperties
                );
            } else {
                sb.AppendParagraph("No unmatched foods as eaten found.");
            }
        }
    }
}
