using System.Text;
using MCRA.Simulation.OutputGeneration.Helpers;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class ExternalContributionBySourceRouteTotalSectionView : SectionView<ExternalContributionBySourceRouteTotalSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var hiddenProperties = new List<string>();
            var isUncertainty = Model.Records.First().Contributions.Count > 0;
            if (!isUncertainty) {
                hiddenProperties.Add(nameof(ExternalContributionBySourceRouteRecord.LowerContributionPercentage));
                hiddenProperties.Add(nameof(ExternalContributionBySourceRouteRecord.UpperContributionPercentage));
                hiddenProperties.Add(nameof(ExternalContributionBySourceRouteRecord.MeanContribution));
            } else {
                hiddenProperties.Add(nameof(ExternalContributionBySourceRouteRecord.ContributionPercentage));
            }
            if (Model.Records.Count > 1) {
                var chartCreator = new ExternalContributionBySourceRouteTotalPieChartCreator(Model, isUncertainty);
                sb.AppendChart(
                    "ExternalTotalDistributionSourceRouteChart",
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
                "ExternalExposureBySourceRouteTotalTable",
                ViewBag,
                caption: "Contributions by source and route (total distribution).",
                saveCsv: true,
                hiddenProperties: hiddenProperties
            );
        }
    }
}
