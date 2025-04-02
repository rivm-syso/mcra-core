using System.Text;
using MCRA.Simulation.OutputGeneration.Helpers;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class ContributionBySourceRouteSubstanceTotalSectionView : SectionView<ContributionBySourceRouteSubstanceTotalSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var hiddenProperties = new List<string>();
            var isUncertainty = Model.Records.First().Contributions.Count > 0;
            if (!isUncertainty) {
                hiddenProperties.Add("LowerContributionPercentage");
                hiddenProperties.Add("UpperContributionPercentage");
                hiddenProperties.Add("MeanContribution");
            } else {
                hiddenProperties.Add("ContributionPercentage");
            }
            if (Model.Records.Count(r => !double.IsNaN(r.ContributionPercentage)) > 1) {
                var chartCreator = new ContributionBySourceRouteSubstanceTotalPieChartCreator(Model, isUncertainty);
                sb.AppendChart(
                    "TotalDistributionSourceRouteSubstanceChart",
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
                "ExposureBySourceRouteSubstanceTotalTable",
                ViewBag,
                caption: "Contributions by source, route and substance (total distribution).",
                saveCsv: true,
                hiddenProperties: hiddenProperties
            );
        }
    }
}
