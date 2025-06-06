using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.DietaryExposureCalculation.IndividualDietaryExposureCalculation;

namespace MCRA.Simulation.Calculators.ConsumerProductExposureCalculation {

    public sealed class ConsumerProductExposurePerSubstance : IIntakePerCompound {

        /// <summary>
        /// Gets/sets the (monitoring) intake portions (concentration + amount).
        /// </summary>
        public IntakePortion IntakePortion { get; set; }

        /// <summary>
        /// The substance for which the exposure is simulated.
        /// </summary>
        public Compound Compound { get; set; }

        /// <summary>
        /// The substance intake amount corrected for relative potency and
        /// assessment group membership.
        /// </summary>
        public double EquivalentSubstanceAmount(double rpf, double membershipProbability) {
            var intake = Amount;
            intake *= rpf * membershipProbability;
            return intake;
        }

        /// <summary>
        /// The total amount used
        /// </summary>
        public double TotalAmountConsumed {
            get {
                return IntakePortion.Amount;
            }
        }

        /// <summary>
        /// Note: this id the total exposure
        /// </summary>
        public double Amount {
            get {
                var exposure = IntakePortion.Amount * IntakePortion.Concentration;
                return exposure;
            }
        }

        /// <summary>
        /// The mean concentration per unit of this intake;
        /// </summary>
        public double MeanConcentration {
            get {
                return IntakePortion.Concentration;
            }
        }

        public ExposureRoute ExposureRoute { get; set; }
    }
}
