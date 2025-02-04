using System.Text;
using MCRA.Simulation.OutputGeneration.Helpers;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class ExternalExposureTotalDistributionRouteSubstanceSectionView : SectionView<ExternalExposureTotalDistributionRouteSubstanceSection> {
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
            if (Model.Records.All(r => double.IsNaN(r.ContributionPercentage))) {
                hiddenProperties.Add("ContributionPercentage");
                hiddenProperties.Add("RelativePotencyFactor");
            }
            //Render HTML
            if (Model.Records.Count > 0) {
                if (!Model.Records.All(r => double.IsNaN(r.ContributionPercentage))) {
                    var chartCreator = new ExternalExposureTotalDistributionRouteSubstancePieChartCreator(Model, isUncertainty);
                    sb.AppendChart(
                        "ExternalExposureTotalDistributionRouteSubstancePieChart",
                        chartCreator,
                        ChartFileType.Svg,
                        Model,
                        ViewBag,
                        chartCreator.Title,
                        true
                    );
                }
                sb.AppendParagraph("Relative potency and absorption factors are not used");
                sb.AppendTable(
                   Model,
                   Model.Records,
                   "TotalDistributionExternalExposureRouteSubstanceTable",
                   ViewBag,
                   caption: "External exposure total distribution route by substance.",
                   saveCsv: true,
                   header: true,
                   hiddenProperties: hiddenProperties
                );
            } else {
                sb.AppendParagraph("No external exposure distribution available.");
            }
        }
    }
}
