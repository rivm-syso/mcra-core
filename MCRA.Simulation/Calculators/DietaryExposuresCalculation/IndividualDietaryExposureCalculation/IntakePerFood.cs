using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers;

namespace MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation {

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
        /// <param name="relativePotencyFactors"></param>
        /// <param name="membershipProbabilities"></param>
        /// <returns></returns>
        public double Intake(
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities
        ) {
            return IntakesPerCompound.Sum(ipc => ipc.EquivalentSubstanceAmount(relativePotencyFactors[ipc.Compound], membershipProbabilities[ipc.Compound]));
        }

        /// <summary>
        /// All intakes per substance summed and expressed in the desired concentration mass unit.
        /// I.e., if per-person, then divided by the individual's bodyweight, otherwise expressed
        /// per person.
        /// </summary>
        /// <param name="relativePotencyFactors"></param>
        /// <param name="membershipProbabilities"></param>
        /// <param name="isPerPerson"></param>
        /// <returns></returns>
        public double IntakePerMassUnit(
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            bool isPerPerson
        ) {
            if (isPerPerson) {
                return Intake(relativePotencyFactors, membershipProbabilities);
            } else {
                return Intake(relativePotencyFactors, membershipProbabilities) / FoodConsumption.Individual.BodyWeight;
            }
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
