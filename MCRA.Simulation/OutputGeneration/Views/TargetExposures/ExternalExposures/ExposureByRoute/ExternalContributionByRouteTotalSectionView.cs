using System.Text;
using MCRA.Simulation.OutputGeneration.Helpers;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class ExternalContributionByRouteTotalSectionView : SectionView<ExternalContributionByRouteTotalSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var hiddenProperties = new List<string>();
            var isUncertainty = Model.Records.First().Contributions.Count > 0;
            if (!isUncertainty) {
                hiddenProperties.Add(nameof(ExternalContributionByRouteRecord.LowerContributionPercentage));
                hiddenProperties.Add(nameof(ExternalContributionByRouteRecord.UpperContributionPercentage));
                hiddenProperties.Add(nameof(ExternalContributionByRouteRecord.MeanContribution));
            } else {
                hiddenProperties.Add(nameof(ExternalContributionByRouteRecord.ContributionPercentage));
            }
            if (Model.Records.Count > 1) {
                var chartCreator = new ExternalContributionByRouteTotalPieChartCreator(Model, isUncertainty);
                sb.AppendChart(
                    "ExternalTotalDistributionRouteChart",
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
                "ExternalExposureByRouteTotalTable",
                ViewBag,
                caption: "Contributions by route (total distribution).",
                saveCsv: true,
                hiddenProperties: hiddenProperties
            );
        }
    }
}
