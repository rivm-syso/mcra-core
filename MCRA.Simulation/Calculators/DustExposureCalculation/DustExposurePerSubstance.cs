using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;

namespace MCRA.Simulation.Calculators.DustExposureCalculation {

    public sealed class DustExposurePerSubstance : IIntakePerCompound {

        /// <summary>
        /// The substance for which the exposure is simulated.
        /// </summary>
        public Compound Compound { get; set; }

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
