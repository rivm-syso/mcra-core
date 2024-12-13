using MCRA.General;
using MCRA.Simulation.Calculators.ComponentCalculation.ExposureMatrixCalculation;
using MCRA.Simulation.Calculators.MixtureCalculation.ExposureMatrixCalculation;
using MCRA.Utils;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.OutputGeneration {

    public class ClusterExposureSection : SummarySection {
        private readonly double _eps = 1E-10;
        public int ClusterId { get; set; }
        public int NumberOfIndividuals { get; set; }
        public List<ClusterExposureRecord> Records { get; set; }

        /// <summary>
        /// Summarize exposure for clusters of individuals and total population (clusterId = 0)
        /// </summary>
        /// <param name="exposureMatrix"></param>
        /// <param name="uMatrix"></param>
        /// <param name="individualMatrix"></param>
        /// <param name="rowRecords"></param>
        /// <param name="exposureMatrixOthers"></param>
        /// <param name="clusterId"></param>
        public void Summarize(
            GeneralMatrix exposureMatrix,
            GeneralMatrix uMatrix,
            IndividualMatrix individualMatrix,
            IDictionary<int, ExposureMatrixRowRecord> rowRecords,
            GeneralMatrix exposureMatrixOthers = null,
            int clusterId = 0
        ) {
            ClusterId = clusterId;
            var sds = rowRecords.Values.Select(c => c.Stdev).ToList();
            var records = new List<ClusterExposureRecord>();
            // E= U ** V = U ** D-1 ** D ** V
            var normalizationFactorU = uMatrix.Transpose().Array.Select(c => c.Sum()).ToArray();

            var exposuresAll = individualMatrix.VMatrix.MultiplyRows(normalizationFactorU).NormalizeColumns().Transpose();
            if (clusterId > 0) {
                var result = individualMatrix
                   .ClusterResult
                   .Clusters
                   .Single(c => c.ClusterId == (clusterId));
                exposuresAll = exposuresAll.GetMatrix(result.Indices.ToArray(), Enumerable.Range(0, uMatrix.ColumnDimension).ToArray());
            }

            var sortedExposuresAll = calculateValues(exposuresAll);

            for (int componentId = 0; componentId < individualMatrix.NumberOfComponents; componentId++) {
                //normalize per individual
                var percentageExplained = (sortedExposuresAll[componentId].Average(c => c.NmfValue) * 100);
                NumberOfIndividuals = exposureMatrix.ColumnDimension;

                var components = uMatrix.NormalizeColumns().GetMatrix(Enumerable.Range(0, rowRecords.Count).ToArray(), [componentId])
                    .ColumnPackedCopy.ToList();
                var column = components.Select((c, i) => new { nmf = c, index = i }).ToList();
                var indices = column.Where(ix => ix.nmf > 0).Select(c => c.index).ToList();
                foreach (var ix in indices) {
                    var exposures = exposureMatrix.Array[ix].Select(c => c * sds[ix]).ToList();
                    var meanExposures = exposures.Average();
                    var probability = double.NaN;
                    var meanExposureOthers = double.NaN;
                    var sign = "(>)";
                    if (exposureMatrixOthers != null) {
                        var exposuresOthers = exposureMatrixOthers.Array[ix].Select(c => c * sds[ix]).ToList();
                        meanExposureOthers = exposuresOthers.Average();
                        var logExposures = exposures.Select(c => Math.Log(c)).ToList();
                        var logExposuresOthers = exposuresOthers.Select(c => Math.Log(c)).ToList();
                        probability = StatisticalTests.TTest(logExposures, logExposuresOthers, true);
                        if (meanExposures < meanExposureOthers) {
                            sign = "(<)";
                        }
                    }
                    var record = new ClusterExposureRecord() {
                        IdCluster = clusterId,
                        IdComponent = (componentId + 1).ToString(),
                        Contribution = percentageExplained,
                        SubstanceCode = rowRecords[ix].Substance.Code,
                        SubstanceName = rowRecords[ix].Substance.Name,
                        BiologicalMatrix = rowRecords[ix].TargetUnit.Target.BiologicalMatrix.GetShortDisplayName(),
                        ExpressionType = rowRecords[ix].TargetUnit.Target.ExpressionType.GetShortDisplayName(),
                        TargetUnit = rowRecords[ix].TargetUnit.GetShortDisplayName(TargetUnit.DisplayOption.AppendExpressionType),
                        RelativeContribution = column[ix].nmf * 100,
                        MeanExposure = meanExposures,
                        MedianExposure = exposures.Median(),
                        P95 = exposures.Percentile(95),
                        MaximumExposure = exposures.Max(),
                        MinimumExposure = exposures.Min(),
                        Sd = Math.Sqrt(exposures.Variance()),
                        NumberOfIndividuals = NumberOfIndividuals,
                        pValue = probability < 0.05 ? $"* {sign}" : string.Empty,
                        MeanExposureOthers = meanExposureOthers,
                        NumberOfIndividualsOthers = exposureMatrixOthers?.ColumnDimension ?? 0,
                    };
                    var substanceCodes = records.Select(c => c.SubstanceCode).ToList();
                    records.Add(record);
                }
            }
            Records = records.OrderByDescending(c => c.RelativeContribution).OrderBy(c => c.IdComponent).ToList();
        }

        /// <summary>
        /// Collect values for contribution calculations
        /// </summary>
        /// <param name="matrix"></param>
        /// <returns></returns>
        private IDictionary<int, List<IndividualComponentRecord>> calculateValues(GeneralMatrix matrix) {
            var recordsPerComponent = new Dictionary<int, List<IndividualComponentRecord>>();
            for (int i = 0; i < matrix.ColumnDimension; i++) {
                var result = new List<IndividualComponentRecord>();
                for (int j = 0; j < matrix.RowDimension; j++) {
                    result.Add(new IndividualComponentRecord() {
                        Name = $"{j}",
                        NmfValue = matrix.Array[j][i] > 0 ? matrix.Array[j][i] : _eps
                    });
                }
                recordsPerComponent[i] = result;
            }
            return recordsPerComponent;
        }
    }
}
