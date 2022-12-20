using MCRA.Simulation.OutputGeneration.Helpers;
using System.Linq;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class DietaryTotalIntakeDistributionSectionView : SectionView<DietaryTotalIntakeDistributionSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            bool showUncertainty = !Model.Percentiles.All(p => double.IsNaN(p.MedianUncertainty));

            //Render HTML
            if (Model is DietaryTotalIntakeCoExposureDistributionSection) {
                sb.Append("div");
                renderSectionView(sb, "DietaryTotalIntakeCoExposureDistributionSection", Model);
                sb.Append("/div");
            } else {
                sb.Append("<div class=\"figure-container\">");
                var chartCreator1 = new DietaryTotalIntakeDistributionChartCreator(Model, ViewBag.GetUnit("IntakeUnit"));
                sb.AppendChart(
                        "DietaryTotalIntakeDistributionChart",
                        chartCreator1,
                        ChartFileType.Svg,
                        Model,
                        ViewBag,
                        chartCreator1.Title,
                        true
                    );
                var chartCreator2 = new DietaryTotalIntakeCumulativeDistributionChartCreator(Model, ViewBag.GetUnit("IntakeUnit"));
                sb.AppendChart(
                        "DietaryTotalIntakeCumulativeDistributionChart",
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
