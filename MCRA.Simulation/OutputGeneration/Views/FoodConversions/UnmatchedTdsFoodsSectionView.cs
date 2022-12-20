using MCRA.Simulation.OutputGeneration.Helpers;
using System.Collections.Generic;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class UnmatchedTdsFoodsSectionView : SectionView<UnmatchedTdsFoodsSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var hiddenProperties = new List<string> {
                "FoodAsMeasuredName",
                "FoodAsMeasuredCode"
            };

            //Render HTML
            if (Model.FoodsNotFound > 1) {
                sb.AppendDescriptionParagraph($"Number of unmatched foods: {Model.UnmatchedFoodsSummaryRecords.Count}");
                sb.AppendTable(
                   Model,
                   Model.UnmatchedFoodsSummaryRecords,
                   "UnMatchedConversionTable",
                   ViewBag,
                   caption: "Unmatched foods in conversion.",
                   saveCsv: true,
                   header: true,
                   hiddenProperties: hiddenProperties
                );
            } else {
                sb.AppendParagraph("Not available");
            }
        }
    }
}
