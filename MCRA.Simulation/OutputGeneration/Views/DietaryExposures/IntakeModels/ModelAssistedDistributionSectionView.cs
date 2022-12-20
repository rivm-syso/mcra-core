using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class ModelAssistedDistributionSectionView : SectionView<ModelAssistedDistributionSection> {
        public override void RenderSectionHtml(StringBuilder sb) {

            sb.Append("<div class=\"figure-container\">");
            var chartCreator1 = new ModelAssistedChartCreator(Model, ViewBag.GetUnit("IntakeUnit"));
            sb.AppendChart(
                "ModelAssistedChart",
                chartCreator1,
                ChartFileType.Svg,
                Model,
                ViewBag,
                chartCreator1.Title,
                true
            );

            var chartCreator2 = new ModelAssistedCumulativeChartCreator(Model, ViewBag.GetUnit("IntakeUnit"));
            sb.AppendChart(
                "ModelAssistedCumulativeChart",
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
