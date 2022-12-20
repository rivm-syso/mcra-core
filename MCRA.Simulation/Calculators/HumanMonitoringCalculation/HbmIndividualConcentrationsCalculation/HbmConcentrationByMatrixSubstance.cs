using MCRA.Data.Compiled.Objects;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation {
    public sealed class HbmConcentrationByMatrixSubstance {

        /// <summary>
        /// The substance.
        /// </summary>
        public Compound Substance { get; set; }

        /// <summary>
        /// The measured endpoint.
        /// </summary>
        public HumanMonitoringSamplingMethod SamplingMethod { get; set; }

        /// <summary>
        /// The estimate of the concentration at the target biological matrix obtained
        /// from human monitoring. Includes corrections for e.g., specific gravity.
        /// </summary>
        public double Concentration { get; set; }

    }
}
