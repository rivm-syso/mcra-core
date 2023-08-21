using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class HazardExposureRatioDistributionSectionView : SectionView<HazardExposureRatioDistributionSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            
            if (Model.Threshold == 1) {
                sb.AppendParagraph($"Nominal Probability of Critical Exposure (POCE) {Model.ProbabilityOfCriticalEffect:G2} %");
            } else {
                sb.AppendParagraph($"Nominal Probability of Critical Exposure (POCE) relative to threshold {Model.Threshold:G2} = {Model.ProbabilityOfCriticalEffect:G2} %");
            }

            sb.Append("<div class=\"figure-container\">");
            var chartCreator = new HazardExposureRatioChartCreator(Model);
            sb.AppendChart(
                "MarginOfExposureDistributionChart",
                chartCreator,
                ChartFileType.Svg,
                Model,
                ViewBag,
                chartCreator.Title,
                true
            );

            var chartCreator1 = new HazardExposureRatioCumulativeChartCreator(Model);
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
