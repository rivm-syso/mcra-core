using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Objects;
using MCRA.Simulation.Calculators.DietaryExposureCalculation.IndividualDietaryExposureCalculation;
using MCRA.Simulation.Calculators.DietaryExposureCalculation.MaximalSubstanceIntakesCorrelationCalculation;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;

namespace MCRA.Simulation.Test.UnitTests.Calculators.DietaryExposuresCalculation.MaximalSubstanceIntakesCorrelationCalculation {
    [TestClass]
    public class MaximalSubstanceIntakesCorrelationCalculatorTests {

        [TestMethod]
        public void MaximalSubstanceIntakesCorrelationCalculator_Test1() {
            var food = FakeFoodsGenerator.Create(1).First();
            var substances = FakeSubstancesGenerator.Create(3);
            var inputPatterns = new double[,] {
                { 3, 4, 0 },
                { 1, 0, 3 },
                { 5, 2, 0 },
                { 6, 0, 1 }
            };
            var checkPatterns = new double[,] {
                { 3, 2, 0 },
                { 1, 0, 1 },
                { 5, 4, 0 },
                { 6, 0, 3 }
            };
            testInputOutputPatterns(food, substances, inputPatterns, checkPatterns);
        }

        [TestMethod]
        public void MaximalSubstanceIntakesCorrelationCalculator_Test2() {
            var food = FakeFoodsGenerator.Create(1).First();
            var substances = FakeSubstancesGenerator.Create(3);
            var inputPatterns = new double[,] {
                { 0, 0, 0 },
                { 0, 0, 0 },
                { 1, 2, 3 },
                { 3, 2, 1 }
            };
            var checkPatterns = new double[,] {
                { 0, 0, 0 },
                { 0, 0, 0 },
                { 1, 2, 1 },
                { 3, 2, 3 }
            };
            testInputOutputPatterns(food, substances, inputPatterns, checkPatterns);
        }

        private static void testInputOutputPatterns(Food food,
            List<Compound> substances,
            double[,] inputPatterns,
            double[,] checkPatterns
        ) {
            var intakes = new List<DietaryIndividualDayIntake>();
            for (int i = 0; i < inputPatterns.GetLength(0); i++) {
                var pattern = Enumerable.Range(0, inputPatterns.GetLength(1))
                    .Select(r => inputPatterns[i, r])
                    .ToArray();
                var intake = new DietaryIndividualDayIntake() {
                    IntakesPerFood = [
                        fakeIntakePerFood(food, substances, 0.2, pattern)
                    ]
                };
                intakes.Add(intake);
            }

            MaximalSubstanceIntakesCorrelationCalculator.Compute(intakes, null);

            var flat = intakes
                .SelectMany(r => r.DetailedIntakesPerFood)
                .ToList();
            for (int i = 0; i < flat.Count; i++) {
                var pattern = substances
                    .Select(s => flat[i].DetailedIntakesPerCompound.FirstOrDefault(r => r.Compound == s)?.MeanConcentration ?? 0)
                    .ToArray()
                    .ToList();
                var checkPattern = Enumerable.Range(0, checkPatterns.GetLength(1))
                    .Select(r => checkPatterns[i, r])
                    .ToArray();
                CollectionAssert.AreEquivalent(checkPattern, pattern);
                System.Diagnostics.Trace.WriteLine(string.Join(",", pattern));
            }
        }

        private static IntakePerFood fakeIntakePerFood(
            Food food,
            List<Compound> substances,
            double amount,
            double[] concentrations
        ) {
            return new IntakePerFood() {
                ConsumptionFoodAsMeasured = new ConsumptionsByModelledFood() {
                    FoodAsMeasured = food,
                    AmountFoodAsMeasured = amount
                },
                IntakesPerCompound = concentrations
                    .Select((r, ix) => new DietaryIntakePerCompound() {
                        Compound = substances[ix],
                        IntakePortion = new IntakePortion() {
                            Amount = amount,
                            Concentration = (float)r
                        }
                    })
                    .Where(r => r.MeanConcentration > 0)
                    .Cast<IIntakePerCompound>()
                    .ToList()
            };
        }
    }
}
