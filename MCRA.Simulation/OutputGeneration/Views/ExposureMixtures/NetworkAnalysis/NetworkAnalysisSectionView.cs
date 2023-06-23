using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class NetworkAnalysisSectionView : SectionView<NetworkAnalysisSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var chartCreator = new NetworkAnalysisChartCreator(Model);
            sb.AppendChart(
                    "NetworkAnalysisChart",
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
