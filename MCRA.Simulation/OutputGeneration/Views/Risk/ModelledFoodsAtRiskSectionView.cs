using MCRA.Utils.ExtensionMethods;
using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class ModelledFoodsAtRiskSectionView : SectionView<ModelledFoodsAtRiskSection> {
        public override void RenderSectionHtml(StringBuilder sb) {

            var hiddenProperties = new List<string>();
            if (Model.Records.All(c => c.AtRiskWithOrWithout == 0)) {
                hiddenProperties.Add("AtRiskWithOrWithout");
            }
            if (Model.Records.All(c => c.AtRiskDueToFood == 0)) {
                hiddenProperties.Add("AtRiskDueToFood");
            }
            //Render HTML
            if (Model.Records.Any()) {
                sb.AppendDescriptionParagraph($"Total {Model.Records.Count} modelled foods.");
                sb.AppendTable(
                    Model,
                    Model.Records,
                    "FoodAtRiskBackgroundForegroundTable",
                    ViewBag,
                    caption: $"Percentage at risk by modelled food. {Model.RiskMetric.GetShortDisplayName()} threshold = {Model.Threshold}",
                    saveCsv: true,
                    displayLimit: 20,
                    hiddenProperties: hiddenProperties
                );
            } else {
                sb.AppendNotification("No positive exposures.");
            }
        }
    }
}
