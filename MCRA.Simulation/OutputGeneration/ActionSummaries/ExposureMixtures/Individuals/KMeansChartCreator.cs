using MCRA.Utils.Charting.OxyPlot;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.R.REngines;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class KMeansChartCreator : RChartCreatorBase {

        private readonly KMeansSection _section;

        public KMeansChartCreator(KMeansSection section) {
            _section = section;
        }

        public override string ChartId {
            get {
                var pictureId = "53b5c3bc-ddae-44d7-bd55-f2ab41e1df6c";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId);
            }
        }

        public override string Title => "KMeans clustering of individuals.";

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
            var individuals = _section.IndividualCodes;
            var components = Enumerable.Range(1, _section.ComponentCodes.Count).ToList();

            var result = _section.ClusterResult.Clusters.Select(c => (
                clusterIds: Enumerable.Repeat(c.ClusterId, c.Indices.Count).ToList(),
                indices: c.Indices
            )).ToList();

            var clusters = new List<int>();
            var indices = new List<int>();
            foreach (var item in result) {
                clusters.AddRange(item.clusterIds);
                indices.AddRange(item.indices);
            }
            var clustering = clusters.Zip(indices, (c, i) => (c, i)).OrderBy(c => c.i).Select(c => c.c).ToList();

            using (var R = new RDotNetEngine()) {
                R.LoadLibrary($"factoextra", null, true);
                R.SetSymbol("matrix", _section.VMatrix.TransposeArrayCopy2);
                R.SetSymbol("individuals", individuals);
                R.SetSymbol("components", components);
                R.EvaluateNoReturn("rownames(matrix) <- individuals");
                R.EvaluateNoReturn("colnames(matrix) <- components");
                R.SetSymbol("clustering", clustering);
                R.EvaluateNoReturn("km <- structure(list(cluster = clustering), class = 'kmeans')");
                openPlot(R);
                R.EvaluateNoReturn("plot(fviz_cluster(km, data = matrix, ellipse.type = 'convex', main = NULL))");
                closePlot(R);
            }
        }

        public override void WritePngToStream(Stream stream) {
            throw new NotImplementedException();
        }
    }
}
