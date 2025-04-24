using MCRA.Data.Compiled.Objects;

namespace MCRA.Simulation.Calculators.DietaryExposureCalculation.IndividualDietaryExposureCalculation {

    /// <summary>
    /// Summarizes all info for a consumed modelled food: the food as eaten, the consumed amount, the exposures per substance.
    /// </summary>
    public interface IIntakePerFood {
        /// <summary>
        /// The modelled food of this intake.
        /// </summary>
        Food FoodAsMeasured { get; }

        /// <summary>
        /// The proportion of FoodAsMeasured in FoodConversionResult multiplied by the Amount consumed (in FoodConsumption).
        /// </summary>
        double Amount { get; }

        /// <summary>
        /// The total compound exposure per modelled food.
        /// </summary>
        List<IIntakePerCompound> IntakesPerCompound { get; }

        /// <summary>
        /// All IntakesPerCompound summed.
        /// </summary>
        double Intake(IDictionary<Compound, double> relativePotencyFactors, IDictionary<Compound, double> membershipProbabilities);

        /// <summary>
        /// Specifies if there is any positive substance exposure present in this food-intake.
        /// </summary>
        /// <returns></returns>
        bool IsPositiveIntake();
    }
}
