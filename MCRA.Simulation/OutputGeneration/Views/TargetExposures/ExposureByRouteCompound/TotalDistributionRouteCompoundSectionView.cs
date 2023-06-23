using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class TotalDistributionRouteCompoundSectionView : SectionView<TotalDistributionRouteCompoundSection> {
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
            if (Model.Records.All(r => double.IsNaN(r.ContributionPercentage))) {
                hiddenProperties.Add("ContributionPercentage");
                hiddenProperties.Add("Contribution");
                hiddenProperties.Add("MeanContribution");
                hiddenProperties.Add("RelativePotencyFactor");
                hiddenProperties.Add("LowerContributionPercentage");
                hiddenProperties.Add("UpperContributionPercentage");
            }
            //Render HTML
            if (Model.Records.Count > 1 && !Model.Records.All(r => double.IsNaN(r.ContributionPercentage))) {
                var chartCreator = new TotalDistributionRouteCompoundPieChartCreator(Model, isUncertainty);
                sb.AppendChart(
                    "TotalDistributionRouteSubstanceChart",
                    chartCreator,
                    ChartFileType.Svg,
                    Model,
                    ViewBag,
                    chartCreator.Title,
                    true
                );
            }
            sb.AppendTable(
                Model,
                Model.Records,
                "TotalDistributionRouteSubstanceTable",
                ViewBag,
                caption: "Contribution and exposure statistics by route x substance (total distribution), RPFs are not applied except for exposure contribution.",
                saveCsv: true,
                hiddenProperties: hiddenProperties
            );
        }
    }
}
