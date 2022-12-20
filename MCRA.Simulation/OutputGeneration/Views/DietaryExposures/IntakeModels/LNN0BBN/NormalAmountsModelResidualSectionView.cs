using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class NormalAmountsModelResidualSectionView : SectionView<NormalAmountsModelResidualSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            //Render HTML
            if (Model.Residuals.Contains(double.NaN)) {
                sb.AppendParagraph("No diagnostic plots available");
            } else {
                sb.Append("<div class=\"figure-container\">");
                var chartCreator1 = new EmpiricalDistributionResidualsChartCreator(Model);
                sb.AppendChart(
                    "EmpiricalDistributionResidualsChart",
                    chartCreator1,
                    ChartFileType.Svg,
                    Model,
                    ViewBag,
                    chartCreator1.Title,
                    true
                );

                var chartCreator2 = new EmpiricalResidualsQQChartCreator(Model);
                sb.AppendChart(
                    "EmpiricalResidualsQQChart",
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
}
