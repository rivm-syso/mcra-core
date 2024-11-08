using MCRA.General;
using MCRA.Simulation.Calculators.ComponentCalculation.ExposureMatrixCalculation;
using MCRA.Utils;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class IndividualsExposureOverviewSection : SummarySection {
        public override bool SaveTemporaryData => false;
        public List<SubGroupComponentSummaryRecord> SubgroupComponentSummaryRecords { get; set; }

        public void Summarize(
            SectionHeader header,
            GeneralMatrix uMatrix,
            IndividualMatrix individualMatrix,
            ClusterMethodType clusterMethodType,
            bool automaticallyDetermineNumberOfClusters,
            bool removeZeros
        ) {
            var count = 0;
            SubgroupComponentSummaryRecords = [];

            // E= U ** V = U ** D-1 ** D ** V
            var normalizationFactorU = uMatrix.Transpose().Array.Select(c => c.Sum()).ToArray();

            var section = new IndividualsExposureSection();
            var subHeader = header.AddSubSectionHeaderFor(section, $"Subgroups", count++);
            for (int clusterId = 1; clusterId <= individualMatrix.ClusterResult.Clusters.Count; clusterId++) {
                section.SummarizePerCluster(
                    clusterId,
                    individualMatrix,
                    normalizationFactorU,
                    removeZeros
                );
                section.SummarizeBoxPlotPerCluster(
                    clusterId,
                    individualMatrix,
                    normalizationFactorU
                );
            }
            SubgroupComponentSummaryRecords.AddRange(section.SubGroupComponentSummaryRecords[1]);
            subHeader.SaveSummarySection(section);

            if (clusterMethodType == ClusterMethodType.Hierarchical) {
                var section1 = new HClustSection();
                var subHeader1 = header.AddSubSectionHeaderFor(section1, "Additional details", count++);
                section1.Summarize(individualMatrix, normalizationFactorU, automaticallyDetermineNumberOfClusters);
                subHeader1.SaveSummarySection(section1);
            } else if (clusterMethodType == ClusterMethodType.Kmeans) {
                var section1 = new KMeansSection();
                var subHeader1 = header.AddSubSectionHeaderFor(section1, "Additional details", count++);
                section1.Summarize(individualMatrix, normalizationFactorU);
                subHeader.SaveSummarySection(section1);
            }
        }
    }
}
