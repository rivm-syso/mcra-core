using MCRA.Simulation.Calculators.ComponentCalculation.ExposureMatrixCalculation;
using MCRA.Utils;
using MCRA.Utils.Logger;
using MCRA.Utils.R.REngines;

namespace MCRA.Simulation.Calculators.ComponentCalculation.HClustCalculation {
    public sealed class HClustCalculator {

        private string _tmpPath;

        private int _numberOfClusters;
        private bool _automaticallyDetermineNumberOfClusters;

        public HClustCalculator(int numberOfClusters, bool automaticallyDetermineNumberOfClusters, string tempPath = null) {
            _numberOfClusters = numberOfClusters;
            _automaticallyDetermineNumberOfClusters = automaticallyDetermineNumberOfClusters;
            _tmpPath = tempPath;
        }

        public ClusterResult Compute(IndividualMatrix matrix, GeneralMatrix uMatrix) {
            var normalizationFactorU = uMatrix.Transpose().Array.Select(c => c.Sum()).ToArray();
            var vMatrix = matrix.VMatrix.MultiplyRows(normalizationFactorU);
            vMatrix = vMatrix.StandardizeColumns();
            var index = Enumerable.Range(0, matrix.VMatrix.ColumnDimension).ToList();
            var individuals = matrix.SimulatedIndividuals.Select(c => c.Id).ToList();
            var components = Enumerable.Range(1, matrix.NumberOfComponents).ToList();
            var logger = _tmpPath != null ? new FileLogger(Path.Combine(_tmpPath, "hclusterMCRA.R")) : null;
            var R = logger != null ? new LoggingRDotNetEngine(logger) : new RDotNetEngine();
            try {
                using (R) {
                    R.SetSymbol("matrix", vMatrix.TransposeArrayCopy2);
                    R.SetSymbol("individuals", individuals);
                    R.SetSymbol("components", components);
                    R.EvaluateNoReturn("rownames(matrix) <- individuals");
                    R.EvaluateNoReturn("colnames(matrix) <- components");
                    R.EvaluateNoReturn("distance <- dist(matrix, method=\"euclidean\")");
                    R.EvaluateNoReturn($"clusters <- hclust(distance, method=\"ward.D2\")");
                    var merge = R.EvaluateIntegerMatrix($"clusters$merge");
                    var height = R.EvaluateNumericVector($"clusters$height");
                    var order = R.EvaluateIntegerVector($"clusters$order");
                    if (_automaticallyDetermineNumberOfClusters) {
                        R.EvaluateNoReturn($"clust_distances <- rev(clusters$height) - rev(clusters$height)[c(2:length(rev(clusters$height)), length(rev(clusters$height)))]");
                        R.EvaluateNoReturn($"clust_distances[1] <- 0");
                        R.EvaluateNoReturn($"nb_class <- min(max(which(clust_distances==max(clust_distances)),2),length(rev(clusters$height)))+1");
                        R.EvaluateNoReturn($"members <- cutree(clusters, nb_class)");
                    } else {
                        R.EvaluateNoReturn($"members <- cutree(clusters, {_numberOfClusters})");
                    }
                    var output = R.EvaluateIntegerVector($"members");
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
                        Height = height,
                        Merge = merge,
                        Order = order,
                    };
                    return clusterResult;
                }
            } finally {
                logger?.Write();
            }
        }
    }
}
