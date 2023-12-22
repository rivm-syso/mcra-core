using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.R.REngines;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class HClustChartCreator : ReportRChartCreatorBase {

        private readonly HClustSection _section;

        public HClustChartCreator(HClustSection section) {
            _section = section;
        }

        public override string ChartId {
            get {
                var pictureId = "d494968e-0ea4-42dc-be72-dc4bd21fc5b2";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId);
            }
        }

        public override string Title => "Hierarchical clustering of individuals.";

        protected override void createPlot(Action<RDotNetEngine> openPlot, Action<RDotNetEngine> closePlot) {
            using (var R = new RDotNetEngine()) {
                R.SetSymbol("merge", _section.ClusterResult.Merge);
                R.SetSymbol("height", _section.ClusterResult.Height);
                R.SetSymbol("order", _section.ClusterResult.Order);
                R.EvaluateNoReturn("clusters <- structure(list(merge = merge, height = height, order = order))");
                R.EvaluateNoReturn("attr(clusters, 'class') <- 'hclust'");
                openPlot(R);
                R.EvaluateNoReturn("plot(clusters, hang = -1, labels = FALSE, xlab = NULL, ylab = NULL, main = NULL, axes = FALSE)");
                R.EvaluateNoReturn($"rect.hclust(clusters, k = {_section.Clusters.Count}, border = \"red\")");
                closePlot(R);
            }
        }

        public override void WritePngToStream(Stream stream) {
            throw new NotImplementedException();
        }
    }
}
