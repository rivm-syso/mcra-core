using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class OccurrenceFrequenciesSummarySectionView : SectionView<OccurrenceFrequenciesSummarySection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var hiddenProperties = new List<string>();
            if (Model.Records.All(r => r.AgriculturalUseFractionUncertaintyValues == null || !r.AgriculturalUseFractionUncertaintyValues.Any())) {
                hiddenProperties.Add(nameof(AgriculturalUseByFoodSubstanceSummaryRecord.AgriculturalUseFractionLowerBoundPercentile));
                hiddenProperties.Add(nameof(AgriculturalUseByFoodSubstanceSummaryRecord.AgriculturalUseFractionUpperBoundPercentile));
                hiddenProperties.Add(nameof(AgriculturalUseByFoodSubstanceSummaryRecord.AgriculturalUseFractionMeanUncertaintyValue));
            } else {
                if (Model.Records.Any(r => r.AgriculturalUseFractionUncertaintyValues.Distinct().Count() > 2)) {
                    hiddenProperties.Add(nameof(AgriculturalUseByFoodSubstanceSummaryRecord.AgriculturalUseFractionMeanUncertaintyValue));
                } else {
                    hiddenProperties.Add(nameof(AgriculturalUseByFoodSubstanceSummaryRecord.AgriculturalUseFractionLowerBoundPercentile));
                    hiddenProperties.Add(nameof(AgriculturalUseByFoodSubstanceSummaryRecord.AgriculturalUseFractionUpperBoundPercentile));
                }
            }
            if (Model.Records.All(r => r.IsAuthorised == null)) {
                hiddenProperties.Add(nameof(AgriculturalUseByFoodSubstanceSummaryRecord.IsAuthorised));
            }

            //Render HTML
            sb.AppendTable(
               Model,
               Model.Records,
               "AgriculturalUseByFoodSubstanceTable",
               ViewBag,
               caption: "Occurrence frequencies by food and substance.",
               saveCsv: true,
               header: true,
               hiddenProperties: hiddenProperties
            );
        }
    }
}
