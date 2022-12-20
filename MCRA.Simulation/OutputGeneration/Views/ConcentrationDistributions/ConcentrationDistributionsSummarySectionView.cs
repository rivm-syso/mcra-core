using MCRA.Simulation.OutputGeneration.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class ConcentrationDistributionsSummarySectionView : SectionView<ConcentrationDistributionsSummarySection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            if (Model.Records.Any()) {
                // Description
                var totalRecords = Model.Records.Count;
                var numberOfFoods = Model.Records.Select(r => r.FoodCode).Distinct().Count();
                var numberOfSubstances = Model.Records.Select(r => r.CompoundCode).Distinct().Count();
                sb.AppendDescriptionParagraph($"Total {totalRecords} concentration distributions for {numberOfFoods} foods and {numberOfSubstances} substances.");

                var hiddenProperties = new List<string>();

                // Table
                sb.AppendTable(
                    Model,
                    Model.Records,
                    "ConcentrationDistributionsDataTable",
                    ViewBag,
                    caption: "Concentration distributions.",
                    saveCsv: true,
                    hiddenProperties: hiddenProperties
                );
            } else {
                sb.AppendDescriptionParagraph("No concentration distributions available for the selected foods and/or substances.");
            }
        }
    }
}
