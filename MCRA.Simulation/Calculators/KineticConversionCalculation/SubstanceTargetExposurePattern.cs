using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.TargetExposuresCalculation;

namespace MCRA.Simulation.Calculators.PbpkModelCalculation {
    public sealed class SubstanceTargetExposurePattern : ISubstanceTargetExposure {

        /// <summary>
        /// Target exposures time series.
        /// </summary>
        public SubstanceTargetExposureTimeSeries TimeSeries { get; set; }

        /// <summary>
        /// Implementation of exposure: should be peak for acute and steady state for chronic.
        /// </summary>
        public double Exposure { get; set; }

        /// <summary>
        /// The relative weight of the compartment (compared to the bodyweight).
        /// TODO: currently the compartment weight is a single value and the assumption
        /// is that the compartment weight does not change over the time course of simulation.
        /// This needs to be changed in the future when working with PBK simulations spanning
        /// multiple years (e.g., from childhood to adulthood) or varying compartment sizes
        /// (e.g., urine).
        /// </summary>
        public double RelativeCompartmentWeight => TimeSeries.RelativeCompartmentWeight;

        public Compound Substance => TimeSeries.Substance;

        public ExposureTarget Target => TimeSeries.TargetUnit.Target;

        /// <summary>
        /// The target exposure per time bin.
        /// </summary>
        public List<SubstanceTargetExposureTimePoint> TargetExposuresPerTimeUnit => TimeSeries.Exposures;

        /// <summary>
        /// Maximum of internal doses (at target).
        /// </summary>
        public double MaximumTargetExposure {
            get {
                return TargetExposuresPerTimeUnit?.Count > 0
                    ? TargetExposuresPerTimeUnit.Max(r => r.Exposure)
                    : 0D;
            }
        }

        public double EquivalentSubstanceExposure(double rpf, double membershipProbability) {
            return Exposure * rpf * membershipProbability;
        }
    }
}
