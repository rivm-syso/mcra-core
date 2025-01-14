using MCRA.Simulation.Calculators.ComponentCalculation.ExposureMatrixCalculation;
using MCRA.Utils;
using MCRA.Utils.Logger;
using MCRA.Utils.R.REngines;

namespace MCRA.Simulation.Calculators.ComponentCalculation.KMeansCalculation {
    public sealed class KMeansCalculator {

        private string _tmpPath;

        private int _numberOfClusters;

        public KMeansCalculator(int numberOfClusters, string tempPath = null) {
            _numberOfClusters = numberOfClusters;
            _tmpPath = tempPath;
        }

        public ClusterResult Compute(
                IndividualMatrix matrix,
                GeneralMatrix uMatrix
            ) {
            var normalizationFactorU = uMatrix.Transpose().Array.Select(c => c.Sum()).ToArray();
            var vMatrix = matrix.VMatrix.MultiplyRows(normalizationFactorU);
            vMatrix = vMatrix.StandardizeColumns();
            var index = Enumerable.Range(0, matrix.VMatrix.ColumnDimension).ToList();
            var individuals = matrix.SimulatedIndividuals.Select(c => c.Id).ToList();
            var components = Enumerable.Range(1, matrix.NumberOfComponents).ToList();
            var logger = _tmpPath != null ? new FileLogger(Path.Combine(_tmpPath, "kmeansClusterMCRA.R")) : null;
            var R = logger != null ? new LoggingRDotNetEngine(logger) : new RDotNetEngine();
            try {
                using (R) {
                    R.LoadLibrary($"factoextra", null, true);
                    R.SetSymbol("matrix", vMatrix.TransposeArrayCopy2);
                    R.SetSymbol("individuals", individuals);
                    R.SetSymbol("components", components);
                    R.EvaluateNoReturn("rownames(matrix) <- individuals");
                    R.EvaluateNoReturn("colnames(matrix) <- components");
                    R.EvaluateNoReturn($"k2 <- kmeans(matrix, centers = {_numberOfClusters}, nstart = 25)");
                    var output = R.EvaluateIntegerVector($"k2$cluster");
                    var rows = output.Zip(index, (x, w) => (Cluster: x, Index: w)).ToList();
                    var clusters = rows
                        .GroupBy(r => r.Cluster)
                        .Select(g => new ClusterRecord {
                            ClusterId = g.Key,
                            SimulatedIndividuals = g.Select(r => matrix.SimulatedIndividuals.ElementAt(r.Index)).ToList(),
                            Indices = g.Select(r => r.Index).ToList()
                        })
                        .ToList();
                    var clusterResult = new ClusterResult() {
                        Clusters = clusters,
                    };
                    return clusterResult;
                }
            } finally {
                logger?.Write();
            }
        }
    }
}
