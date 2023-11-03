using MCRA.General;
using MCRA.Simulation.Calculators.ComponentCalculation.ExposureMatrixCalculation;
using MCRA.Simulation.OutputGeneration.ActionSummaries.HumanMonitoringData;
using MCRA.Utils;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class IndividualsExposureSection : SummarySection {

        private readonly double _eps = 1E-10;
        public override bool SaveTemporaryData => false;
        public Dictionary<int, List<SubGroupComponentSummaryRecord>> SubGroupComponentSummaryRecords { get; } = new();
        public Dictionary<(int component, int cluster), ComponentClusterPercentilesRecord> BoxPlotSummaryRecords { get; } = new();
        public bool Selection { get; set; }

        #region Comparer class IndividualRecord
        internal class IndividualRecordComparer : IComparer<List<IndividualComponentRecord>> {
            private double RoundToSignificantDigits(double d, int digits) {
                if (digits == 999) {
                    return d;
                }
                if (d == 0) {
                    return 0;
                } else if (double.IsInfinity(d) || double.IsNaN(d)) {
                    return d;
                }

                double scale = Math.Pow(10, Math.Floor(Math.Log10(Math.Abs(d))) + 1);
                return scale * Math.Round(d / scale, digits);
            }
            public int Compare(List<IndividualComponentRecord> x, List<IndividualComponentRecord> y) {
                if (x.Count != y.Count) {
                    throw new System.ArgumentException("Array lengths must equal.");
                }
                var digits = 999;
                var maxX = RoundToSignificantDigits(x[0].NmfValue, digits);
                var maxY = RoundToSignificantDigits(y[0].NmfValue, digits);
                var indexX = 0;
                var indexY = 0;
                for (int i = 1; i < x.Count; i++) {
                    var valY = RoundToSignificantDigits(y[i].NmfValue, digits);
                    var valX = RoundToSignificantDigits(x[i].NmfValue, digits);
                    maxY = maxY > valY ? maxY : valY;
                    maxX = maxX > valX ? maxX : valX;
                    indexY = maxY > valY ? indexY : i;
                    indexX = maxX > valX ? indexX : i;
                }
                if (indexX < indexY) {
                    return 1;
                } else if (indexX > indexY) {
                    return -1;
                }
                if (maxX < maxY) {
                    return -1;
                } else if (maxX > maxY) {
                    return 1;
                }
                return 0;
            }
        }

        #endregion

        /// <summary>
        /// Calculate boxplot percentiles
        /// </summary>
        /// <param name="clusterId"></param>
        /// <param name="individualMatrix"></param>
        /// <param name="removeZeros"></param>
        public void SummarizeBoxPlotPerCluster(
            int clusterId,
            IndividualMatrix individualMatrix,
            double[] normalizationFactorU
        ) {
            var result = individualMatrix
                .ClusterResult
                .Clusters
                .Single(c => c.ClusterId == (clusterId));

            var vMatrixScaled = individualMatrix.VMatrix.MultiplyRows(normalizationFactorU);
            var exposuresSubgroup = vMatrixScaled.GetMatrix(Enumerable.Range(0, vMatrixScaled.RowDimension).ToArray(), result.Indices.ToArray());
            for (int k = 0; k < individualMatrix.VMatrix.RowDimension; k++) {
                var exposures = exposuresSubgroup.Array[k].ToList();
                var componentSummaryRecord = calculateBoxPlotPercentiles(k + 1, clusterId, exposures);
                BoxPlotSummaryRecords.Add((k + 1, clusterId), componentSummaryRecord);
            }
        }

        /// <summary>
        /// Boxplots 
        /// </summary>
        /// <param name="substance"></param>
        /// <param name="exposures"></param>
        /// <returns></returns>
        private static ComponentClusterPercentilesRecord calculateBoxPlotPercentiles(
            int component,
            int clusterId,
            List<double> exposures
        ) {
            var percentages = new double[] { 5, 10, 25, 50, 75, 90, 95 };
            var percentiles = exposures
                .Percentiles(percentages)
                .ToList();
            var positives = exposures.Where(r => r > 0).ToList();
            return new ComponentClusterPercentilesRecord() {
                MinPositives = positives.Any() ? positives.Min() : 0,
                MaxPositives = positives.Any() ? positives.Max() : 0,
                ComponentNumber = component,
                ClusterId = clusterId,
                Percentiles = percentiles,
                NumberOfPositives = exposures.Count,
            };
        }


        /// <summary>
        /// Normalization is applied per component.
        /// For each component K, the coefficients array of individuals is normalized by dividing it with the total array sum. 
        /// So each cel represents a fraction, summing up to 1 for each component.
        /// </summary>
        /// <param name="clusterId"></param>
        /// <param name="individualMatrix"></param>
        /// <param name="removeZeros"></param>
        public void SummarizePerCluster(
            int clusterId,
            IndividualMatrix individualMatrix,
            double[] normalizationFactorU,
            bool removeZeros
        ) {
            Selection = removeZeros;
            var result = individualMatrix
                .ClusterResult
                .Clusters
                .Single(c => c.ClusterId == (clusterId));

            var numberOfIndividuals = result.Individuals.Count;

            var vMatrixScaled = individualMatrix.VMatrix.MultiplyRows(normalizationFactorU);

            //normalize per individual: exposuresAll = components x individuals
            var exposuresAll = vMatrixScaled.NormalizeColumns().Transpose();
            var exposuresSubgroup = exposuresAll.GetMatrix(result.Indices.ToArray(), Enumerable.Range(0, vMatrixScaled.RowDimension).ToArray());
            var sortedExposuresSubgroup = calculateValues(exposuresSubgroup);
            var sortedExposuresAll = calculateValues(exposuresAll);

            for (int k = 0; k < individualMatrix.VMatrix.RowDimension; k++) {
                var componentSummaryRecord = new SubGroupComponentSummaryRecord {
                    ClusterId = clusterId,
                    ComponentNumber = k + 1,
                    Percentage = sortedExposuresSubgroup[k].Average(c => c.NmfValue) * 100,
                    PercentageAll = sortedExposuresAll[k].Average(c => c.NmfValue) * 100,
                    NumberOfIndividuals = numberOfIndividuals
                };
                if (!SubGroupComponentSummaryRecords.TryGetValue(clusterId, out var clusterList)) {
                    clusterList = new List<SubGroupComponentSummaryRecord>();
                    SubGroupComponentSummaryRecords.Add(clusterId, clusterList);
                };
                clusterList.Add(componentSummaryRecord);
            }
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
