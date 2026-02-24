using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class ContributionByRouteTotalSectionView : SectionView<ContributionByRouteTotalSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var hiddenProperties = new List<string>();
            var isUncertainty = Model.Records.First().Contributions.Count > 0;
            if (!isUncertainty) {
                hiddenProperties.Add(nameof(ContributionByRouteRecord.LowerContributionPercentage));
                hiddenProperties.Add(nameof(ContributionByRouteRecord.UpperContributionPercentage));
                hiddenProperties.Add(nameof(ContributionByRouteRecord.MeanContribution));
            } else {
                hiddenProperties.Add(nameof(ContributionByRouteRecord.ContributionPercentage));
            }
            if (Model.Records.Count(r => !double.IsNaN(r.ContributionPercentage)) > 1) {
                var chartCreator = new ContributionByRouteTotalPieChartCreator(Model, isUncertainty);
                sb.AppendChart(
                    "TotalDistributionRouteChart",
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
                "ExposureByRouteTotalTable",
                ViewBag,
                caption: "Contributions by route (total distribution).",
                saveCsv: true,
                hiddenProperties: hiddenProperties
            );
        }
    }
}
