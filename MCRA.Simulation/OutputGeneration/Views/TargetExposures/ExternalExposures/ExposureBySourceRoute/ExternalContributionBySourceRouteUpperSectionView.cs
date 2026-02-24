using System.Text;
using MCRA.Simulation.OutputGeneration.Helpers;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class ExternalContributionBySourceRouteUpperSectionView : SectionView<ExternalContributionBySourceRouteUpperSection> {
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
            var individualString = Model.NumberOfIntakes == 1 ? $"1 individual" : $"{Model.NumberOfIntakes} individuals";
            sb.AppendParagraph($"Exposure: upper tail {Model.CalculatedUpperPercentage:F1}% ({individualString}), " +
                $"minimum {Model.LowPercentileValue:G4} {ViewBag.GetUnit("IntakeUnit")}, " +
                $"maximum {Model.HighPercentileValue:G4} {ViewBag.GetUnit("IntakeUnit")}");

            if (Model.Records.Count > 1) {
                var chartCreator = new ExternalContributionBySourceRouteUpperPieChartCreator(Model, isUncertainty);
                sb.AppendChart(
                    "ExternalUpperDistributionSourceRouteChart",
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
                Model.Records,
                "ExternalExposureBySourceRouteUpperTable",
                ViewBag,
                caption: $"Contributions by source and route for the upper distribution (estimated {Model.CalculatedUpperPercentage:F1}%).",
                saveCsv: true,
                hiddenProperties: hiddenProperties
            );
        }
    }
}
