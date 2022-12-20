using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class ModelBasedDistributionSectionView : SectionView<ModelBasedDistributionSection> {
        public override void RenderSectionHtml(StringBuilder sb) {

            //Render HTML
            sb.Append("<div class=\"figure-container\">");
            var chartCreator1 = new ModelBasedChartCreator(Model, ViewBag.GetUnit("IntakeUnit"));
            sb.AppendChart(
                "ModelBasedUsualExposureDistributionChart",
                chartCreator1,
                ChartFileType.Svg,
                Model,
                ViewBag,
                chartCreator1.Title,
                true
            );

            var chartCreator2 = new ModelBasedCumulativeChartCreator(Model, ViewBag.GetUnit("IntakeUnit"));
            sb.AppendChart(
                "ModelBasedCumulativeExposureDistributionChart",
                chartCreator2,
                ChartFileType.Svg,
                Model,
                ViewBag,
                chartCreator2.Title,
                true
            );
            sb.Append("</div>");
        }
    }
}
