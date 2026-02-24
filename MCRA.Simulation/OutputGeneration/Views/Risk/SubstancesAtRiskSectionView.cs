using MCRA.Utils.ExtensionMethods;
using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class SubstancesAtRiskSectionView : SectionView<SubstancesAtRiskSection> {
        public override void RenderSectionHtml(StringBuilder sb) {

            var hiddenProperties = new List<string>();
            if (Model.Records.All(c => c.AtRiskWithOrWithout == 0)) {
                hiddenProperties.Add(nameof(SubstanceAtRiskRecord.AtRiskWithOrWithout));
            }
            if (Model.Records.All(c => c.AtRiskDueToSubstance == 0)) {
                hiddenProperties.Add(nameof(SubstanceAtRiskRecord.AtRiskDueToSubstance));
            }
            //Render HTML
            if (Model.Records.Any()) {
                sb.AppendDescriptionParagraph($"Total {Model.Records.Count} substances.");
                sb.AppendTable(
                    Model,
                    Model.Records,
                    "SubstanceAtRiskBackgroundForegroundTable",
                    ViewBag,
                    caption: $"Percentage at risk by substance. {Model.RiskMetric.GetShortDisplayName()} threshold = {Model.Threshold}",
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
