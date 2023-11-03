using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Calculators.MixtureCalculation.ExposureMatrixCalculation;
using MCRA.Utils;

namespace MCRA.Simulation.Calculators.ComponentCalculation.ExposureMatrixCalculation {
    public sealed class ExposureMatrix {

        /// <summary>
        /// The exposure matrix.
        /// </summary>
        public GeneralMatrix Exposures { get; set; }

        /// <summary>
        /// The individual(day) records of the columns of the matrix.
        /// </summary>
        public ICollection<Individual> Individuals { get; set; }

        /// <summary>
        /// The key index follows the array order of the exposure matrix.
        /// Rows are combinations of substance and target.
        /// </summary>
        public IDictionary<int, ExposureMatrixRowRecord> RowRecords { get; set; }

    }
}
