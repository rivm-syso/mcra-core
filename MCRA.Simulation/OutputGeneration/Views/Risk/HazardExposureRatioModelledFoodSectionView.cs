using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class HazardExposureRatioModelledFoodSectionView : SectionView<HazardExposureRatioModelledFoodSection> {
        public override void RenderSectionHtml(StringBuilder sb) {

            var hiddenProperties = new List<string> {
                nameof(RiskByModelledFoodRecord.NumberOfSubstances),
                nameof(RiskByModelledFoodRecord.MeanAll)
            };
            var isUncertainty = Model.Records.FirstOrDefault()?.Contributions.Any() ?? false;
            if (!isUncertainty) {
                hiddenProperties.Add(nameof(RiskByModelledFoodRecord.LowerContributionPercentage));
                hiddenProperties.Add(nameof(RiskByModelledFoodRecord.UpperContributionPercentage));
                hiddenProperties.Add(nameof(RiskByModelledFoodRecord.MeanContribution));
            } else {
                hiddenProperties.Add(nameof(RiskByModelledFoodRecord.ContributionPercentage));
            }
            //Render HTML
            if (Model.Records.Any()) {
                sb.AppendDescriptionParagraph($"Total {Model.Records.Count} modelled foods.");
                sb.AppendTable(
                    Model,
                    Model.Records,
                    "MarginOfExposureByModelledFoodTable",
                    ViewBag,
                    caption: "Risk statistics by modelled food (total distribution).",
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
