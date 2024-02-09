using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class HazardExposureRatioSubstanceSectionView : SectionView<HazardExposureRatioSubstanceSection> {
        public override void RenderSectionHtml(StringBuilder sb) {

            var hiddenProperties = new List<string>();

            var isUncertainty = Model.Records.FirstOrDefault()?.Contributions.Any() ?? false;
            if (!isUncertainty) {
                hiddenProperties.Add("LowerContributionPercentage");
                hiddenProperties.Add("UpperContributionPercentage");
                hiddenProperties.Add("MeanContribution");
            } else {
                hiddenProperties.Add("ContributionPercentage");
            }
            hiddenProperties.Add("MeanAll");

            //Render HTML
            if (Model.Records.Any()) {
                var chartCreator = new DistributionRiskHEDriversPieChartCreator(Model, null, isUncertainty);
                sb.AppendChart(
                    "RiskContributionsBySubstanceTotalChart",
                    chartCreator,
                    ChartFileType.Svg,
                    Model,
                    ViewBag,
                    chartCreator.Title,
                    true
                );

                sb.AppendDescriptionParagraph($"Total distribution {Model.Records.Count} substances.");
                sb.AppendTable(
                    Model,
                    Model.Records,
                    "RiskContributionsBySubstanceTotalTable",
                    ViewBag,
                    caption: $"Risk contributions by substance (total distribution).",
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
