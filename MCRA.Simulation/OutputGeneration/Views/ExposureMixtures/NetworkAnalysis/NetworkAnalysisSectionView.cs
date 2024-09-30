using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class NetworkAnalysisSectionView : SectionView<NetworkAnalysisSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            if (Model.GlassoSelect != null) {
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
            } else {
                sb.AppendDescriptionParagraph("A network analysis is currently not supported when the exposure type is acute.");
            }
        }
    }
}
