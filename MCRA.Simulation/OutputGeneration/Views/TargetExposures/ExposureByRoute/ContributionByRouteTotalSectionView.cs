using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class ContributionByRouteTotalSectionView : SectionView<ContributionByRouteTotalSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var hiddenProperties = new List<string>();
            var isUncertainty = Model.ContributionRecords.First().Contributions.Count > 0;
            if (!isUncertainty) {
                hiddenProperties.Add("LowerContributionPercentage");
                hiddenProperties.Add("UpperContributionPercentage");
                hiddenProperties.Add("MeanContribution");
            } else {
                hiddenProperties.Add("ContributionPercentage");
            }
            //Render HTML
            if (Model.ContributionRecords.Count(r => !double.IsNaN(r.ContributionPercentage)) > 1) {
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
                Model.ContributionRecords,
                "ExternalExposureByRouteTotalTable",
                ViewBag,
                caption: "Contributions by route (total distribution).",
                saveCsv: true,
                hiddenProperties: hiddenProperties
            );
        }
    }
}
