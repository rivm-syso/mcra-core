using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;

namespace MCRA.Simulation.Calculators.NonDietaryIntakeCalculation {

    public sealed class NonDietaryIntakePerCompound : IIntakePerCompound {

        /// <summary>
        /// The substance to which the intake belongs.
        /// </summary>
        public Compound Compound { get; set; }

        /// <summary>
        /// The (aggregated) substance intake amount.
        /// </summary>
        public double Amount { get; set; }

        /// <summary>
        /// The exposure route of this intake.
        /// </summary>
        public ExposureRoute Route { get; set; }

        /// <summary>
        /// The substance intake amount corrected for relative potency and
        /// assessment group membership.
        /// </summary>
        public double EquivalentSubstanceAmount(double rpf, double membershipProbability) {
            return Amount * rpf * membershipProbability;
        }
    }
}
