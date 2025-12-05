using MCRA.Utils.Statistics;
using MCRA.General;
using MCRA.Simulation.Calculators.RiskCalculation;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;

namespace MCRA.Simulation.Test.UnitTests.Calculators.RiskCalculation {
    [TestClass]
    public class RisksByFoodCalculatorTests {

        [TestMethod]
        public void RisksByFoodCalculator_ComputeAcute() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var effect = FakeEffectsGenerator.Create();
            var substances = FakeSubstancesGenerator.Create(5);
            var relativePotencyFactors = substances.ToDictionary(c => c, c => 1d);
            var membershipProbabilities = substances.ToDictionary(c => c, c => 1d);
            var individuals = FakeIndividualsGenerator.Create(50, 2, random, useSamplingWeights: true);
            var individualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var foodsAsMeasured = FakeFoodsGenerator.Create(3);
            var dietaryIndividualDayIntakes = FakeDietaryIndividualDayIntakeGenerator.Create(individualDays, foodsAsMeasured, substances, 0, true, random);
            var dietaryExposureUnit = TargetUnit.FromExternalExposureUnit(ExternalExposureUnit.ugPerKgBWPerDay);
            var hazardCharacterisations = FakeHazardCharacterisationModelsGenerator.Create(effect, substances.ToList(), seed);
            var referenceSubstances = substances.First();
            var hazardCharacterisationsUnit = TargetUnit.FromExternalExposureUnit(ExternalExposureUnit.ugPerKgBWPerDay);

            //Calculate based on dietary exposures, chronic
            var dietaryIndividualDayExposures = dietaryIndividualDayIntakes
                   .AsParallel()
                   .Select(c => new DietaryIndividualDayTargetExposureWrapper(c, dietaryExposureUnit.ExposureUnit))
                   .OrderBy(r => r.SimulatedIndividualDayId)
                   .ToList();
            dietaryIndividualDayExposures.ForEach(c => c.IntraSpeciesDraw = random.NextDouble());

            var risksByFoodCalculator = new RisksByFoodCalculator(HealthEffectType.Risk);
            var risksByFood = risksByFoodCalculator.ComputeByModelledFood(
                dietaryIndividualDayExposures,
                dietaryExposureUnit,
                hazardCharacterisations,
                hazardCharacterisationsUnit,
                relativePotencyFactors,
                membershipProbabilities,
                foodsAsMeasured,
                referenceSubstances
            );
            CollectionAssert.AreEquivalent(risksByFood.Keys, foodsAsMeasured);
            Assert.HasCount(risksByFood.Values.First().Count, individualDays);
            Assert.IsTrue(risksByFood.Values.All(rbf => rbf.All(r => !double.IsNaN(r.HazardExposureRatio))));
            Assert.IsTrue(risksByFood.Values.All(rbf => rbf.All(r => !double.IsNaN(r.ExposureHazardRatio))));
        }

        [TestMethod]
        public void RisksByFoodCalculator_ComputeChronic() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var effect = FakeEffectsGenerator.Create();
            var substances = FakeSubstancesGenerator.Create(5);
            var relativePotencyFactors = substances.ToDictionary(c => c, c => 1d);
            var membershipProbabilities = substances.ToDictionary(c => c, c => 1d);
            var individuals = FakeIndividualsGenerator.Create(50, 2, random, useSamplingWeights: true);
            var individualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var foodsAsMeasured = FakeFoodsGenerator.Create(3);
            var dietaryIndividualDayIntakes = FakeDietaryIndividualDayIntakeGenerator.Create(individualDays, foodsAsMeasured, substances, 0, true, random);
            var dietaryExposureUnit = TargetUnit.FromExternalExposureUnit(ExternalExposureUnit.ugPerKgBWPerDay);
            var hazardCharacterisations = FakeHazardCharacterisationModelsGenerator.Create(effect, substances, seed);
            var referenceSubstances = substances.First();
            var hazardCharacterisationsUnit = TargetUnit.FromExternalExposureUnit(ExternalExposureUnit.ugPerKgBWPerDay);

            //Calculate based on dietary exposures, chronic
            var dietaryIndividualExposures = dietaryIndividualDayIntakes
               .AsParallel()
               .GroupBy(c => c.SimulatedIndividual.Id)
               .Select(c => new DietaryIndividualTargetExposureWrapper(c.ToList(), dietaryExposureUnit.ExposureUnit))
               .OrderBy(r => r.SimulatedIndividual.Id)
               .ToList();
            dietaryIndividualExposures.ForEach(c => c.IntraSpeciesDraw = random.NextDouble());
            var risksByFoodCalculator = new RisksByFoodCalculator(HealthEffectType.Risk);
            var risksByFood = risksByFoodCalculator.ComputeByModelledFood(
                dietaryIndividualExposures,
                dietaryExposureUnit,
                hazardCharacterisations,
                hazardCharacterisationsUnit,
                relativePotencyFactors,
                membershipProbabilities,
                foodsAsMeasured,
                referenceSubstances
            );
            CollectionAssert.AreEquivalent(risksByFood.Keys, foodsAsMeasured);
            Assert.HasCount(risksByFood.Values.First().Count, individuals);
            Assert.IsTrue(risksByFood.Values.All(rbf => rbf.All(r => !double.IsNaN(r.HazardExposureRatio))));
            Assert.IsTrue(risksByFood.Values.All(rbf => rbf.All(r => !double.IsNaN(r.ExposureHazardRatio))));
        }
    }
}
