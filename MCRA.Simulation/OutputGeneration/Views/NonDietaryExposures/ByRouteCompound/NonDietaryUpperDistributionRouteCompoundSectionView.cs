using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class NonDietaryUpperDistributionRouteCompoundSectionView : SectionView<NonDietaryUpperDistributionRouteCompoundSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var isUncertainty = Model.NonDietaryUpperDistributionRouteCompoundRecords.Count > 0 && Model.NonDietaryUpperDistributionRouteCompoundRecords.First().Contributions.Count > 0;
            var hiddenProperties = new List<string>();
            if (!isUncertainty) {
                hiddenProperties.Add("LowerContributionPercentage");
                hiddenProperties.Add("UpperContributionPercentage");
                hiddenProperties.Add("MeanContribution");
            } else {
                hiddenProperties.Add("ContributionPercentage");
            }
            //Render HTML
            if (Model.NonDietaryUpperDistributionRouteCompoundRecords.Count > 0) {
                var chartCreator = new NonDietaryUpperDistributionRouteCompoundPieChartCreator(Model, isUncertainty);
                sb.AppendChart(
                    "NonDietaryUpperDistributionRouteCompoundPieChart",
                    chartCreator,
                    ChartFileType.Svg,
                    Model,
                    ViewBag,
                    chartCreator.Title,
                    true
                );
                sb.Append($"<p>Exposure: upper percentage {Model.UpperPercentage:F2} % ({Model.NRecords} records), " +
                    $"minimum {Model.LowPercentileValue:G4} {ViewBag.GetUnit("IntakeUnit").ToHtml()}, " +
                    $"maximum {Model.HighPercentileValue:G4} {ViewBag.GetUnit("IntakeUnit").ToHtml()}</p>");
                sb.AppendTable(
                   Model,
                   Model.NonDietaryUpperDistributionRouteCompoundRecords,
                   "NonDietaryUpperDistributionRouteSubstanceTable",
                   ViewBag,
                   caption: "NonDietary upper distribution route by substance.",
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
