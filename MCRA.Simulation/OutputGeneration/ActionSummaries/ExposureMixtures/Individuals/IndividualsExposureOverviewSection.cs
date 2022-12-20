using MCRA.General;
using MCRA.Simulation.Calculators.ComponentCalculation.ExposureMatrixCalculation;
using MCRA.Utils;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class IndividualsExposureOverviewSection : SummarySection {
        public override bool SaveTemporaryData => false;
        public List<IndividualComponentRecord> IndividualComponentRecords { get; set; }
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

            var tmp = new Dictionary<int, List<IndividualComponentRecord>>();

            SubgroupComponentSummaryRecords = new List<SubGroupComponentSummaryRecord>();
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
                    uMatrix,
                    removeZeros
                );
                tmp[clusterId] = section.IndividualComponentRecords;
                SubgroupComponentSummaryRecords.AddRange(section.SubGroupComponentSummaryRecords);
                subHeader.SaveSummarySection(section);
            }
            //To get clusters in the order in the heatplot 
            IndividualComponentRecords = new List<IndividualComponentRecord>();
            for (int i = individualMatrix.ClusterResult.Clusters.Count; i > 0; i--) {
                IndividualComponentRecords.AddRange(tmp[i]);
            }

            if (clusterMethodType == ClusterMethodType.Hierarchical) {
                var section = new HClustSection();
                var subHeader = header.AddSubSectionHeaderFor(section, "Additional details", count++);
                section.Summarize(individualMatrix, automaticallyDetermineNumberOfClusters);
                subHeader.SaveSummarySection(section);
            } else if (clusterMethodType == ClusterMethodType.Kmeans) {
                var section = new KMeansSection();
                var subHeader = header.AddSubSectionHeaderFor(section, "Additional details", count++);
                section.Summarize(individualMatrix);
                subHeader.SaveSummarySection(section);
            }
        }
    }
}
