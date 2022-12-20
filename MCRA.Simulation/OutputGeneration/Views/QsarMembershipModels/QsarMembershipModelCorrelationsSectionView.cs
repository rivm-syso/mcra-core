using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class QsarMembershipModelCorrelationsSectionView : SectionView<QsarMembershipModelCorrelationsSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            //Render HTML
            var chartCreator = new QsarMembershipModelPearsonCorrelationsChartCreator(Model);
            sb.AppendChart(
                "QsarMembershipModelPearsonCorrelationsChart",
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
