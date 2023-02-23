using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class OccurrenceFrequenciesSummarySectionView : SectionView<OccurrenceFrequenciesSummarySection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var hiddenProperties = new List<string>();
            if (Model.Records.All(r => r.AgriculturalUseFractionUncertaintyValues == null || !r.AgriculturalUseFractionUncertaintyValues.Any())) {
                hiddenProperties.Add("AgriculturalUseFractionLowerBoundPercentile");
                hiddenProperties.Add("AgriculturalUseFractionUpperBoundPercentile");
                hiddenProperties.Add("AgriculturalUseFractionMeanUncertaintyValue");
            } else {
                if (Model.Records.Any(r => r.AgriculturalUseFractionUncertaintyValues.Distinct().Count() > 2)) {
                    hiddenProperties.Add("AgriculturalUseFractionMeanUncertaintyValue");
                } else {
                    hiddenProperties.Add("AgriculturalUseFractionLowerBoundPercentile");
                    hiddenProperties.Add("AgriculturalUseFractionUpperBoundPercentile");
                }
            }
            if (Model.Records.All(r => r.IsAuthorised == null)) {
                hiddenProperties.Add("IsAuthorised");
            }

            //Render HTML
            sb.AppendTable(
               Model,
               Model.Records,
               "AgriculturalUseByFoodSubstanceTable",
               ViewBag,
               caption: "agricultural use by food x substance.",
               saveCsv: true,
               header: true,
               hiddenProperties: hiddenProperties
            );
        }
    }
}
