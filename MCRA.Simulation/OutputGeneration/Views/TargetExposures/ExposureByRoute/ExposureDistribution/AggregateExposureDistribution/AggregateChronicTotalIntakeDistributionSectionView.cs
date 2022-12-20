using MCRA.Simulation.OutputGeneration.Helpers;
using System.Collections.Generic;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class AggregateChronicTotalIntakeDistributionSectionView : SectionView<AggregateTotalIntakeDistributionSection> {
        public override void RenderSectionHtml(StringBuilder sb) {

            //Render HTML
            sb.Append("<div class=\"figure-container\">");
            var chartCreator1 = new AggregateTotalIntakeDistributionChartCreator(Model, ViewBag.GetUnit("IntakeUnit"));
            sb.AppendChart(
                "AggregateTotalIntakeDistributionChart",
                chartCreator1,
                ChartFileType.Svg,
                Model,
                ViewBag,
                chartCreator1.Title,
                true
            );

            var chartCreator2 = new AggregateTotalIntakeCumulativeDistributionChartCreator(Model, ViewBag.GetUnit("IntakeUnit"));
            sb.AppendChart(
                "AggregateTotalIntakeCumulativeDistributionChart",
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
