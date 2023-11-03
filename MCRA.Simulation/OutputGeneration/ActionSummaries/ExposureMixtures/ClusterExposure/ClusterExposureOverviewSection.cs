using MCRA.General;
using MCRA.Simulation.Calculators.ComponentCalculation.ExposureMatrixCalculation;
using MCRA.Utils;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class ClusterExposureOverviewSection : SummarySection {
        private readonly double _eps = 1E-10;

        /// <summary>
        /// Summarize exposure overview
        /// </summary>
        /// <param name="exposureMatrix"></param>
        /// <param name="individualMatrix"></param>
        /// <param name="uMatrix"></param>
        /// <param name="clusterMethod"></param>
        /// <param name="header"></param>
        public void Summarize(
            ExposureMatrix exposureMatrix,
            IndividualMatrix individualMatrix,
            GeneralMatrix uMatrix,
            ClusterMethodType clusterMethod,
            SectionHeader header
        ) {
            var count = 0;
            var substances = exposureMatrix.RowRecords.Select(c => c.Value.Substance).ToList();
            var sectionAll = new ClusterExposureSection();
            var subHeaderAll = header.AddSubSectionHeaderFor(sectionAll, $"Population", count++);
            sectionAll.Summarize(
                exposureMatrix.Exposures,
                uMatrix,
                individualMatrix,
                exposureMatrix.RowRecords
            );

            if (clusterMethod != ClusterMethodType.NoClustering) {
                for (int clusterId = 1; clusterId <= individualMatrix.ClusterResult.Clusters.Count; clusterId++) {
                    var clusterResult = individualMatrix
                        .ClusterResult
                        .Clusters
                        .Single(c => c.ClusterId == (clusterId));

                    var numberOfIndividuals = clusterResult.Individuals.Count;
                    var exposureMatrixCluster = exposureMatrix.Exposures
                        .GetMatrix(Enumerable.Range(0, substances.Count).ToArray(), clusterResult.Indices.ToArray());

                    var clusterResultOthers = individualMatrix
                        .ClusterResult
                        .Clusters
                        .Where(c => c.ClusterId != (clusterId))
                        .ToList();
                    var exposureMatrixOtherClusters = exposureMatrix.Exposures
                        .GetMatrix(Enumerable.Range(0, substances.Count).ToArray(), clusterResultOthers.SelectMany(c => c.Indices).ToArray());


                    var section = new ClusterExposureSection();
                    var subHeader = subHeaderAll.AddSubSectionHeaderFor(section, $"Subgroup {clusterId} (n = {numberOfIndividuals})", count++);
                    section.Summarize(
                        exposureMatrixCluster,
                        uMatrix,
                        individualMatrix,
                        exposureMatrix.RowRecords,
                        exposureMatrixOtherClusters,
                        clusterId
                    );
                    subHeader.SaveSummarySection(section);
                }
            }
            subHeaderAll.SaveSummarySection(sectionAll);
            if (clusterMethod != ClusterMethodType.NoClustering) {
                var detailSection = new ExposureDetailSection();
                var subHeader1 = header.AddSubSectionHeaderFor(detailSection, "Additional details", count++);
                detailSection.Summarize(exposureMatrix);
                subHeader1.SaveSummarySection(detailSection);
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
