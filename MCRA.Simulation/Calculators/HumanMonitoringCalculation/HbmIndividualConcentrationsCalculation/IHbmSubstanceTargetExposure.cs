using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Calculators.TargetExposuresCalculation;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmIndividualConcentrationsCalculation {
    public interface IHbmSubstanceTargetExposure : ISubstanceTargetExposureBase {

        /// <summary>
        /// The biological matrix for which this concentration
        /// value applies.
        /// </summary>
        string BiologicalMatrix { get; }

        /// <summary>
        /// The estimate of the concentration at the target biological matrix obtained
        /// from human monitoring. Includes corrections for e.g., specific gravity.
        /// </summary>
        double Concentration { get; set; }

        /// <summary>
        /// The original sampling methods of the from which this.
        /// </summary>
        List<HumanMonitoringSamplingMethod> SourceSamplingMethods { get; }

        /// <summary>
        /// The total substance concentration corrected for RPF and 
        /// membership probability.
        /// </summary>
        double EquivalentSubstanceConcentration(double rpf, double membershipProbability);
    }
}
