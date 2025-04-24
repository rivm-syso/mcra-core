using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Objects;
using MCRA.Simulation.Calculators.DietaryExposureCalculation.IndividualDietaryExposureCalculation;

namespace MCRA.Simulation.Test.Mock.FakeDataGenerators {
    /// <summary>
    /// Class for generating mock dietary individual day intakes
    /// </summary>
    public static class FakeDietaryIndividualDayIntakeGenerator {

        internal class MockIntakePerFood : IIntakePerFood {
            public SimulatedIndividual SimulatedIndividual { get; set; }
            public double Amount { get; set; }
            public Food FoodAsMeasured { get; set; }
            public List<IIntakePerCompound> IntakesPerCompound { get; set; }

            public double Intake(IDictionary<Compound, double> relativePotencyFactors, IDictionary<Compound, double> membershipProbabilities) {
                return IntakesPerCompound.Sum(ipc => ipc.EquivalentSubstanceAmount(relativePotencyFactors[ipc.Compound], membershipProbabilities[ipc.Compound]));
            }

            public double IntakePerMassUnit(IDictionary<Compound, double> relativePotencyFactors, IDictionary<Compound, double> membershipProbabilities, bool isPerPerson) {
                return Intake(relativePotencyFactors, membershipProbabilities) / (isPerPerson ? 1 : SimulatedIndividual.BodyWeight);
            }

            public bool IsPositiveIntake() {
                return IntakesPerCompound.Any(r => r.Amount > 0);
            }
        }

        /// <summary>
        /// Creates dietary individual intake days (no imputation)
        /// </summary>
        /// <param name="simulatedIndividualDays"></param>
        /// <param name="foods"></param>
        /// <param name="compounds"></param>
        /// <param name="fractionZeros"></param>
        /// <param name="isDetailed"></param>
        /// <param name="random"></param>
        /// <param name="isAggregate">Return an AggregateIntakePerCompound or a dietaryIntakePerCompound</param>
        /// <returns></returns>
        public static List<DietaryIndividualDayIntake> Create(
            ICollection<SimulatedIndividualDay> simulatedIndividualDays,
            ICollection<Food> foods,
            ICollection<Compound> compounds,
            double fractionZeros,
            bool isDetailed,
            IRandom random,
            bool isAggregate = true
        ) {
            var individualDayIntakes = new List<DietaryIndividualDayIntake>();
            for (int i = 0; i < simulatedIndividualDays.Count; i++) {
                var individualDay = simulatedIndividualDays.ElementAt(i);
                var mockIntakesPerFood = isDetailed
                    ? createMockDetailedIntakesPerFoods(foods, compounds, individualDay, random, isAggregate)
                    : createMockIntakesPerFoods(foods, compounds, individualDay, random);
                if (random.NextDouble() < fractionZeros) {
                    mockIntakesPerFood = [];
                }
                var othersIntakesPerCompounds = new List<AggregateIntakePerCompound>() { }.Cast<IIntakePerCompound>().ToList();
                var individiualDayIntake = new DietaryIndividualDayIntake() {
                    SimulatedIndividualDayId = individualDay.SimulatedIndividualDayId,
                    SimulatedIndividual = individualDay.SimulatedIndividual,
                    Day = individualDay.Day,
                    IntakesPerFood = mockIntakesPerFood,
                    OtherIntakesPerCompound = othersIntakesPerCompounds
                };
                individualDayIntakes.Add(individiualDayIntake);
            }

            return individualDayIntakes;
        }

        /// <summary>
        /// Creates  dietary individual intake days with one imputed compound
        /// </summary>
        /// <param name="simulatedIndividualDays"></param>
        /// <param name="foods"></param>
        /// <param name="compounds"></param>
        /// <param name="fractionZeros"></param>
        /// <param name="isDetailed"></param>
        /// <param name="random"></param>
        /// <returns></returns>
        public static List<DietaryIndividualDayIntake> GenerateImputed(
            List<SimulatedIndividualDay> simulatedIndividualDays,
            List<Food> foods,
            List<Compound> compounds,
            double fractionZeros,
            bool isDetailed,
            IRandom random
        ) {
            var individualDayIntakes = new List<DietaryIndividualDayIntake>();
            var numberOfCompounds = compounds.Count;
            for (int i = 0; i < simulatedIndividualDays.Count; i++) {
                var individualDay = simulatedIndividualDays.ElementAt(i);
                var mockIntakesPerFood = isDetailed
                    ? createMockDetailedIntakesPerFoods(foods, compounds.Take(numberOfCompounds - 1).ToList(), individualDay, random)
                    : createMockIntakesPerFoods(foods, compounds.Take(numberOfCompounds - 1).ToList(), individualDay, random);
                if (random.NextDouble() < fractionZeros) {
                    mockIntakesPerFood = [];
                }
                var othersIntakesPerCompounds = new List<AggregateIntakePerCompound>(){ new() {
                    Compound = compounds.Last(),
                    Amount = random.NextDouble() * 10,
                }
                }.Cast<IIntakePerCompound>().ToList();

                var individualDayIntake = new DietaryIndividualDayIntake() {
                    SimulatedIndividualDayId = individualDay.SimulatedIndividualDayId,
                    SimulatedIndividual = individualDay.SimulatedIndividual,
                    Day = individualDay.Day,
                    IntakesPerFood = mockIntakesPerFood,
                    OtherIntakesPerCompound = othersIntakesPerCompounds
                };
                individualDayIntakes.Add(individualDayIntake);
            }

            return individualDayIntakes;
        }

        /// <summary>
        /// Creates intakes per foods
        /// </summary>
        /// <param name="foods"></param>
        /// <param name="compounds"></param>
        /// <param name="simulatedIndividualDay"></param>
        /// <param name="random"></param>
        /// <returns></returns>
        private static List<IIntakePerFood> createMockIntakesPerFoods(
            ICollection<Food> foods,
            ICollection<Compound> compounds,
            SimulatedIndividualDay simulatedIndividualDay,
            IRandom random
        ) {
            return foods.Select(r =>
                new MockIntakePerFood() {
                    SimulatedIndividual = simulatedIndividualDay.SimulatedIndividual,
                    Amount = 100,
                    FoodAsMeasured = r,
                    IntakesPerCompound = compounds.Select(c => new AggregateIntakePerCompound() {
                        Compound = c,
                        Amount = random.NextDouble() * 10,
                    }).Cast<IIntakePerCompound>().ToList()
                }
            ).Cast<IIntakePerFood>().ToList();
        }

        /// <summary>
        /// Creates detailed intakes per food
        /// </summary>
        /// <param name="foods"></param>
        /// <param name="compounds"></param>
        /// <param name="simulatedIndividualDay"></param>
        /// <param name="random"></param>
        /// <param name="isAggregateIntake"></param>
        /// <returns></returns>
        private static List<IIntakePerFood> createMockDetailedIntakesPerFoods(
            ICollection<Food> foods,
            ICollection<Compound> compounds,
            SimulatedIndividualDay simulatedIndividualDay,
            IRandom random,
            bool isAggregateIntake = true
        ) {

            return foods
                .Select(r => {
                    var consumption = new FoodConsumption() {
                        Food = r,
                        Amount = random.NextDouble() * 100,
                        IndividualDay = new IndividualDay() {
                            Individual = simulatedIndividualDay.SimulatedIndividual.Individual,
                            IdDay = simulatedIndividualDay.Day
                        }
                    };
                    var conversionResult = new Dictionary<Compound, FoodConversionResult>();
                    foreach (var compound in compounds) {
                        conversionResult.Add(compound, new FoodConversionResult() { Compound = compound, FoodAsMeasured = r });
                    }
                    var result = new List<IIntakePerCompound>();

                    if (isAggregateIntake) {
                        result = compounds.Select(c => new AggregateIntakePerCompound() {
                            Compound = c,
                            Amount = (float)random.NextDouble() * 10,

                        }).Cast<IIntakePerCompound>().ToList();
                    } else {
                        result = compounds.Select(c => new DietaryIntakePerCompound() {
                            Compound = c,
                            IntakePortion = new IntakePortion() {
                                Amount = (float)random.NextDouble() * 10,
                                Concentration = (float)random.NextDouble() * .01f,
                            },
                            ProcessingFactor = 1,
                            ProcessingCorrectionFactor = 1,
                        }).Cast<IIntakePerCompound>().ToList();
                    }
                    return new IntakePerFood() {
                        Amount = random.NextDouble() * 70,
                        ConsumptionFoodAsMeasured = new ConsumptionsByModelledFood() {
                            FoodConsumption = consumption,
                            FoodAsMeasured = r,
                            AmountFoodAsMeasured = (float)random.NextDouble() * 50,
                            IndividualDay = new IndividualDay() {
                                Individual = simulatedIndividualDay.SimulatedIndividual.Individual,
                                IdDay = simulatedIndividualDay.Day
                            },
                            ConversionResultsPerCompound = conversionResult,
                        },
                        IntakesPerCompound = result,
                    };
                })
                .Cast<IIntakePerFood>().ToList();
        }
    }
}
