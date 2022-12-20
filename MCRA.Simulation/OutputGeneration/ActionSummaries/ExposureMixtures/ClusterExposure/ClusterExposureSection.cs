using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Calculators.ComponentCalculation.ExposureMatrixCalculation;
using MCRA.Utils;
using MCRA.Utils.Statistics;
using System;
using System.Collections.Generic;
using System.Linq;

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
        /// <param name="substances"></param>
        /// <param name="sds"></param>
        /// <param name="exposureMatrixOthers"></param>
        /// <param name="clusterId"></param>
        public void Summarize(
            GeneralMatrix exposureMatrix,
            GeneralMatrix uMatrix,
            IndividualMatrix individualMatrix,
            List<Compound> substances,
            List<double> sds,
            GeneralMatrix exposureMatrixOthers = null,
            int clusterId = 0
        ) {
            ClusterId = clusterId;

            var records = new List<ClusterExposureRecord>();
            for (int componentId = 0; componentId < individualMatrix.NumberOfComponents; componentId++) {

                //normalize per individual
                var exposuresAll = individualMatrix.VMatrix.NormalizeColumns().Transpose();
                var sortedExposuresAll = calculateValues(exposuresAll);
                var percentageExplained = (sortedExposuresAll[componentId].Average(c => c.NmfValue) * 100);


                NumberOfIndividuals = exposureMatrix.ColumnDimension;

                var components = uMatrix.GetMatrix(Enumerable.Range(0, substances.Count).ToArray(), new int[] { componentId })
                        .ColumnPackedCopy.ToList();
                var column = components.Select((c, i) => new { nmf = c, index = i })
                    .Where(ix => ix.nmf > 0)
                    .Select(c => c)
                    .ToList();
                var indices = column.Select(c => c.index).ToList();
                var sum = column.Sum(c => c.nmf);
                var percentage = column.Select(c => c.nmf / sum * 100).ToList();
                for (int i = 0; i < indices.Count; i++) {
                    var ix = indices[i];
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
                        Contribution = percentageExplained.ToString("F1"),
                        SubstanceCode = substances[ix].Code,
                        SubstanceName = substances[ix].Name,
                        MeanExposure = meanExposures,
                        MedianExposure = exposures.Median(),
                        P95 = exposures.Percentile(95),
                        MaximumExposure = exposures.Max(),
                        MinimumExposure = exposures.Min(),
                        Sd = Math.Sqrt(exposures.Variance()),
                        NumberOfIndividuals = NumberOfIndividuals,
                        pValue = probability < 0.05 ? $"* {sign}" : string.Empty,
                        MeanExposureOthers = meanExposureOthers
                    };
                    var substanceCodes = records.Select(c => c.SubstanceCode).ToList();

                    if (substanceCodes.Any() && substanceCodes.Contains(record.SubstanceCode)) {
                        var identicalRecord = records.Single(c => c.SubstanceCode.Equals(record.SubstanceCode));
                        identicalRecord.IdComponent += $", {record.IdComponent}";
                        identicalRecord.Contribution += $", {record.Contribution}";
                    } else {
                        records.Add(record);
                    }
                }
            }
            Records = records.OrderBy(c => c.IdComponent).ToList();
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
