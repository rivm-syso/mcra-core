using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Objects;

namespace MCRA.Simulation.Calculators.DietaryExposureCalculation.IndividualDietaryExposureCalculation {

    /// <summary>
    /// Summarizes all info for a consumed modelled food: the food as eaten, the consumed amount, the exposures per substance.
    /// </summary>
    public sealed class IntakePerFood : IIntakePerFood {
        /// <summary>
        /// The consumption per modelled food.
        /// </summary>
        public ConsumptionsByModelledFood ConsumptionFoodAsMeasured { get; set; }

        /// <summary>
        /// The proportion of FoodAsMeasured in FoodConversionResult multiplied by the Amount consumed (in FoodConsumption).
        /// </summary>
        public double Amount { get; set; }

        /// <summary>
        /// The total compound exposure per modelled food.
        /// </summary>
        public List<IIntakePerCompound> IntakesPerCompound { get; set; }

        /// <summary>
        /// The modelled food of this intake.
        /// </summary>
        public Food FoodAsMeasured {
            get {
                return ConsumptionFoodAsMeasured.FoodAsMeasured;
            }
        }

        /// <summary>
        /// Gets the food as eaten of this intake per food.
        /// </summary>
        public Food FoodAsEaten {
            get {
                return FoodConsumption.Food;
            }
        }

        /// <summary>
        /// The original FoodConsumption entity.
        /// </summary>
        public FoodConsumption FoodConsumption {
            get {
                return ConsumptionFoodAsMeasured.FoodConsumption;
            }
        }

        /// <summary>
        /// Cast of the intakes per substance.
        /// </summary>
        public IEnumerable<DietaryIntakePerCompound> DetailedIntakesPerCompound {
            get {
                return IntakesPerCompound.Cast<DietaryIntakePerCompound>();
            }
        }

        /// <summary>
        /// All intakes per substance summed.
        /// </summary>
        public double Intake(
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities
        ) {
            return IntakesPerCompound.Sum(ipc => ipc.EquivalentSubstanceAmount(relativePotencyFactors[ipc.Compound], membershipProbabilities[ipc.Compound]));
        }

        /// <summary>
        /// Specifies if there is any positive substance exposure present in this food-intake.
        /// </summary>
        /// <returns></returns>
        public bool IsPositiveIntake() {
            return IntakesPerCompound.Any(r => r.Amount > 0);
        }
    }
}
