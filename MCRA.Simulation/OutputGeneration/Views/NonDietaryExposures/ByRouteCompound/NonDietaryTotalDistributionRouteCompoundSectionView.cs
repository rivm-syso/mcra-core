using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class NonDietaryTotalDistributionRouteCompoundSectionView : SectionView<NonDietaryTotalDistributionRouteCompoundSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var isUncertainty = Model.NonDietaryTotalDistributionRouteCompoundRecords.Count > 0 && Model.NonDietaryTotalDistributionRouteCompoundRecords.First().Contributions.Count > 0;
            var hiddenProperties = new List<string>();
            if (!isUncertainty) {
                hiddenProperties.Add("LowerContributionPercentage");
                hiddenProperties.Add("UpperContributionPercentage");
                hiddenProperties.Add("MeanContribution");
            } else {
                hiddenProperties.Add("ContributionPercentage");
            }
            //Render HTML
            if (Model.NonDietaryTotalDistributionRouteCompoundRecords.Count > 0) {
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
                   Model.NonDietaryTotalDistributionRouteCompoundRecords,
                   "TotalDistributionNonDietaryRouteSubstanceTable",
                   ViewBag,
                   caption: "NonDietary total distribution route by substance.",
                   saveCsv: true,
                   header: true,
                   hiddenProperties: hiddenProperties
                );
            } else {
                sb.AppendParagraph("No non-dietary exposure distribution available");
            }
        }
    }
}
