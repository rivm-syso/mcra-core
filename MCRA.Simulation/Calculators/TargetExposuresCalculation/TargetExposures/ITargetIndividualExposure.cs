using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers;

namespace MCRA.Simulation.Calculators.TargetExposuresCalculation {
    public interface ITargetIndividualExposure {

        SimulatedIndividual SimulatedIndividual { get; }

        double IntraSpeciesDraw { get; set; }

        /// <summary>
        /// Returns all the substances for this target exposure.
        /// </summary>
        ICollection<Compound> Substances { get; }

        /// <summary>
        /// Returns the (cumulative) substance conconcentration of the
        /// target. I.e., the total (corrected) amount divided by the
        /// volume of the target.
        /// </summary>
        double GetSubstanceExposure(Compound substance);

        /// <summary>
        /// Gets the target exposure value for a substance, corrected for
        /// relative potency and membership probability.
        /// </summary>
        double GetSubstanceExposure(
            Compound substance,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities
        );

        ISubstanceTargetExposure GetSubstanceTargetExposure(Compound compound);

        /// <summary>
        /// Get the total substance concentration
        /// </summary>
        double GetCumulativeExposure(
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities
        );

        /// <summary>
        /// Returns true if the exposure is positive.
        /// </summary>
        bool IsPositiveExposure();

    }
}
