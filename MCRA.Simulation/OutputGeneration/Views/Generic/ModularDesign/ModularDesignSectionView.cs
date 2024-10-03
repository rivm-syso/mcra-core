using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class ModularDesignSectionView : SectionView<ModularDesignSection> {
        public override void RenderSectionHtml(StringBuilder sb) {

            var chartCreator = new DiagramChartCreator(Model);
            sb.AppendChart(
                    "Diagram",
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
