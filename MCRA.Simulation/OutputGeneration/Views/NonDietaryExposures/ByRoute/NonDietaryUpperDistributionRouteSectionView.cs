using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class NonDietaryUpperDistributionRouteSectionView : SectionView<NonDietaryUpperDistributionRouteSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var isUncertainty = Model.NonDietaryUpperDistributionRouteRecords.Count > 0 &&  Model.NonDietaryUpperDistributionRouteRecords.First().Contributions.Count > 0;
            var hiddenProperties = new List<string>();
            if (!isUncertainty) {
                hiddenProperties.Add("LowerContributionPercentage");
                hiddenProperties.Add("UpperContributionPercentage");
                hiddenProperties.Add("MeanContribution");
            } else {
                hiddenProperties.Add("ContributionPercentage");
            }
            //Render HTML
            if (Model.NonDietaryUpperDistributionRouteRecords.Count > 0) {
                var chartCreator = new NonDietaryUpperDistributionRoutePieChartCreator(Model, isUncertainty);
                sb.AppendChart(
                    "NonDietaryUpperDistributionRoutePieChart",
                    chartCreator,
                    ChartFileType.Svg,
                    Model,
                    ViewBag,
                    chartCreator.Title,
                    true
                );

                sb.AppendParagraph("Absorption factors are not used");
                sb.Append($"<p>Exposure: upper percentage {Model.UpperPercentage:F2} % ({Model.NRecords} records), " +
                    $"minimum {Model.LowPercentileValue:G4} {ViewBag.GetUnit("IntakeUnit").ToHtml()}, " +
                    $"maximum {Model.HighPercentileValue:G4} {ViewBag.GetUnit("IntakeUnit").ToHtml()}</p>");
                sb.AppendTable(
                   Model,
                   Model.NonDietaryUpperDistributionRouteRecords,
                   "NonDietaryUpperDistributionRouteTable",
                   ViewBag,
                   caption: "NonDietary upper distribution per route.",
                   saveCsv: true,
                   header: true,
                   hiddenProperties: hiddenProperties
                );
            } else {
                sb.AppendParagraph("No non-dietary upper exposure distribution available");
            }
        }
    }
}
