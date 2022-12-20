using MCRA.Simulation.OutputGeneration.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class UpperDistributionAggregateRouteSectionView : SectionView<UpperDistributionAggregateRouteSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var hiddenProperties = new List<string>();
            var isUncertainty = Model.DistributionRouteUpperRecords.First().Contributions.Count > 0;
            if (!isUncertainty) {
                hiddenProperties.Add("LowerContributionPercentage");
                hiddenProperties.Add("UpperContributionPercentage");
                hiddenProperties.Add("MeanContribution");
            } else {
                hiddenProperties.Add("ContributionPercentage");
            }
            //Render HTML
            sb.AppendParagraph($"Upper percentage {Model.UpperPercentage:F2} % ({Model.NRecords} records), " +
                $"minimum {Model.LowPercentileValue:G4} {ViewBag.GetUnit("IntakeUnit")}, " +
                $"maximum {Model.HighPercentileValue:G4} {ViewBag.GetUnit("IntakeUnit")}");

            if (Model.DistributionRouteUpperRecords.Count > 0) {
                var chartCreator = new UpperDistributionAggregateRoutePieChartCreator(Model, isUncertainty);
                sb.AppendChart(
                    "UpperDistributionRouteChart",
                    chartCreator,
                    ChartFileType.Svg,
                    Model,
                    ViewBag,
                    chartCreator.Title,
                    true
                );

                sb.AppendTable(
                    Model,
                    Model.DistributionRouteUpperRecords,
                    "UpperDistributionRouteTable",
                    ViewBag,
                    caption: "Contribution and exposure statistics by route (upper tail distribution).",
                    saveCsv: true,
                    hiddenProperties: hiddenProperties
                );
            } else {
                sb.AppendParagraph("No upper distribution available for specified percentage");
            }
        }
    }
}
