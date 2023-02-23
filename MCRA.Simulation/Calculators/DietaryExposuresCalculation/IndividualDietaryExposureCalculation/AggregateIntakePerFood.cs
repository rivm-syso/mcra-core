using MCRA.Data.Compiled.Objects;

namespace MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation {

    /// <summary>
    /// Summarizes all info for a consumed modelled food: the food as eaten, the consumed amount, the exposures per substance.
    /// </summary>
    public sealed class AggregateIntakePerFood : IIntakePerFood {

        /// <summary>
        /// The modelled food of this intake.
        /// </summary>
        public Food FoodAsMeasured { get; set; }

        /// <summary>
        /// The consumed amounts of the possitive intakes of this intake per food
        /// that were not modelled by a detailed intake per food.
        /// </summary>
        public double GrossAmount { get; set; }

        /// <summary>
        /// The consumed amounts of the possitive intakes of this intake per food
        /// that were not modelled by a detailed intake per food.
        /// </summary>
        public double NetAmount { get; set; }

        /// <summary>
        /// The bodyweight
        /// </summary>
        public double BodyWeight { get; set; }

        /// <summary>
        /// The total compound exposure per modelled food.
        /// </summary>
        public List<IIntakePerCompound> IntakesPerCompound { get; set; }

        /// <summary>
        /// All IntakesPerCompound summed.
        /// </summary>
        public double Intake(IDictionary<Compound, double> relativePotencyFactors, IDictionary<Compound, double> membershipProbabilities) {
            return IntakesPerCompound.Sum(ipc => ipc.Intake(relativePotencyFactors[ipc.Compound], membershipProbabilities[ipc.Compound]));
        }

        /// <summary>
        /// The proportion of FoodAsMeasured in FoodConversionResult multiplied by the Amount consumed (in FoodConsumption).
        /// </summary>
        public double Amount {
            get {
                return GrossAmount;
            }
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
                return Intake(relativePotencyFactors, membershipProbabilities) / BodyWeight;
            }
        }

        /// <summary>
        /// Specifies if there is any positive substance exposure present in this food-intake.
        /// </summary>
        /// <returns></returns>
        public bool IsPositiveIntake() {
            return IntakesPerCompound.Any(r => r.Exposure > 0);
        }
    }
}
