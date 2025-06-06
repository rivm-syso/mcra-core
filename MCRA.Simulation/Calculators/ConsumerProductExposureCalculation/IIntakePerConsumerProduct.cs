using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Calculators.DietaryExposureCalculation.IndividualDietaryExposureCalculation;

namespace MCRA.Simulation.Calculators.ConsumerProductExposureCalculation{

    /// <summary>
    /// Summarizes all info for a consumer product.
    /// </summary>
    public interface IIntakePerConsumerProduct {
        /// <summary>
        /// The consumer product of this intake.
        /// </summary>
        ConsumerProduct Product { get; }

        /// <summary>
        /// The total compound exposure per modelled food.
        /// </summary>
        List<IIntakePerCompound> IntakesPerSubstance { get; }

        /// <summary>
        /// All IntakesPerSubstance summed.
        /// </summary>
        double Intake(IDictionary<Compound, double> relativePotencyFactors, IDictionary<Compound, double> membershipProbabilities);

        /// <summary>
        /// Specifies if there is any positive substance exposure present in this consumer product exposure.
        /// </summary>
        /// <returns></returns>
        bool IsPositiveIntake();
    }
}
