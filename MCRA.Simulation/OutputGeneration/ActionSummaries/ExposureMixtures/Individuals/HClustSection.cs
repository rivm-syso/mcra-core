using MCRA.Simulation.Calculators.ComponentCalculation.ExposureMatrixCalculation;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class HClustSection : ClusterSectionBase {

        /// <summary>
        /// Summarize hierarchical clustering results
        /// </summary>
        /// <param name="individualMatrix"></param>
        /// <param name="automaticallyDetermineNumberOfClusters"></param>
        public void Summarize(
            IndividualMatrix individualMatrix,
            double[] normalizationFactorU,
            bool automaticallyDetermineNumberOfClusters
        ) {
            SummarizeClustering(individualMatrix, automaticallyDetermineNumberOfClusters);
            ClusterResult = individualMatrix.ClusterResult;
            VMatrix = individualMatrix.VMatrix.MultiplyRows(normalizationFactorU);
            IndividualCodes = individualMatrix.SimulatedIndividuals.Select(c => c.Code).ToList();
            ComponentCodes = Enumerable.Range(1, individualMatrix.NumberOfComponents).Select(c => c.ToString()).ToList();
        }

        /// <summary>
        /// Write V matrix SNMU to csv file
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public string WriteVMatrixCsv(string filename) {
            return VMatrix.WriteToCsvFile(filename, ComponentCodes, IndividualCodes);
        }
    }
}
