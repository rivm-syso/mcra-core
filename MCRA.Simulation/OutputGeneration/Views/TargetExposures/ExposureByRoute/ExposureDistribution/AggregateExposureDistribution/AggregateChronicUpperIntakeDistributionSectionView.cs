using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class AggregateChronicUpperIntakeDistributionSectionView : SectionView<AggregateUpperIntakeDistributionSection> {
        public override void RenderSectionHtml(StringBuilder sb) {

            //Render HTML
            var chartCreator = new AggregateUpperIntakeDistributionChartCreator(Model, ViewBag.GetUnit("IntakeUnit"));
            sb.AppendChart(
                "AggregateUpperIntakeDistributionChart",
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
