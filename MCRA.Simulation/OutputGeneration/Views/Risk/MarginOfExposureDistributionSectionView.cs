using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class MarginOfExposureDistributionSectionView : SectionView<MarginOfExposureDistributionSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            
            if (Model.ThresholdMarginOfExposure == 1) {
                sb.AppendParagraph($"Nominal Probability of Critical Exposure (POCE) {Model.ProbabilityOfCriticalEffect:G2} %");
            } else {
                sb.AppendParagraph($"Nominal Probability of Critical Exposure (POCE) relative to threshold {Model.ThresholdMarginOfExposure:G2} = {Model.ProbabilityOfCriticalEffect:G2} %");
            }

            sb.Append("<div class=\"figure-container\">");
            var chartCreator = new MarginOfExposureChartCreator(Model);
            sb.AppendChart(
                "MarginOfExposureDistributionChart",
                chartCreator,
                ChartFileType.Svg,
                Model,
                ViewBag,
                chartCreator.Title,
                true
            );

            var chartCreator1 = new MarginOfExposureCumulativeChartCreator(Model);
            sb.AppendChart(
                "MarginOfExposureCumulativeChart",
                chartCreator1,
                ChartFileType.Svg,
                Model,
                ViewBag,
                chartCreator1.Title,
                true
            );
            sb.Append("</div>");
        }
    }
}
