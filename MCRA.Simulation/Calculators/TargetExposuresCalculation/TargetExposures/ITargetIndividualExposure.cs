using MCRA.Data.Compiled.Objects;

namespace MCRA.Simulation.Calculators.TargetExposuresCalculation {
    public interface ITargetIndividualExposure {

        /// <summary>
        /// The id of the individual used during simulation runs.
        /// </summary>
        int SimulatedIndividualId { get; }

        /// <summary>
        /// The sampling weight of the individual.
        /// </summary>
        double IndividualSamplingWeight { get; }

        /// <summary>
        /// The body weight of the individual as used in calculations, which is most of the times equal to the
        /// original individual body weight read from the data or an imputed value when the body weight is missing.
        /// </summary>
        double SimulatedIndividualBodyWeight { get; }

        Individual Individual { get; }

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
