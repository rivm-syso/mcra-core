using MCRA.Simulation.OutputGeneration.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class ModelledFoodsSummarySectionView : SectionView<ModelledFoodsSummarySection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var hiddenProperties = new List<string>();

            //Render HTML
            if (Model.Records?.Any() ?? false) {
                var distinctFoods = Model.Records.Select(r => r.FoodCode).Distinct().Count();
                var distinctSubstances = Model.Records.Select(r => r.SubstanceCode).Distinct().Count();
                var missingCombinations = (distinctFoods * distinctSubstances) - Model.Records.Count;
                var description = $"Total {distinctFoods} modelled foods";
                if (distinctSubstances > 1) {
                    description += $" for {distinctSubstances} different substances.";
                    if (missingCombinations > 0) {
                        description += $" There are {missingCombinations} unmatched food/substance combinations";
                    }
                }
                description += ".";
                sb.AppendDescriptionParagraph(description);
                if (Model.Records?.Any() ?? false) {
                    sb.AppendTable(
                        Model,
                        Model.Records, 
                        "ModelledFoodsTable", 
                        ViewBag,
                        header: true,
                        caption: "Modelled foods",
                        saveCsv: true, 
                        sortable: true,
                        hiddenProperties: hiddenProperties
                    );
                }
            } else {
                sb.AppendDescriptionParagraph("Could not identify any modelled foods.");
            }
        }
    }
}
