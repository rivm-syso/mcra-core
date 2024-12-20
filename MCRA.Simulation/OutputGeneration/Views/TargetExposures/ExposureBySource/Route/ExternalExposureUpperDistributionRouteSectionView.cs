using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class ExternalExposureUpperDistributionRouteSectionView : SectionView<ExternalExposureUpperDistributionRouteSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var isUncertainty = Model.Records.Count > 0 &&  Model.Records.First().Contributions.Count > 0;
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
                var chartCreator = new ExternalExposureUpperDistributionRoutePieChartCreator(Model, isUncertainty);
                sb.AppendChart(
                    "ExternalExposureUpperDistributionRoutePieChart",
                    chartCreator,
                    ChartFileType.Svg,
                    Model,
                    ViewBag,
                    chartCreator.Title,
                    true
                );

                sb.AppendParagraph("Absorption factors are not used");
                sb.Append($"<p>Exposure: upper {Model.CalculatedUpperPercentage:F1}% ({Model.NRecords} records), " +
                    $"minimum {Model.LowPercentileValue:G4} {ViewBag.GetUnit("ExternalExposureUnit").ToHtml()}, " +
                    $"maximum {Model.HighPercentileValue:G4} {ViewBag.GetUnit("ExternalExposureUnit").ToHtml()}</p>");
                sb.AppendTable(
                   Model,
                   Model.Records,
                   "ExternalExposureUpperDistributionRouteTable",
                   ViewBag,
                   caption: $"External exposure contributions per route of the upper tail of the distribution (estimated {Model.CalculatedUpperPercentage:F1}%).",
                   saveCsv: true,
                   header: true,
                   hiddenProperties: hiddenProperties
                );
            } else {
                sb.AppendParagraph("No external exposure upper exposure distribution available");
            }
        }
    }
}
