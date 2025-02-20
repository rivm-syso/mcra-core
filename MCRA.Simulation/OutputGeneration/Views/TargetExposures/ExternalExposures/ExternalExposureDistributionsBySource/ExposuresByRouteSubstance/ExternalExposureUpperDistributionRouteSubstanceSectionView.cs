using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class ExternalExposureUpperDistributionRouteSubstanceSectionView : SectionView<ExternalExposureUpperDistributionRouteSubstanceSection> {
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
                var chartCreator = new ExternalExposureUpperDistributionRouteSubstancePieChartCreator(Model, isUncertainty);
                sb.AppendChart(
                    "ExternalExposureUpperDistributionRouteSubstancePieChart",
                    chartCreator,
                    ChartFileType.Svg,
                    Model,
                    ViewBag,
                    chartCreator.Title,
                    true
                );
                sb.Append($"<p>Exposure: upper tail {Model.CalculatedUpperPercentage:F1}% ({Model.NRecords} records), " +
                    $"minimum {Model.LowPercentileValue:G4} {ViewBag.GetUnit("ExternalExposureUnit").ToHtml()}, " +
                    $"maximum {Model.HighPercentileValue:G4} {ViewBag.GetUnit("ExternalExposureUnit").ToHtml()}</p>");
                sb.AppendTable(
                   Model,
                   Model.Records,
                   "ExternalExposureUpperDistributionRouteSubstanceTable",
                   ViewBag,
                   caption: $"External exposure contributions by route x substance to the upper tail of the distribution (estimated {Model.CalculatedUpperPercentage:F1}%).",
                   saveCsv: true,
                   header: true,
                   hiddenProperties: hiddenProperties
                );
            } else {
                if (Model.UpperPercentage.HasValue) {
                    sb.AppendParagraph("No positive exposures found.");
                } else {
                    sb.AppendParagraph("Upper distribution can not be determined because no relative potency factors are available.");
                }
            }
        }
    }
}
