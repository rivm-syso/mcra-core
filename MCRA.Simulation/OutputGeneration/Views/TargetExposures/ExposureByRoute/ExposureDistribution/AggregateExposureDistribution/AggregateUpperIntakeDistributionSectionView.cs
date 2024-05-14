using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class AggregateUpperIntakeDistributionSectionView : SectionView<AggregateUpperIntakeDistributionSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            bool showUncertainty = !Model.Percentiles.All(p => double.IsNaN(p.MedianUncertainty));

            //Render HTML
            sb.Append("<div class=\"figure-container\">");
            if (Model.PercentageZeroIntake < 100) {
                sb.AppendParagraph($"Exposure: upper tail {Model.UpperPercentage:F1}% (estimated {Model.CalculatedUpperPercentage:F1})");
                var chartCreator = new AggregateUpperIntakeDistributionChartCreator(Model, ViewBag.GetUnit("IntakeUnit"));
                sb.AppendChart(
                    "AggregateUpperIntakeDistributionChart",
                    chartCreator,
                    ChartFileType.Svg,
                    Model,
                    ViewBag,
                    chartCreator.Title,
                    true
                );
            } else {
                sb.AppendParagraph("No positive exposures!", "warning");
            }
            if ((Model.CategorizedHistogramBins?.Count ?? 0) > 1) {
                var chartCreator = new AcuteUpperStackedHistogramChartCreator(Model, ViewBag.GetUnit("IntakeUnit"));
                sb.AppendChart(
                    "AcuteUpperStackedHistogramChart",
                    chartCreator,
                    ChartFileType.Svg,
                    Model,
                    ViewBag,
                    chartCreator.Title,
                    true
                );
            }
            sb.Append("</div>");
        }
    }
}
