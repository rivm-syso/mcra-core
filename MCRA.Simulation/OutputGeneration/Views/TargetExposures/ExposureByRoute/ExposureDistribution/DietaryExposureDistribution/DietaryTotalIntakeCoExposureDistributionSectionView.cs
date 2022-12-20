using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class DietaryTotalIntakeCoExposureDistributionSectionView : SectionView<DietaryTotalIntakeCoExposureDistributionSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            if (Model.PercentageZeroIntake < 100) {
                sb.Append("<div class=\"figure-container\">");
                var chartCreator1 = new DietaryTotalIntakeCoExposureDistributionChartCreator(Model, ViewBag.GetUnit("IntakeUnit"));
                sb.AppendChart(
                    "DietaryTotalIntakeCoExposureDistributionChart",
                    chartCreator1,
                    ChartFileType.Svg,
                    Model,
                    ViewBag,
                    chartCreator1.Title,
                    true
                );
                var chartCreator2 = new DietaryTotalIntakeCoExposureCumulativeDistributionChartCreator(Model, ViewBag.GetUnit("IntakeUnit"));
                sb.AppendChart(
                    "DietaryTotalIntakeCoExposureCumulativeDistributionChart",
                    chartCreator2,
                    ChartFileType.Svg,
                    Model,
                    ViewBag,
                    chartCreator2.Title,
                    true
                );
                sb.Append("</div>");
            } else {
                sb.AppendDescriptionParagraph("Not available");
            }
        }
    }
}
