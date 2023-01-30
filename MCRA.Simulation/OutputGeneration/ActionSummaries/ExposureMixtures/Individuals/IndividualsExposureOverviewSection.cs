using MCRA.General;
using MCRA.Simulation.Calculators.ComponentCalculation.ExposureMatrixCalculation;
using MCRA.Utils;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class IndividualsExposureOverviewSection : SummarySection {
        public override bool SaveTemporaryData => false;
        public List<SubGroupComponentSummaryRecord> SubgroupComponentSummaryRecords { get; set; }
        public int NumberOfIndividuals { get; set; }
        public int NumberOfComponents { get; set; }
        public int NumberOfClusters { get; set; }

        public void Summarize(
            SectionHeader header,
            GeneralMatrix uMatrix,
            IndividualMatrix individualMatrix,
            ClusterMethodType clusterMethodType,
            bool automaticallyDetermineNumberOfClusters,
            bool removeZeros
        ) {

            var count = 0;
            NumberOfIndividuals = individualMatrix
                    .ClusterResult
                    .Clusters
                    .Sum(c => c.Individuals.Count);
            NumberOfComponents = individualMatrix.NumberOfComponents;
            NumberOfClusters = individualMatrix.ClusterResult.Clusters.Count;
            SubgroupComponentSummaryRecords = new List<SubGroupComponentSummaryRecord>();

            // E= U ** V = U ** D-1 ** D ** V
            var normalizationFactorU = uMatrix.Transpose().Array.Select(c => c.Sum()).ToArray();

            for (int clusterId = 1; clusterId <= individualMatrix.ClusterResult.Clusters.Count; clusterId++) {
                var numberOfIndividuals = individualMatrix
                    .ClusterResult
                    .Clusters
                    .Single(c => c.ClusterId == (clusterId)).Individuals.Count;
                var section = new IndividualsExposureSection();
                var subHeader = header.AddSubSectionHeaderFor(section, $"Subgroup {clusterId} (n = {numberOfIndividuals})", count++);
                section.SummarizePerCluster(
                    clusterId,
                    individualMatrix,
                    normalizationFactorU,
                    removeZeros
                );
                SubgroupComponentSummaryRecords.AddRange(section.SubGroupComponentSummaryRecords);
                subHeader.SaveSummarySection(section);
            }

            if (clusterMethodType == ClusterMethodType.Hierarchical) {
                var section = new HClustSection();
                var subHeader = header.AddSubSectionHeaderFor(section, "Additional details", count++);
                section.Summarize(individualMatrix, normalizationFactorU, automaticallyDetermineNumberOfClusters);
                subHeader.SaveSummarySection(section);
            } else if (clusterMethodType == ClusterMethodType.Kmeans) {
                var section = new KMeansSection();
                var subHeader = header.AddSubSectionHeaderFor(section, "Additional details", count++);
                section.Summarize(individualMatrix, normalizationFactorU);
                subHeader.SaveSummarySection(section);
            }
        }
    }
}
