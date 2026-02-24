using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class ExposureHazardRatioModelledFoodSubstanceSectionView : SectionView<ExposureHazardRatioModelledFoodSubstanceSection> {
        public override void RenderSectionHtml(StringBuilder sb) {

            var hiddenProperties = new List<string>();
            var isUncertainty = Model.Records.FirstOrDefault()?.Contributions.Any() ?? false;
            if (!isUncertainty) {
                hiddenProperties.Add(nameof(RiskByFoodSubstanceRecord.LowerContributionPercentage));
                hiddenProperties.Add(nameof(RiskByFoodSubstanceRecord.UpperContributionPercentage));
                hiddenProperties.Add(nameof(RiskByFoodSubstanceRecord.MeanContribution));
            } else {
                hiddenProperties.Add(nameof(RiskByFoodSubstanceRecord.ContributionPercentage));
            }

            //Render HTML
            if (Model.Records.Any()) {
                sb.AppendDescriptionParagraph($"Total {Model.Records.Count} modelled foods x substances.");
                sb.AppendTable(
                    Model,
                    Model.Records,
                    "HazardIndexByModelledFoodSubstanceTable",
                    ViewBag,
                    caption: "Risk statistics by modelled food x substance (total distribution).",
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
