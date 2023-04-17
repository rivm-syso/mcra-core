using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.KineticModelCalculation;
using MCRA.Simulation.Calculators.RiskCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation.TargetExposuresCalculators;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.Calculators.RiskCalculation {

    /// <summary>
    /// RiskCalculation calculator
    /// </summary>
    [TestClass]
    public class ChronicRiskCalculatorTests {

        /// <summary>
        /// Calculate risk based on rpfs and memberships: chronic, cumulative, RPF weighted
        /// </summary>
        [TestMethod]
        public void ChronicRiskCalculator_TestCumulativeRPFWeighted() {
            int seed = 1;
            var random = new McraRandomGenerator(seed);
            var substances = MockSubstancesGenerator.Create(4);
            var referenceSubstance = substances.First();
            var individuals = MockIndividualsGenerator.Create(25, 2, random, useSamplingWeights: true);
            var individualDays = MockIndividualDaysGenerator.Create(individuals);
            var pointsOfDeparture = MockHazardCharacterisationModelsGenerator.Create(new Effect(), substances, seed);
            var rpfs = pointsOfDeparture.ToDictionary(r => r.Key, r => pointsOfDeparture[referenceSubstance].Value / r.Value.Value);
            var memberships = substances.ToDictionary(r => r, r => 1d);
            var exposures = MockTargetExposuresGenerator.MockIndividualExposures(individuals, substances, random);

            var effectCalculator = new RiskCalculator<ITargetIndividualExposure>();
            exposures.ForEach(c => c.IntraSpeciesDraw = random.NextDouble());
            var individualEffects = effectCalculator.ComputeCumulative(
                exposures,
                pointsOfDeparture,
                rpfs,
                memberships,
                referenceSubstance,
                new TargetUnit(ExposureUnit.ugPerKgBWPerDay),
                HealthEffectType.Risk,
                false
            );

            Assert.AreEqual(25, individualEffects.Count);
        }

        /// <summary>
        /// Calculate risk based on rpfs and memberships: chronic, cumulative, Sum of ratios
        /// </summary>
        [TestMethod]
        public void ChronicRiskCalculator_TestCumulativeSumOfRatios() {
            int seed = 1;
            var random = new McraRandomGenerator(seed);
            var substances = MockSubstancesGenerator.Create(4);
            var referenceSubstance = substances.First();
            var individuals = MockIndividualsGenerator.Create(25, 2, random, useSamplingWeights: true);
            var individualDays = MockIndividualDaysGenerator.Create(individuals);
            var memberships = substances.ToDictionary(r => r, r => 1d);
            var exposures = MockTargetExposuresGenerator.MockIndividualExposures(individuals, substances, random);
            var hazardCharacterisations = MockHazardCharacterisationModelsGenerator.Create(new Effect(), substances, seed);
            var effectCalculator = new RiskCalculator<ITargetIndividualExposure>();
            exposures.ForEach(c => c.IntraSpeciesDraw = random.NextDouble());
            var individualEffectsDictionary = effectCalculator.ComputeBySubstance(
                    exposures,
                    hazardCharacterisations,
                    substances,
                    new TargetUnit(ExposureUnit.ugPerKgBWPerDay),
                    HealthEffectType.Risk,
                    false
                );
            var individualEffects = effectCalculator.ComputeSumOfRatios(
                individualEffectsDictionary,
                memberships,
                HealthEffectType.Risk
            );

            Assert.AreEqual(25, individualEffects.Count);
        }

        /// <summary>
        /// Calculate risk based on intraspecies factor models: chronic, cumulative
        /// </summary>
        [TestMethod]
        public void ChronicRiskCalculator_TestMultiple() {
            int seed = 1;
            var random = new McraRandomGenerator(seed);
            var substances = MockSubstancesGenerator.Create(100);
            var referenceSubstance = substances.First();
            var individuals = MockIndividualsGenerator.Create(100, 2, random, useSamplingWeights: true);
            var hazardCharacterisations = MockHazardCharacterisationModelsGenerator.Create(new Effect(), substances, seed);
            var exposures = MockTargetExposuresGenerator.MockIndividualExposures(individuals, substances, random);
            exposures.ForEach(c => c.IntraSpeciesDraw = random.NextDouble());
            var sum = 0d;
            var ncounter = 10;
            for (int i = 0; i < ncounter; i++) {
                var effectCalculator = new RiskCalculator<ITargetIndividualExposure>();
                var individualEffectsDictionary = effectCalculator.ComputeBySubstance(
                    exposures,
                    hazardCharacterisations,
                    substances,
                    new TargetUnit(ExposureUnit.ugPerKgBWPerDay),
                    HealthEffectType.Risk,
                    false
                );

                foreach (var substance in substances) {
                    sum += individualEffectsDictionary[substance].Sum(c => c.MarginOfExposure);
                }

                Assert.AreEqual(individuals.Count, individualEffectsDictionary.First().Value.Count);
            }
            var test = sum / ncounter;
            Assert.AreEqual(13668877.698889038, test, 0.001);
        }

        /// <summary>
        /// The dietary exposure risk action for external dose levels.
        /// Generates dietary exposures, converts them to target exposures and compares both results in the risk caculator.
        /// Compares dietary risk with target exposure risks.
        /// Chronic, TargetDoseLevelType = TargetDoseLevelType.External
        /// </summary>
        [TestMethod]
        public void RiskActionCalculator_TestChronicExternal() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var substances = MockSubstancesGenerator.Create(5);
            var correctedRelativePotencyFactors = substances.ToDictionary(c => c, c => 1d);
            var membershipProbabilities = substances.ToDictionary(c => c, c => 1d);
            var individuals = MockIndividualsGenerator.Create(50, 2, random, useSamplingWeights: true);
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var foodsAsMeasured = MockFoodsGenerator.Create(3);
            var dietaryIndividualDayIntakes = MockDietaryIndividualDayIntakeGenerator.Create(individualDays, foodsAsMeasured, substances, 0, true, random);
            var dietaryExposureUnit = TargetUnit.CreateDietaryExposureUnit(ConsumptionUnit.g, ConcentrationUnit.mgPerKg, BodyWeightUnit.kg, false);
            var hazardCharacterisations = MockHazardCharacterisationModelsGenerator.Create(new Effect() { Code = "code" }, substances.ToList(), seed);
            var referenceSubstances = substances.First();
            var hazardCharacterisationsUnit = new TargetUnit(ExposureUnit.ugPerKgBWPerDay);

            //Calculate based on dietary exposures, chronic
            var dietaryIndividualExposures = dietaryIndividualDayIntakes
               .AsParallel()
               .GroupBy(c => c.SimulatedIndividualId)
               .Select(c => new DietaryIndividualTargetExposureWrapper(c.ToList()))
               .OrderBy(r => r.SimulatedIndividualId)
               .ToList();

            var iec = new RiskCalculator<ITargetIndividualExposure>();
            var exposures = dietaryIndividualExposures.Cast<ITargetIndividualExposure>().ToList();
            exposures.ForEach(c => c.IntraSpeciesDraw = random.NextDouble());
            var cumulativeIndividualEffects1 = iec.ComputeCumulative(
                exposures,
                hazardCharacterisations,
                correctedRelativePotencyFactors,
                membershipProbabilities,
                referenceSubstances,
                hazardCharacterisationsUnit,
                HealthEffectType.Risk,
                isPerPerson: false
            );
            var dietaryExposureSum = cumulativeIndividualEffects1.Sum(c => c.ExposureConcentration);

            var individualEffectsBySubstance1 = iec.ComputeBySubstance(
                exposures,
                hazardCharacterisations,
                substances,
                hazardCharacterisationsUnit,
                HealthEffectType.Risk,
                isPerPerson: false
            );

            // Create target exposure individual day exposures based on dietary exposures
            var aggregateIndividualDayExposures = AggregateIntakeCalculator.CreateAggregateIndividualDayExposures(
                dietaryIndividualDayIntakes,
                null,
                new List<ExposureRouteType>() { ExposureRouteType.Dietary }
            );

            // Create aggregate individual exposures
            var aggregateIndividualExposures = AggregateIntakeCalculator.CreateAggregateIndividualExposures(
                aggregateIndividualDayExposures
            );
            foreach (var item in exposures) {
                aggregateIndividualExposures.Single(c => c.SimulatedIndividualId == item.SimulatedIndividualId).IntraSpeciesDraw = item.IntraSpeciesDraw;
            }
            var targetExposuresCalculator = new ExternalTargetExposuresCalculator();
            var targetIndividualExposures2 = targetExposuresCalculator.ComputeTargetIndividualExposures(
                aggregateIndividualExposures.Cast<IExternalIndividualExposure>().ToList(),
                substances,
                referenceSubstances,
                new List<ExposureRouteType> { ExposureRouteType.Dietary },
                dietaryExposureUnit,
                null,
                null,
                new ProgressState()
            ).ToDictionary(c => c.SimulatedIndividualId, c => c);

            aggregateIndividualExposures.ForEach(c => c.TargetExposuresBySubstance = targetIndividualExposures2[c.SimulatedIndividualId].TargetExposuresBySubstance);

            exposures = aggregateIndividualExposures.Cast<ITargetIndividualExposure>().ToList();
            exposures.ForEach(c => c.IntraSpeciesDraw = random.NextDouble());
            var cumulativeIndividualEffects2 = iec.ComputeCumulative(
                exposures,
                hazardCharacterisations,
                correctedRelativePotencyFactors,
                membershipProbabilities,
                referenceSubstances,
                hazardCharacterisationsUnit,
                HealthEffectType.Risk,
                isPerPerson: false
            );
            var targetExposureSum = cumulativeIndividualEffects2.Sum(c => c.ExposureConcentration);
            var individualEffectsBySubstance2 = iec.ComputeBySubstance(
                exposures,
                hazardCharacterisations,
                substances,
                hazardCharacterisationsUnit,
                HealthEffectType.Risk,
                isPerPerson: false
            );

            Assert.AreEqual(dietaryExposureSum, targetExposureSum, 1e-4);
            foreach (var substance in substances) {
                var dietaryExposureBySubstanceSum = individualEffectsBySubstance1[substance].Sum(c => c.ExposureConcentration);
                var targetExposureBySubstanceSum = individualEffectsBySubstance2[substance].Sum(c => c.ExposureConcentration);
                Assert.AreEqual(dietaryExposureBySubstanceSum, targetExposureBySubstanceSum, 1e-4);
            }
        }
    }
}
