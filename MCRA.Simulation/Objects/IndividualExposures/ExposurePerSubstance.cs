using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.DietaryExposureCalculation.IndividualDietaryExposureCalculation;

namespace MCRA.Simulation.Objects.IndividualExposures {

    public sealed class ExposurePerSubstance : IIntakePerCompound {

        /// <summary>
        /// The substance to which the intake belongs.
        /// </summary>
        public Compound Compound { get; set; }

        /// <summary>
        /// The (aggregated) substance intake amount.
        /// </summary>
        public double Amount { get; set; }

        ExposureRoute IIntakePerCompound.ExposureRoute => throw new NotImplementedException();

        /// <summary>
        /// The substance intake amount corrected for relative potency and
        /// assessment group membership.
        /// </summary>
        public double EquivalentSubstanceAmount(double rpf, double membershipProbability) {
            return Amount * rpf * membershipProbability;
        }
    }
}
