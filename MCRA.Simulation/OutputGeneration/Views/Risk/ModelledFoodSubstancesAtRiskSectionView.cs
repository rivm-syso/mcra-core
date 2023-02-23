using MCRA.Utils.ExtensionMethods;
using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class ModelledFoodSubstancesAtRiskSectionView : SectionView<ModelledFoodSubstancesAtRiskSection> {
        public override void RenderSectionHtml(StringBuilder sb) {

            var hiddenProperties = new List<string>();
            if (Model.Records.All(c => c.AtRiskWithOrWithout == 0)) {
                hiddenProperties.Add("AtRiskWithOrWithout");
            }
            if (Model.Records.All(c => c.AtRiskDueToModelledFoodSubstance == 0)) {
                hiddenProperties.Add("AtRiskDueToModelledFoodSubstance");
            }
            //Render HTML
            if (Model.Records.Any()) {
                sb.AppendDescriptionParagraph($"Total {Model.Records.Count} modelled foods x substances.");
                sb.AppendTable(
                    Model,
                    Model.Records,
                    "ModelledFoodSubstanceAtRiskBackgroundForegroundTable",
                    ViewBag,
                    caption: $"Percentage at risk by modelled food x substance. {Model.RiskMetric.GetShortDisplayName()} threshold = {Model.Threshold}",
                    saveCsv: true,
                    displayLimit: 20,
                    hiddenProperties: hiddenProperties
                );
            } else {
                sb.AppendParagraph("No positive exposures found", "warning");
            }
        }
    }
}
