using System.Text;
using MCRA.Simulation.OutputGeneration.Helpers;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class ExposureEffectFunctionSummarySectionView : SectionView<ExposureEffectFunctionSummarySection> {
        public override void RenderSectionHtml(StringBuilder sb) {

            sb.AppendLine(Model.ExposureEffectFunction.Expression.ExpressionString);

            var chartCreator = new ExposureEffectFunctionChartCreator(Model);
            sb.AppendChart(
                "ExposureEffectFunctionChart",
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
