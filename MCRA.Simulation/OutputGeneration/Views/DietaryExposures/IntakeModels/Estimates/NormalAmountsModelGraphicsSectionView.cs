using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class NormalAmountsModelGraphicsSectionView : SectionView<NormalAmountsModelGraphicsSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            //Render HTML
            var chartCreator = new AmountsCovariableChartCreator(Model);
            sb.AppendChart(
                "AmountsCovariableChart",
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
