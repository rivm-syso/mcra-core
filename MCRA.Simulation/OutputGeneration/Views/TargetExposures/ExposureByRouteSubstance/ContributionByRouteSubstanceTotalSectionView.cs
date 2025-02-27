using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class ContributionByRouteSubstanceTotalSectionView : SectionView<ContributionByRouteSubstanceTotalSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var hiddenProperties = new List<string>();
            var isUncertainty = Model.Records.First().Contributions.Count > 0;
            if (!isUncertainty) {
                hiddenProperties.Add("LowerContributionPercentage");
                hiddenProperties.Add("UpperContributionPercentage");
                hiddenProperties.Add("MeanContribution");
            } else {
                hiddenProperties.Add("ContributionPercentage");
            }

            var records = Model.Records.Where(r => !double.IsNaN(r.ContributionPercentage)).ToList();
            if (records.All(r => double.IsNaN(r.ContributionPercentage))) {
                hiddenProperties.Add("ContributionPercentage");
                hiddenProperties.Add("Contribution");
                hiddenProperties.Add("MeanContribution");
                hiddenProperties.Add("RelativePotencyFactor");
                hiddenProperties.Add("LowerContributionPercentage");
                hiddenProperties.Add("UpperContributionPercentage");
            }
            if (records.Count > 0) {
                var chartCreator = new ContributionByRouteSubstanceTotalPieChartCreator(Model, isUncertainty);
                sb.AppendChart(
                    "TotalDistributionRouteSubstanceChart",
                    chartCreator,
                    ChartFileType.Svg,
                    Model,
                    ViewBag,
                    chartCreator.Title,
                    true
                );
                sb.AppendTable(
                    Model,
                    records,
                    "TotalDistributionRouteSubstanceTable",
                    ViewBag,
                    caption: "Contributions by route and substance for the total distribution.",
                    saveCsv: true,
                    hiddenProperties: hiddenProperties
                );
            } else {
                sb.AppendParagraph("No positive exposures found.");
            }
        }
    }
}
