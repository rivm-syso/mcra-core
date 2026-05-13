using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Calculators.DietaryExposureCalculation.IndividualDietaryExposureCalculation;

namespace MCRA.Simulation.Calculators.ConsumerProductExposureCalculation {

    public sealed record ConsumerProductExposurePerSubstance(
        /// <summary>
        /// The substance for which the exposure is simulated.
        /// </summary>
        Compound Compound,
        /// <summary>
        /// The total exposure/intake (absolute amount, per person) in ug
        /// </summary>
        double Amount
        ) : IIntakePerCompound {

        /// <summary>
        /// The substance intake amount corrected for relative potency and
        /// assessment group membership.
        /// </summary>
        public double EquivalentSubstanceAmount(double rpf, double membershipProbability) {
            var intake = Amount * rpf * membershipProbability;
            return intake;
        }
    }
}
