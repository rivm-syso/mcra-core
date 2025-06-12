using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.DietaryExposureCalculation.IndividualDietaryExposureCalculation;

namespace MCRA.Simulation.Calculators.ConsumerProductExposureCalculation {

    public sealed class ConsumerProductExposurePerSubstance : IIntakePerCompound {

        /// <summary>
        /// The substance for which the exposure is simulated.
        /// </summary>
        public Compound Compound { get; set; }

        /// <summary>
        /// The total amount used.
        /// </summary>
        public double UseAmount { get; set; }

        /// <summary>
        /// The total concentration.
        /// </summary>
        public double Concentration { get; set; }

        /// <summary>
        /// The total exposure/intake (absolute amount).
        /// </summary>
        public double Amount {
            get {
                var exposure = UseAmount * Concentration;
                return exposure;
            }
        }

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
