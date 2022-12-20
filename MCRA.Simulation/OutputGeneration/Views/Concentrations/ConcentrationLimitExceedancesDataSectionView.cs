using MCRA.Simulation.OutputGeneration.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class ConcentrationLimitExceedancesDataSectionView : SectionView<ConcentrationLimitExceedancesDataSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            if (Model.Records?.Any() ?? false) {
                var noRemovedSamples = Model.Records.Sum(c => c.NumberOfSamplesExceedingLimit);
                var noRetainedSamples = Model.Records.Sum(c => c.TotalNumberOfAnalysedSamples) - noRemovedSamples;
                var foodsCount = Model.Records.Select(r => r.FoodCode).Distinct().Count();
                var substancesCount = Model.Records.Select(r => r.SubstanceCode).Distinct().Count();
                sb.AppendDescriptionParagraph($"Total {Model.Records.Count} concentration limits (for {foodsCount} foods and {substancesCount} substances) with exceedances of more than {Model.ExceedanceFactionThreshold:P1} of the limit value.");
                sb.AppendDescriptionParagraph($"Removed {noRemovedSamples} background samples with concentrations > {Model.ExceedanceFactionThreshold} * limit value, retained {noRetainedSamples} samples.");
                sb.AppendTable(
                    Model,
                    Model.Records,
                    "ConcentrationLimitExceedancesDataTable",
                    ViewBag,
                    caption: "Concentration limits exceedances.",
                    header: true,
                    saveCsv: true
                );
            } else {
                sb.AppendDescriptionParagraph($"No concentration limits exceedances of more than {Model.ExceedanceFactionThreshold:P1} of the limit value were detected.");
            }
        }
    }
}
