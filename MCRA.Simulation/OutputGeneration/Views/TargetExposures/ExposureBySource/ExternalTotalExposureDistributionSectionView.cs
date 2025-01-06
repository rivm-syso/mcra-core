using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class ExternalTotalExposureDistributionSectionView : SectionView<ExternalTotalExposureDistributionSection> {
        public override void RenderSectionHtml(StringBuilder sb) {

            if (Model.IntakeDistributionBins.Count > 0) {
                var histogramChartCreator = new ExternalTotalIntakeDistributionChartCreator(Model, ViewBag.GetUnit("ExternalExposureUnit"));
                sb.Append("<div class=\"figure-container\">");
                sb.AppendChart(
                    "ExternalTotalExposureDistributionChart",
                    histogramChartCreator,
                    ChartFileType.Svg,
                    Model,
                    ViewBag,
                    histogramChartCreator.Title,
                    true
                );

                var cumulativeDistributionChartCreator = new ExternalTotalExposureCumulativeDistributionChartCreator(Model, ViewBag.GetUnit("ExternalExposureUnit"));
                sb.AppendChart(
                    "ExternalTotalExposureCumulativeDistributionChart",
                    cumulativeDistributionChartCreator,
                    ChartFileType.Svg,
                    Model,
                    ViewBag,
                    cumulativeDistributionChartCreator.Title,
                    true
                );
                sb.Append("</div>");
            } else {
                sb.AppendParagraph("No external exposure distribution available.");
            }
        }
    }
}
