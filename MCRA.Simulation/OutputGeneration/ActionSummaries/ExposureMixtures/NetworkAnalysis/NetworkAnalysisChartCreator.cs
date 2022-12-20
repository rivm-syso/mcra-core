using log4net.Layout;
using MCRA.Utils.Charting.OxyPlot;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.R.REngines;
using OxyPlot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class NetworkAnalysisChartCreator : RChartCreatorBase {

        private readonly NetworkAnalysisSection _section;

        public NetworkAnalysisChartCreator(NetworkAnalysisSection section) {
            _section = section;
        }

        public override string ChartId {
            get {
                var pictureId = "a77ecee8-e565-4d77-a57f-b0b27d141603";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId);
            }
        }

        public override string Title => "Network analysis of substances.";

        public override void CreateToPng(string fileName) {
            Action<RDotNetEngine> openPlot = (rEngine) => rEngine.EvaluateNoReturn("png('" + fileName.Replace(@"\", "/") + "')");
            Action<RDotNetEngine> closePlot = (rEngine) => rEngine.EvaluateNoReturn("dev.off()");
            createPlot(openPlot, closePlot);
        }

        public override void CreateToSvg(string fileName) {
            Action<RDotNetEngine> openPlot = (rEngine) => rEngine.EvaluateNoReturn("svg('" + fileName.Replace(@"\", "/") + "')");
            Action<RDotNetEngine> closePlot = (rEngine) => rEngine.EvaluateNoReturn("dev.off()");
            createPlot(openPlot, closePlot);
        }

        public override string ToSvgString(int width, int height) {
            Action<RDotNetEngine> openPlot = (rEngine) => {
                rEngine.LoadLibrary($"svglite", null, true);
                rEngine.EvaluateNoReturn("s <- svgstring(standalone = FALSE)");
            };
            string result = null;
            Action<RDotNetEngine> closePlot = (rEngine) => {
                result = rEngine.EvaluateString("s()");
                rEngine.EvaluateNoReturn("dev.off()");
            };
            createPlot(openPlot, closePlot);
            return result;
        }

        private void createPlot(Action<RDotNetEngine> openPlot, Action<RDotNetEngine> closePlot) {

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

        public override void WritePngToStream(Stream stream) {
            throw new NotImplementedException();
        }
    }
}
