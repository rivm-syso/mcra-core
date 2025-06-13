using System.Text;
using MCRA.Simulation.OutputGeneration.Helpers;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class ConsumerProductExposuresByRouteSectionView : SectionView<ConsumerProductExposuresByRouteSection> {
        public override void RenderSectionHtml(StringBuilder sb) {

            var hiddenProperties = new List<string>();

            if (Model.Records.All(c => c.NumberOfSubstances <= 1)) {
                hiddenProperties.Add("NumberOfSubstances");
            }

            var isUncertainty = Model.Records.FirstOrDefault()?.Contributions.Any() ?? false;
            if (!isUncertainty) {
                hiddenProperties.Add("LowerContributionPercentage");
                hiddenProperties.Add("UpperContributionPercentage");
                hiddenProperties.Add("MeanContribution");
            } else {
                hiddenProperties.Add("ContributionPercentage");
            }


            //Render HTML
            if (Model.Records.Any(r => r.Total > 0)) {
                sb.AppendDescriptionParagraph($"Total {Model.Records.Count} routes.");


                if (Model.Records.Count > 1) {
                    var chartCreator = new ConsumerProductExposuresByRoutePieChartCreator(Model, isUncertainty);
                    sb.AppendChart(
                        "ConsumerProductExposureByRouteChart",
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
                    "ConsumerProductExposureByRouteTable",
                    ViewBag,
                    caption: "Exposure statistics by route (total distribution).",
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
