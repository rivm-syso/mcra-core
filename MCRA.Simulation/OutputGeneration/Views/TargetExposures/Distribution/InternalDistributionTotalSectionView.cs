using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class InternalDistributionTotalSectionView : SectionView<InternalDistributionTotalSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            bool showUncertainty = !Model.Percentiles.All(p => double.IsNaN(p.MedianUncertainty));

            //Render HTML
            if (Model.PercentageZeroIntake < 100) {
                sb.Append("<div class=\"figure-container\">");
                {
                    var chartCreator = new InternalChronicDistributionTotalChartCreator(Model, ViewBag.GetUnit("IntakeUnit"));
                    sb.AppendChart(
                        "AggregateTotalIntakeDistributionChart",
                        chartCreator,
                        ChartFileType.Svg,
                        Model,
                        ViewBag,
                        chartCreator.Title,
                        true
                    );
                }
                {
                    var chartCreator = new InternalCumulativeDistributionChartCreator(Model, ViewBag.GetUnit("IntakeUnit"));
                    sb.AppendChart(
                        "AggregateTotalIntakeCumulativeDistributionChart",
                        chartCreator,
                        ChartFileType.Svg,
                        Model,
                        ViewBag,
                        chartCreator.Title,
                        true
                    );
                }
                sb.Append("</div>");
            } else {
                sb.AppendParagraph("No positive exposures!", "warning");
            }
            if ((Model.CategorizedHistogramBins?.Count ?? 0) > 1) {
                var chartCreator = new InternalChronicStackedHistogramChartCreator(Model, ViewBag.GetUnit("IntakeUnit"));
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
