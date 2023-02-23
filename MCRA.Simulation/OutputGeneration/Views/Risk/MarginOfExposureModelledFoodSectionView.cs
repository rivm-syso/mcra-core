using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class MarginOfExposureModelledFoodSectionView : SectionView<MarginOfExposureModelledFoodSection> {
        public override void RenderSectionHtml(StringBuilder sb) {

            var hiddenProperties = new List<string>();
            hiddenProperties.Add("NumberOfSubstances");

            var isUncertainty = Model.Records.FirstOrDefault()?.Contributions.Any() ?? false;
            if (!isUncertainty) {
                hiddenProperties.Add("LowerContributionPercentage");
                hiddenProperties.Add("UpperContributionPercentage");
                hiddenProperties.Add("MeanContribution");
            } else {
                hiddenProperties.Add("ContributionPercentage");
            }

            //Render HTML
            if (Model.Records.Any()) {
                sb.AppendDescriptionParagraph($"Total {Model.Records.Count} modelled foods.");
                sb.AppendTable(
                    Model,
                    Model.Records,
                    "MarginOfExposureByModelledFoodTable",
                    ViewBag,
                    caption: "Margin of exposure statistics by modelled food (total distribution).",
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
