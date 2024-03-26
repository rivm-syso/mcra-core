using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class NonDietaryTotalDistributionRouteCompoundSectionView : SectionView<NonDietaryTotalDistributionRouteCompoundSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var isUncertainty = Model.Records.Count > 0 && Model.Records.First().Contributions.Count > 0;
            var hiddenProperties = new List<string>();
            if (!isUncertainty) {
                hiddenProperties.Add("LowerContributionPercentage");
                hiddenProperties.Add("UpperContributionPercentage");
                hiddenProperties.Add("MeanContribution");
            } else {
                hiddenProperties.Add("ContributionPercentage");
            }
            //Render HTML
            if (Model.Records.Count > 0) {
                var chartCreator = new NonDietaryTotalDistributionRouteCompoundPieChartCreator(Model, isUncertainty);
                sb.AppendChart(
                    "NonDietaryTotalDistributionRouteCompoundPieChart",
                    chartCreator,
                    ChartFileType.Svg,
                    Model,
                    ViewBag,
                    chartCreator.Title,
                    true
                );

                sb.AppendParagraph("Relative potency and absorption factors are not used");
                sb.AppendTable(
                   Model,
                   Model.Records,
                   "TotalDistributionNonDietaryRouteSubstanceTable",
                   ViewBag,
                   caption: "Non-dietary total distribution route by substance.",
                   saveCsv: true,
                   header: true,
                   hiddenProperties: hiddenProperties
                );
            } else {
                sb.AppendParagraph("No non-dietary exposure distribution available.");
            }
        }
    }
}
