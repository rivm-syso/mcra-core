using MCRA.Simulation.OutputGeneration.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class ConcentrationExtrapolationsSummarySectionView : SectionView<ConcentrationExtrapolationsSummarySection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var distinctFoods = Model.Records.Select(r => r.FoodCode).Distinct().Count();
            var distinctSubstances = Model.Records.Select(r => r.ActiveSubstanceCode).Distinct().Count();
            //Render HTML
            sb.AppendDescriptionParagraph($"Number of records: {Model.Records.Count}.");
            sb.AppendDescriptionParagraph($"Number of different extrapolated foods: {distinctFoods}.");
            sb.AppendDescriptionParagraph($"Number of different substances: {distinctSubstances}.");
            sb.AppendTable(
                Model,
                Model.Records,
                "ConcentrationExtrapolationsTable",
                ViewBag,
                caption: "Food extrapolation information.",
                header: true,
                saveCsv: true
            );
        }
    }
}
