using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class UsualIntakeDistributionPerFoodAsMeasuredSectionView : SectionView<UsualIntakeDistributionPerFoodAsMeasuredSection> {
        public override void RenderSectionHtml(StringBuilder sb) {

            //Render HTML
            if (Model.IndividualExposuresByCategory?.Any() ?? false) {
                sb.Append("<div class=\"figure-container\">");
                var chartCreator1 = new MtaDistributionByFoodAsMeasuredChartCreator(Model, ViewBag.GetUnit("IntakeUnit"), false);
                sb.AppendChart(
                    "MtaDistributionByFoodAsMeasured1Chart",
                    chartCreator1,
                    ChartFileType.Svg,
                    Model,
                    ViewBag,
                    saveChartFile: true
                );

                var chartCreator2 = new MtaDistributionByFoodAsMeasuredChartCreator(Model, ViewBag.GetUnit("IntakeUnit"), true);
                sb.AppendChart(
                    "MtaDistributionByFoodAsMeasured2Chart",
                    chartCreator2,
                    ChartFileType.Svg,
                    Model,
                    ViewBag,
                    saveChartFile: true
                );
                sb.Append("</div>");
            } else {
                sb.AppendParagraph("No intakes available");
            }
        }
    }
}
