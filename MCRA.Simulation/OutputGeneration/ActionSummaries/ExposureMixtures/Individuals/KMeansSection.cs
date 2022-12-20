using MCRA.Simulation.Calculators.ComponentCalculation.ExposureMatrixCalculation;
using System.Linq;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class KMeansSection : ClusterSectionBase {
        /// <summary>
        /// Summarize K-Means clustering results
        /// </summary>
        /// <param name="individualMatrix"></param>
        public void Summarize(IndividualMatrix individualMatrix) {
            SummarizeClustering(individualMatrix);
            VMatrix = individualMatrix.VMatrix;
            IndividualCodes = individualMatrix.Individuals.Select(c => c.Code).ToList();
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
