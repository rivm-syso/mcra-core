using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers;

namespace MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation {

    /// <summary>
    /// Contains all information for a single individual-day.
    /// </summary>
    public sealed class DietaryIndividualDayIntake : IIndividualDayIntake {

        private double _sumIntakesPerFood = double.NaN;
        private double _totalExposurePerMassUnit = double.NaN;

        public DietaryIndividualDayIntake() { }

        /// <summary>
        /// The id assigned to the simulated individual.
        /// The sampling weight of the simulated individual.
        /// For ExposureType == acute and numberOfIterations == 0, use samplingweights to determine percentiles (USESAMPLINGWEIGHTS):
        ///   - always correct input,
        ///   - correct output; 
        /// For ExposureType == acute and numberOfIterations > 0, no samplingweights to determine percentiles, weights are already in simulated exposures (DO NOT USESAMPLINGWEIGHTS)
        ///   - always correct input,
        ///   - output is already weighted;
        ///  For ExposureType == chronic (USESAMPLINGWEIGHTS)
        ///   - always correct input,
        ///   - correct output; 
        /// </summary>
        public int SimulatedIndividualId { get; set; }

        /// <summary>
        /// Identifier for a simulated individual day
        /// </summary>
        public int SimulatedIndividualDayId { get; set; }

        /// <summary>
        /// The original Individual entity.
        /// </summary>
        public Individual Individual { get; set; }

        /// <summary>
        /// The exposure day.
        /// </summary>
        public string Day { get; set; }

        /// <summary>
        /// The individual sampling weight.
        /// </summary>
        public double IndividualSamplingWeight { get; set; }

        /// <summary>
        /// Intakes specified per food as eaten.
        /// </summary>
        public List<IIntakePerFood> IntakesPerFood { get; set; }

        /// <summary>
        /// Used in imputation for missing compounds
        /// </summary>
        public List<IIntakePerCompound> OtherIntakesPerCompound { get; set; } = [];

        /// <summary>
        /// The intakes per food that have detailed residue information.
        /// </summary>
        public IEnumerable<IntakePerFood> DetailedIntakesPerFood {
            get {
                return IntakesPerFood
                    .Where(ipf => ipf is IntakePerFood)
                    .Cast<IntakePerFood>();
            }
        }

        /// <summary>
        /// The intakes per food that have detailed residue information.
        /// </summary>
        public IEnumerable<AggregateIntakePerFood> AggregateIntakesPerFood {
            get {
                return IntakesPerFood
                    .Where(ipf => ipf is AggregateIntakePerFood)
                    .Cast<AggregateIntakePerFood>();
            }
        }

        /// <summary>
        /// Sums all (substance) dietary exposures on this individual-day.
        /// </summary>
        /// <param name="relativePotencyFactors"></param>
        /// <param name="membershipProbabilities"></param>
        /// <returns></returns>
        public double TotalExposure(
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities
        ) {
            if (double.IsNaN(_sumIntakesPerFood)) {
                _sumIntakesPerFood = IntakesPerFood.Sum(i => i.Intake(relativePotencyFactors, membershipProbabilities));
            }
            return _sumIntakesPerFood + TotalOtherIntakesPerCompound(relativePotencyFactors, membershipProbabilities);
        }

        /// <summary>
        /// Computes the total dietary (substance)exposures per unit body weight
        /// on this individual-day.
        /// </summary>
        /// <param name="relativePotencyFactors"></param>
        /// <param name="membershipProbabilities"></param>
        /// <param name="isPerPerson"></param>
        /// <returns></returns>
        public double TotalExposurePerMassUnit(
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            bool isPerPerson
        ) {
            if (double.IsNaN(_totalExposurePerMassUnit)) {
                _totalExposurePerMassUnit = TotalExposure(relativePotencyFactors, membershipProbabilities) / (isPerPerson ? 1 : Individual.BodyWeight);
            }
            return _totalExposurePerMassUnit;
        }

        /// <summary>
        /// Returns whether there is any positive substance intake/exposure recorded
        /// in this individual-day intake record.
        /// </summary>
        /// <returns></returns>
        public bool IsPositiveIntake() {
            return IntakesPerFood.Any(r => r.IsPositiveIntake())
                || OtherIntakesPerCompound.Any(r => r.Amount > 0);
        }

        /// <summary>
        /// Calculates total intake for substances (not body weight scaled).
        /// </summary>
        /// <param name="relativePotencyFactors"></param>
        /// <param name="membershipProbabilities"></param>
        /// <returns></returns>
        public double TotalOtherIntakesPerCompound(
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities
        ) {
            return OtherIntakesPerCompound?.Sum(s => s.EquivalentSubstanceAmount(relativePotencyFactors[s.Compound], membershipProbabilities[s.Compound])) ?? 0;
        }

        /// <summary>
        /// Returns the total dietary exposure per bodyweight for the given category of modelled foods.
        /// </summary>
        /// <param name="foods"></param>
        /// <param name="relativePotencyFactors"></param>
        /// <param name="membershipProbabilities"></param>
        /// <param name="isPerPerson"></param>
        /// <returns></returns>
        public double GetTotalDietaryIntakePerMassUnitPerCategory(
            IEnumerable<Food> foods,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            bool isPerPerson
        ) {
            var intakePerCategory = IntakesPerFood
                .Where(c => foods.Contains(c.FoodAsMeasured))
                .GroupBy(ipf => ipf.FoodAsMeasured)
                .Sum(c => c.Sum(i => i.IntakePerMassUnit(relativePotencyFactors, membershipProbabilities, isPerPerson)));
            return intakePerCategory;
        }

        /// <summary>
        /// Returns whether there is any positive substance intake recorded for the specified
        /// modelled foods category.
        /// </summary>
        /// <param name="foodsAsMeasured"></param>
        /// <returns></returns>
        public bool HasPositiveCategoryIntake(ICollection<Food> foodsAsMeasured) {
            var tabuList = foodsAsMeasured.ToHashSet();
            return IntakesPerFood
                .Any(c => tabuList.Contains(c.FoodAsMeasured) && c.IntakesPerCompound.Any(r => r.Amount > 0));
        }

        /// <summary>
        /// Returns the aggregate intake per substance of the dietary individual day intake.
        /// </summary>
        /// <returns></returns>
        public ICollection<IIntakePerCompound> GetTotalIntakesPerSubstance() {
            var intakesPerCompound = IntakesPerFood
                .SelectMany(ipc => ipc.IntakesPerCompound)
                .Concat(OtherIntakesPerCompound)
                .GroupBy(ipc => ipc.Compound)
                .Select(g => new AggregateIntakePerCompound() {
                    Compound = g.Key,
                    Amount = g.Sum(ipc => ipc.Amount),
                })
                .Cast<IIntakePerCompound>()
                .ToList();
            return intakesPerCompound;
        }

        /// <summary>
        /// Returns the aggregate intake per substance of the dietary individual day intake.
        /// </summary>
        /// <returns></returns>
        public List<AggregateIntakePerCompound> GetDietaryIntakesPerSubstance() {
            var intakesPerCompound = IntakesPerFood
                .SelectMany(ipc => ipc.IntakesPerCompound)
                .Concat(OtherIntakesPerCompound)
                .GroupBy(ipc => ipc.Compound)
                .Select(g => new AggregateIntakePerCompound() {
                    Compound = g.Key,
                    Amount = g.Sum(ipc => ipc.Amount),
                })
                .ToList();
            return intakesPerCompound;
        }

        /// <summary>
        /// Returns the total intake of the substance of the dietary individual day intake.
        /// </summary>
        /// <returns></returns>
        public double GetSubstanceTotalExposure(Compound substance) {
            var totalIntake = IntakesPerFood
                .SelectMany(ipf => ipf.IntakesPerCompound.Where(ipc => ipc.Compound == substance))
                .Sum(r => r.Amount);
            totalIntake += OtherIntakesPerCompound.Where(r => r.Compound == substance).Sum(r => r.Amount);
            return totalIntake;
        }

        /// <summary>
        /// Returns the total intake of the substance of the dietary individual day intake.
        /// </summary>
        /// <returns></returns>
        public double GetSubstanceTotalExposureCoExposure(Compound substance) {
            var totalIntake = IntakesPerFood
                .Where(c => c.IsPositiveIntake())
                .Select(ipf => ipf.IntakesPerCompound.Where(ipc => ipc.Compound == substance))
                .Sum(r => r.Sum(s => s.Amount));
            totalIntake += OtherIntakesPerCompound.Where(r => r.Compound == substance).Sum(r => r.Amount);
            return totalIntake;
        }

        /// <summary>
        /// Computes the total dietary substance exposures per mass-unit on this individual-day.
        /// </summary>
        /// <param name="substance"></param>
        /// <param name="isPerPerson"></param>
        /// <returns></returns>
        public double GetSubstanceTotalExposurePerMassUnit(
            Compound substance,
            bool isPerPerson
        ) {
            var result = GetSubstanceTotalExposure(substance) / (isPerPerson ? 1D : Individual.BodyWeight);
            return result;
        }

        /// <summary>
        /// Returns whether there is co-exposure for this individual day intake.
        /// </summary>
        /// <returns></returns>
        public bool HasCoExposure() {
            return IntakesPerFood
                .SelectMany(ipc => ipc.IntakesPerCompound)
                .Where(r => r.Amount > 0)
                .Concat(OtherIntakesPerCompound)
                .Where(r => r.Amount > 0)
                .Select(r => r.Compound)
                .Distinct()
                .Skip(1).Any();
        }

        /// <summary>
        /// Calculate exposure per modelled food
        /// </summary>
        /// <param name="relativePotencyFactors"></param>
        /// <param name="membershipProbabilities"></param>
        /// <param name="isPerPerson"></param>
        /// <returns></returns>
        public IDictionary<Food, IIntakePerModelledFood> GetModelledFoodTotalExposures(
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            bool isPerPerson
        ) {
            var intakePerModelledFood = IntakesPerFood
                .SelectMany(c => c.IntakesPerCompound,
                    (c, ipc) => (
                        food: c.FoodAsMeasured,
                        intakePerCompound: ipc.EquivalentSubstanceAmount(relativePotencyFactors[ipc.Compound], membershipProbabilities[ipc.Compound]) / (isPerPerson ? 1 : Individual.BodyWeight)
                    ))
                .GroupBy(c => c.food)
                .Select(c => new IntakePerModelledFood() {
                    ModelledFood = c.Key,
                    Exposure = c.Sum(r => r.intakePerCompound)
                })
                .Cast<IIntakePerModelledFood>()
                .ToDictionary(c => c.ModelledFood);
            return intakePerModelledFood;
        }

        /// <summary>
        /// Calculate exposure per modelled food x substance
        /// </summary>
        /// <param name="relativePotencyFactors"></param>
        /// <param name="membershipProbabilities"></param>
        /// <param name="isPerPerson"></param>
        /// <returns></returns>
        public IDictionary<(Food, Compound), IIntakePerModelledFoodSubstance> GetModelledFoodSubstanceTotalExposures(
           IDictionary<Compound, double> relativePotencyFactors,
           IDictionary<Compound, double> membershipProbabilities,
           bool isPerPerson
       ) {
            var result = new Dictionary<(Food, Compound), IIntakePerModelledFoodSubstance>();
            var intakePerModelledFoodSubstance = IntakesPerFood
                .SelectMany(c => c.IntakesPerCompound,
                    (c, ipc) => (
                        food: c.FoodAsMeasured,
                        substance: ipc.Compound,
                        intakePerSubstance: ipc.EquivalentSubstanceAmount(relativePotencyFactors[ipc.Compound], membershipProbabilities[ipc.Compound]) / (isPerPerson ? 1 : Individual.BodyWeight)
                    ))
                .GroupBy(c => (c.food, c.substance))
                .Select(c => new IntakePerModelledFoodSubstance() {
                    Substance = c.Key.substance,
                    ModelledFood = c.Key.food,
                    Exposure = c.Sum(r => r.intakePerSubstance)
                })
                .Cast<IIntakePerModelledFoodSubstance>()
                .ToList();

            foreach (var item in intakePerModelledFoodSubstance) {
                result[(item.ModelledFood, item.Substance)] = item;
            }
            return result;
        }
    }
}
