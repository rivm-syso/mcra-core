using MCRA.Data.Compiled.Objects;
using MCRA.General;

namespace MCRA.Simulation.Calculators.PbpkModelCalculation {
    public class SubstanceTargetExposureTimeSeries {

        /// <summary>
        /// The chemical substance.
        /// </summary>
        public Compound Substance { get; set; }

        /// <summary>
        /// The (internal) exposure target and unit.
        /// </summary>
        public TargetUnit TargetUnit { get; set; }

        /// <summary>
        /// The exposure time series (amount or concentration per time).
        /// Time unit is in days.
        /// </summary>
        public List<SubstanceTargetExposureTimePoint> Exposures { get; set; }

        /// <summary>
        /// Relative compartment size of the target.
        /// </summary>
        public double RelativeCompartmentWeight { get; set; }

    }
}
