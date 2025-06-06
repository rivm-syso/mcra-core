using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Calculators.DietaryExposureCalculation.IndividualDietaryExposureCalculation;

namespace MCRA.Simulation.Calculators.ConsumerProductExposureCalculation {

    /// <summary>
    /// Summarizes all info for a consumer product.
    /// </summary>
    public sealed class IntakePerConsumerProduct : IIntakePerConsumerProduct {

        /// <summary>
        /// The total substance exposure of consumer product.
        /// </summary>
        public List<IIntakePerCompound> IntakesPerSubstance { get; set; }

        /// <summary>
        /// The consumer product of this exposure.
        /// </summary>
        public ConsumerProduct Product { get; set; }


        /// <summary>
        /// All intakes per substance summed.
        /// </summary>
        public double Intake(
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities
        ) {
            return IntakesPerSubstance.Sum(ipc => ipc.EquivalentSubstanceAmount(relativePotencyFactors[ipc.Compound], membershipProbabilities[ipc.Compound]));
        }

        /// <summary>
        /// Specifies if there is any positive substance exposure present in this food-intake.
        /// </summary>
        /// <returns></returns>
        public bool IsPositiveIntake() {
            return IntakesPerSubstance.Any(r => r.Amount > 0);
        }
    }
}
