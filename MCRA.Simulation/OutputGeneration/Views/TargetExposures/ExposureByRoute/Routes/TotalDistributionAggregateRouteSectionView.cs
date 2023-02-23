using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class TotalDistributionAggregateRouteSectionView : SectionView<TotalDistributionAggregateRouteSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var hiddenProperties = new List<string>();
            var isUncertainty = Model.DistributionRouteTotalRecords.First().Contributions.Count > 0;
            if (!isUncertainty) {
                hiddenProperties.Add("LowerContributionPercentage");
                hiddenProperties.Add("UpperContributionPercentage");
                hiddenProperties.Add("MeanContribution");
            } else {
                hiddenProperties.Add("ContributionPercentage");
            }
            //Render HTML
            if (Model.DistributionRouteTotalRecords.Where(r => !double.IsNaN(r.ContributionPercentage)).Count() > 1) {
                var chartCreator = new TotalDistributionAggregateRoutePieChartCreator(Model, isUncertainty);
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
                Model.DistributionRouteTotalRecords, 
                "TotalDistributionAggregateRouteTable", 
                ViewBag, 
                caption: "Exposure statistics by route (total distribution).",
                saveCsv: true,
                hiddenProperties: hiddenProperties
            );
        }
    }
}
