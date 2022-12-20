using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class UsualIntakeDistributionPerCategoryModelSectionView : SectionView<UsualIntakeDistributionPerCategoryModelSection> {
        public override void RenderSectionHtml(StringBuilder sb) {

            //Render HTML
            sb.Append("<div class=\"figure-container\">");
            var chartCreator1 = new MtaGroupingDistributionChartCreator(Model, ViewBag.GetUnit("IntakeUnit"));
            sb.AppendChart(
                "MtaGroupingDistributionChart",
                chartCreator1,
                ChartFileType.Svg,
                Model,
                ViewBag,
                chartCreator1.Title,
                true
            );

            var chartCreator2 = new MtaGroupingCumulativeDistributionChartCreator(Model, ViewBag.GetUnit("IntakeUnit"));
            sb.AppendChart(
                "MtaGroupingCumulativeDistributionChart",
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
