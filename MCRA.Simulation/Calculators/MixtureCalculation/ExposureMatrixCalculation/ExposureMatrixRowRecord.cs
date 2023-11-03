using MCRA.Data.Compiled.Objects;
using MCRA.General;

namespace MCRA.Simulation.Calculators.MixtureCalculation.ExposureMatrixCalculation {
    public sealed class ExposureMatrixRowRecord {

        /// <summary>
        /// Substance of the row.
        /// </summary>
        public Compound Substance { get; set; }

        /// <summary>
        /// The target unit which contains the target of the row
        /// </summary>
        public TargetUnit TargetUnit { get; set; }

        /// <summary>
        /// The original standard deviations of the values of the individual(day)s for 
        /// this substance target combination. Can be used to back-translate standardized
        /// matrices to unstandardized exposures (i.e., the original substance exposures).
        /// </summary>
        public double Stdev { get; set; }
    }
}
