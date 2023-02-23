using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class AggregateTotalIntakeDistributionSectionView : SectionView<AggregateTotalIntakeDistributionSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            bool showUncertainty = !Model.Percentiles.All(p => double.IsNaN(p.MedianUncertainty));

            //Render HTML
            if (Model.PercentageZeroIntake < 100) {
                sb.Append("<div class=\"figure-container\">");
                var chartCreator1 = new AggregateTotalIntakeDistributionChartCreator(Model, ViewBag.GetUnit("IntakeUnit"));
                sb.AppendChart(
                    "AggregateTotalIntakeDistributionChart",
                    chartCreator1,
                    ChartFileType.Svg,
                    Model,
                    ViewBag,
                    chartCreator1.Title,
                    true
                );

                var chartCreator2 = new AggregateTotalIntakeCumulativeDistributionChartCreator(Model, ViewBag.GetUnit("IntakeUnit"));
                sb.AppendChart(
                    "AggregateTotalIntakeCumulativeDistributionChart",
                    chartCreator2,
                    ChartFileType.Svg,
                    Model,
                    ViewBag,
                    chartCreator2.Title,
                    true
                );
                sb.Append("</div>");
            } else {
                sb.AppendParagraph("No positive exposures!", "warning");
            }
            if ((Model.AcuteCategorizedHistogramBins?.Count() ?? 0) > 1) {
                var chartCreator = new AcuteTotalStackedHistogramChartCreator(Model, ViewBag.GetUnit("IntakeUnit"));
                sb.AppendChart(
                    "AcuteTotalStackedHistogramChart",
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
