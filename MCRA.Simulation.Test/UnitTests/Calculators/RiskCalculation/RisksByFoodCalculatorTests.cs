using MCRA.Utils.Statistics;
using MCRA.General;
using MCRA.Simulation.Calculators.RiskCalculation;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace MCRA.Simulation.Test.UnitTests.Calculators.RiskCalculation {
    [TestClass]
    public class RisksByFoodCalculatorTests {

        [TestMethod]
        public void RisksByFoodCalculator_ComputeAcute() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var effect = MockEffectsGenerator.Create();
            var substances = MockSubstancesGenerator.Create(5);
            var relativePotencyFactors = substances.ToDictionary(c => c, c => 1d);
            var membershipProbabilities = substances.ToDictionary(c => c, c => 1d);
            var individuals = MockIndividualsGenerator.Create(50, 2, random, useSamplingWeights: true);
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var foodsAsMeasured = MockFoodsGenerator.Create(3);
            var dietaryIndividualDayIntakes = MockDietaryIndividualDayIntakeGenerator.Create(individualDays, foodsAsMeasured, substances, 0, true, random);
            var dietaryExposureUnit = TargetUnit.CreateDietaryExposureUnit(ConsumptionUnit.g, ConcentrationUnit.mgPerKg, BodyWeightUnit.kg, false);
            var hazardCharacterisations = MockHazardCharacterisationModelsGenerator.Create(effect, substances.ToList(), seed);
            var referenceSubstances = substances.First();
            var hazardCharacterisationsUnit = new TargetUnit(ExposureUnit.ugPerKgBWPerDay);

            //Calculate based on dietary exposures, chronic
            var dietaryIndividualDayExposures = dietaryIndividualDayIntakes
                   .AsParallel()
                   .Select(c => new DietaryIndividualDayTargetExposureWrapper(c))
                   .OrderBy(r => r.SimulatedIndividualDayId)
                   .ToList();
            dietaryIndividualDayExposures.ForEach(c => c.IntraSpeciesDraw = random.NextDouble());

            var risksByFoodCalculator = new RisksByFoodCalculator();
            var risksByFood = risksByFoodCalculator.ComputeByModelledFood(
                dietaryIndividualDayExposures,
                hazardCharacterisations,
                relativePotencyFactors,
                membershipProbabilities,
                referenceSubstances,
                dietaryExposureUnit,
                isPerPerson: false
            );
            CollectionAssert.AreEquivalent(risksByFood.Keys, foodsAsMeasured);
            Assert.AreEqual(risksByFood.Values.First().Count, individualDays.Count);
        }

        [TestMethod]
        public void RisksByFoodCalculator_ComputeChronic() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var effect = MockEffectsGenerator.Create();
            var substances = MockSubstancesGenerator.Create(5);
            var relativePotencyFactors = substances.ToDictionary(c => c, c => 1d);
            var membershipProbabilities = substances.ToDictionary(c => c, c => 1d);
            var individuals = MockIndividualsGenerator.Create(50, 2, random, useSamplingWeights: true);
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var foodsAsMeasured = MockFoodsGenerator.Create(3);
            var dietaryIndividualDayIntakes = MockDietaryIndividualDayIntakeGenerator.Create(individualDays, foodsAsMeasured, substances, 0, true, random);
            var dietaryExposureUnit = TargetUnit.CreateDietaryExposureUnit(ConsumptionUnit.g, ConcentrationUnit.mgPerKg, BodyWeightUnit.kg, false);
            var hazardCharacterisations = MockHazardCharacterisationModelsGenerator.Create(effect, substances.ToList(), seed);
            var referenceSubstances = substances.First();
            var hazardCharacterisationsUnit = new TargetUnit(ExposureUnit.ugPerKgBWPerDay);

            //Calculate based on dietary exposures, chronic
            var dietaryIndividualExposures = dietaryIndividualDayIntakes
               .AsParallel()
               .GroupBy(c => c.SimulatedIndividualId)
               .Select(c => new DietaryIndividualTargetExposureWrapper(c.ToList()))
               .OrderBy(r => r.SimulatedIndividualId)
               .ToList();
            dietaryIndividualExposures.ForEach(c => c.IntraSpeciesDraw = random.NextDouble());
            var risksByFoodCalculator = new RisksByFoodCalculator();
            var risksByFood = risksByFoodCalculator.ComputeByModelledFood(
                dietaryIndividualExposures,
                hazardCharacterisations,
                relativePotencyFactors,
                membershipProbabilities,
                referenceSubstances,
                dietaryExposureUnit,
                isPerPerson: false
            );
            CollectionAssert.AreEquivalent(risksByFood.Keys, foodsAsMeasured);
            Assert.AreEqual(risksByFood.Values.First().Count, individuals.Count);
        }
    }
}
