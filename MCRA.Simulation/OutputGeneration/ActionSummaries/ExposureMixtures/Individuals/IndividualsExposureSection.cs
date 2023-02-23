using MCRA.Simulation.Calculators.ComponentCalculation.ExposureMatrixCalculation;
using MCRA.Utils;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class IndividualsExposureSection : SummarySection {

        private readonly double _eps = 1E-10;
        public override bool SaveTemporaryData => false;
        public List<SubGroupComponentSummaryRecord> SubGroupComponentSummaryRecords { get; set; }
        public int ClusterId { get; set; }
        public int NumberOfComponents { get; set; }
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
            ClusterId = clusterId;
            Selection = removeZeros;
            NumberOfComponents = individualMatrix.NumberOfComponents;
            var result = individualMatrix
                .ClusterResult
                .Clusters
                .Single(c => c.ClusterId == (clusterId));
            
            var vMatrixScaled = individualMatrix.VMatrix.MultiplyRows(normalizationFactorU);

            //normalize per individual: exposuresAll = components x individuals
            var exposuresAll = vMatrixScaled.NormalizeColumns().Transpose();
            var exposuresSubgroup = exposuresAll.GetMatrix(result.Indices.ToArray(), Enumerable.Range(0, vMatrixScaled.RowDimension).ToArray());
            var sortedExposuresSubgroup = calculateValues(exposuresSubgroup);
            var sortedExposuresAll = calculateValues(exposuresAll);

            SubGroupComponentSummaryRecords = new List<SubGroupComponentSummaryRecord>();
            for (int k = 0; k < individualMatrix.VMatrix.RowDimension; k++) {
                var componentSummaryRecord = new SubGroupComponentSummaryRecord() {
                    ClusterId = clusterId,
                    ComponentNumber = k + 1,
                };
                componentSummaryRecord.Percentage = sortedExposuresSubgroup[k].Average(c => c.NmfValue) * 100;
                componentSummaryRecord.PercentageAll = sortedExposuresAll[k].Average(c => c.NmfValue) * 100;
                SubGroupComponentSummaryRecords.Add(componentSummaryRecord);
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
