using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class OIMDistributionSectionView : SectionView<OIMDistributionSection> {
        public override void RenderSectionHtml(StringBuilder sb) {

            //Render HTML
            if (!Model.IsTotalDistribution) {
                sb.AppendDescriptionParagraph($"Exposure: upper percentage {Model.UpperPercentage:F1} % ({Model.NRecords} records), " +
                    $"minimum {Model.LowPercentileValue:G4} {ViewBag.GetUnit("IntakeUnit")}, " +
                    $"maximum {Model.HighPercentileValue:G4} {ViewBag.GetUnit("IntakeUnit")}, " +
                    $"(estimated {Model.CalculatedUpperPercentage:F1}%).");
            }

            if (Model.IsTotalDistribution) {
                sb.Append("<div class=\"figure-container\">");
                var distributionChartCreator = new OIMDistributionChartCreator(Model, ViewBag.GetUnit("IntakeUnit"));
                sb.AppendChart(
                        "TotalDistributionChart",
                        distributionChartCreator,
                        ChartFileType.Svg,
                        Model,
                        ViewBag,
                        distributionChartCreator.Title,
                        true
                    );
                var cumulativeDistributionChartCreator = new OIMCumulativeChartCreator(Model, ViewBag.GetUnit("IntakeUnit"));
                sb.AppendChart(
                        "TotalCumulativeDistributionChart",
                        cumulativeDistributionChartCreator,
                        ChartFileType.Svg,
                        Model,
                        ViewBag,
                        cumulativeDistributionChartCreator.Title,
                        true
                    );
                sb.Append("</div>");
            } else {
                var chartCreator = new OIMDistributionChartCreator(Model, ViewBag.GetUnit("IntakeUnit"));
                sb.AppendChart(
                        "UpperDistributionChart",
                        chartCreator,
                        ChartFileType.Svg,
                        Model,
                        ViewBag,
                        chartCreator.Title,
                        true
                    );
            }

            if (Model.IsAggregate && (Model.CategorizedHistogramBins?.Count ?? 0) > 1) {
                var chartCreator = new InternalOIMStackedHistogramChartCreator(Model, ViewBag.GetUnit("IntakeUnit"));
                sb.AppendChart(
                        "AggregateHistogramChart",
                        chartCreator,
                        ChartFileType.Svg,
                        Model,
                        ViewBag,
                        chartCreator.Title,
                        true
                    );
            }
        }
    }
}
