using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.R.REngines;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class NetworkAnalysisChartCreator : ReportRChartCreatorBase {

        private readonly NetworkAnalysisSection _section;

        public override string ChartId {
            get {
                var pictureId = "a77ecee8-e565-4d77-a57f-b0b27d141603";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId);
            }
        }

        public override string Title => "Network analysis of substances.";

        public NetworkAnalysisChartCreator(NetworkAnalysisSection section) {
            _section = section;
        }

        protected override void createPlot(Action<RDotNetEngine> openPlot, Action<RDotNetEngine> closePlot) {
            using (var R = new RDotNetEngine()) {
                R.LoadLibrary($"igraph", null, true);
                R.SetSymbol("glasso.select", _section.GlassoSelect);
                R.SetSymbol("names", _section.SubstanceCodes);
                //Community detection
                R.EvaluateNoReturn("g <- graph.adjacency(as.matrix(glasso.select != 0), mode = \"undirected\", diag = FALSE)");
                R.EvaluateNoReturn("g.community <- cluster_walktrap(g)");
                R.EvaluateNoReturn("layout_grid <- layout.fruchterman.reingold(g)");
                openPlot(R);
                R.EvaluateNoReturn("plot(g.community, g, layout = layout_grid, vertex.label = names)");
                closePlot(R);
            }
        }
    }
}
