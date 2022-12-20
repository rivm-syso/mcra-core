using MCRA.Simulation.OutputGeneration.Helpers;
using System.Linq;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class UsualIntakeDistributionPerCategorySectionView : SectionView<UsualIntakeDistributionPerCategorySection> {
        public override void RenderSectionHtml(StringBuilder sb) {

            //Render HTML
            sb.Append("<div class=\"figure-container\">");
            //plots per model category
            if (Model.IndividualExposuresByCategory?.Any() ?? false) {
                var chartCreator1 = new MtaDistributionByCategoryChartCreator(Model, ViewBag.GetUnit("IntakeUnit"), false);
                sb.AppendChart(
                    "MtaDistributionByCategory1Chart",
                    chartCreator1,
                    ChartFileType.Svg,
                    Model,
                    ViewBag,
                    saveChartFile: true
                );

                var chartCreator2 = new MtaDistributionByCategoryChartCreator(Model, ViewBag.GetUnit("IntakeUnit"), true);
                sb.AppendChart(
                    "MtaDistributionByCategory2Chart",
                    chartCreator2,
                    ChartFileType.Svg,
                    Model,
                    ViewBag,
                    saveChartFile: true
                );

            } else {
                sb.AppendParagraph("No intakes available");
            }
            sb.Append("</div>");

            if (Model.UsualIntakeDistributionPerCategoryModelSections?.Any() ?? false) {
                for (var i = 0; i < Model.UsualIntakeDistributionPerCategoryModelSections.Count; i++) {
                    var item = Model.UsualIntakeDistributionPerCategoryModelSections[i];
                    sb.Append($"<h3>Model {i + 1}: {item.FoodNames.ToHtml()}</h3>");
                    renderSectionView(sb, "UsualIntakeDistributionPerCategoryModelSection", item);
                }
            }
        }
    }
}
