using MCRA.Simulation.Calculators.ComponentCalculation.ExposureMatrixCalculation;
using MCRA.Utils;

namespace MCRA.Simulation.OutputGeneration {

    /// <summary>
    /// Download for exposure matrix
    /// </summary>
    public sealed class ExposureDetailSection : SummarySection {
        public List<string> SubstanceCodes { get; set; }
        public GeneralMatrix ExposureMatrix { get; set; }
        public List<string> IndividualCodes { get; set; }


        /// <summary>
        /// Download for original exposure matrix
        /// </summary>
        /// <param name="exposureMatrix"></param>
        public void Summarize(
                ExposureMatrix exposureMatrix
            ) {
            SubstanceCodes = exposureMatrix.RowRecords.Values.Select(c => c.Substance.Code).ToList();
            var sdDiag = GeneralMatrix.CreateDiagonal(exposureMatrix.RowRecords.Values.Select(c => c.Stdev).ToArray());
            ExposureMatrix = sdDiag.Multiply(exposureMatrix.Exposures);
            IndividualCodes = exposureMatrix.Individuals.Select(c => c.Code).ToList();
        }

        /// <summary>
        /// Write exposure matrix  to csv file
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public string WriteExposureMatrixCsv(string filename) {
            return ExposureMatrix.WriteToCsvFile(filename, SubstanceCodes, IndividualCodes);
        }
    }
}
