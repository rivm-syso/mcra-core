using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class FrequenciesModelGraphicsSectionView : SectionView<FrequenciesModelGraphicsSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            //Render HTML
            var chartCreator = new FrequenciesCovariableChartCreator(Model);
            sb.AppendChart(
                "FrequenciesCovariableChart",
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
