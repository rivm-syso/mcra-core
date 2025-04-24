using MCRA.Data.Compiled.Objects;

namespace MCRA.Simulation.Calculators.DietaryExposureCalculation.IndividualDietaryExposureCalculation {

    /// <summary>
    /// Represents an intake for a substance aggregated over multiple sources (e.g., modelled foods).
    /// This class can be used to summarize/compress multiple intakes per substance in a single object.
    /// </summary>
    public sealed class AggregateIntakePerCompound : IIntakePerCompound {

        /// <summary>
        /// The substance to which the intake belongs.
        /// </summary>
        public Compound Compound { get; set; }

        /// <summary>
        /// The (aggregated) substance intake amount.
        /// </summary>
        public double Amount { get; set; }

        /// <summary>
        /// The substance intake amount corrected for relative potency and
        /// assessment group membership.
        /// </summary>
        public double EquivalentSubstanceAmount(double rpf, double membershipProbability) {
            return Amount * rpf * membershipProbability;
        }
    }
}
