using System.Text;
using MCRA.Simulation.OutputGeneration.Helpers;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class ExternalContributionBySourceRouteUpperSectionView : SectionView<ExternalContributionBySourceRouteUpperSection> {
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
            sb.AppendParagraph($"Exposure: upper tail {Model.CalculatedUpperPercentage:F1}% ({Model.NumberOfIntakes} records), " +
                $"minimum {Model.LowPercentileValue:G4} {ViewBag.GetUnit("IntakeUnit")}, " +
                $"maximum {Model.HighPercentileValue:G4} {ViewBag.GetUnit("IntakeUnit")}");

            if (Model.ContributionRecords.Count > 1) {
                var chartCreator = new ExternalContributionBySourceRouteUpperPieChartCreator(Model, isUncertainty);
                sb.AppendChart(
                    "UpperDistributionSourceRouteChart",
                    chartCreator,
                    ChartFileType.Svg,
                    Model,
                    ViewBag,
                    chartCreator.Title,
                    true
                );
            } else {
                sb.AppendParagraph("No upper distribution available for specified percentage");
            }
            sb.AppendTable(
                Model,
                Model.ContributionRecords,
                "ExternalExposureBySourceRouteUpperTable",
                ViewBag,
                caption: $"Contributions by source and route for the upper distribution (estimated {Model.CalculatedUpperPercentage:F1}%).",
                saveCsv: true,
                hiddenProperties: hiddenProperties
            );
        }
    }
}
