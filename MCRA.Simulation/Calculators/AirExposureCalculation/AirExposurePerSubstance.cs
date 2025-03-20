using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;

namespace MCRA.Simulation.Calculators.AirExposureCalculation {

    public sealed class AirExposurePerSubstance : IIntakePerCompound {

        /// <summary>
        /// The substance for which the exposure is simulated.
        /// </summary>
        public Compound Compound { get; set; }

        /// <summary>
        /// The (aggregated) substance intake amount.
        /// </summary>
        public double Amount { get; set; }

        /// <summary>
        /// The substance intake amount corrected for relative potency and assessment group membership.
        /// </summary>
        public double EquivalentSubstanceAmount(double rpf, double membershipProbability) {
            var exposure = Amount;
            exposure *= rpf * membershipProbability;
            return exposure;
        }
    }
}
