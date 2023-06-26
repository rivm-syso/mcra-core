using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.KineticModelCalculation;
using MCRA.Simulation.Calculators.RiskCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation.TargetExposuresCalculators;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.Calculators.RiskCalculation {

    /// <summary>
    /// RiskCalculation calculator
    /// </summary>
    [TestClass]
    public class AcuteRiskCalculatorTests {

        /// <summary>
        /// Calculate risk based on rpfs and memberships: acute, cumulative
        /// </summary>
        [TestMethod]
        public void AcuteRiskCalculator_TestCumulative() {
            int seed = 1;
            var random = new McraRandomGenerator(seed);
            var substances = MockSubstancesGenerator.Create(4);
            var referenceSubstance = substances.First();
            var individuals = MockIndividualsGenerator.Create(25, 2, random, useSamplingWeights: true);
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var exposures = MockTargetExposuresGenerator.MockIndividualDayExposures(individualDays, substances, random);
            var pointsOfDeparture = MockHazardCharacterisationModelsGenerator.Create(new Effect(), substances, seed);
            var rpfs = pointsOfDeparture.ToDictionary(r => r.Key, r => pointsOfDeparture[referenceSubstance].Value / r.Value.Value);
            var memberships = substances.ToDictionary(r => r, r => 1d);
            var intraSpeciesFactorModels = MockIntraSpeciesFactorModelsGenerator.Create(substances);
            exposures.ForEach(c => c.IntraSpeciesDraw = random.NextDouble());
            var effectCalculator = new RiskCalculator<ITargetIndividualDayExposure>();
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
            Assert.AreEqual(50, individualEffects.Count);
        }

        /// <summary>
        /// Calculate risk based on intraspecies factor models: acute, cumulative
        /// </summary>
        [TestMethod]
        public void AcuteRiskCalculator_TestMultiple() {
            int seed = 1;
            var random = new McraRandomGenerator(seed);
            var substances = MockSubstancesGenerator.Create(4);
            var individuals = MockIndividualsGenerator.Create(25, 2, random, useSamplingWeights: true);
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var exposures = MockTargetExposuresGenerator.MockIndividualDayExposures(individualDays, substances, random);
            var hazardCharacterisations = MockHazardCharacterisationModelsGenerator.Create(new Effect(), substances, seed);
            exposures.ForEach(c => c.IntraSpeciesDraw = random.NextDouble());
            var effectCalculator = new RiskCalculator<ITargetIndividualDayExposure>();
            var individualEffectsDictionary = effectCalculator.ComputeBySubstance(
                exposures,
                hazardCharacterisations,
                substances,
                new TargetUnit(ExposureUnit.ugPerKgBWPerDay),
                HealthEffectType.Risk,
                false
            );

            Assert.AreEqual(individualDays.Count, individualEffectsDictionary.First().Value.Count);
        }


        /// <summary>
        /// The dietary exposure risk action for external dose levels.
        /// Generates dietary exposures, converts them to target exposures and compares results in the risk caculator.
        /// Compares dietary risk with target exposure risks.
        /// Acute, TargetDoseLevelType = TargetDoseLevelType.External
        /// </summary>
        [TestMethod]
        public void RiskActionCalculator_TestAcuteExternal() {
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
            var relativePotencyFactors = MockRelativePotencyFactorsGenerator.MockRelativePotencyFactors(substances).ToDictionary(r => r.Compound, r => r.RPF.HasValue ? r.RPF.Value : 1.0D);
            var referenceSubstances = substances.First();
            var hazardCharacterisationsUnit = new TargetUnit(ExposureUnit.ugPerKgBWPerDay);

            //Calculate based on dietary exposures, chronic
            var dietaryIndividualDayExposures = dietaryIndividualDayIntakes
                   .AsParallel()
                   .Select(c => new DietaryIndividualDayTargetExposureWrapper(c))
                   .OrderBy(r => r.SimulatedIndividualDayId);

            var iec = new RiskCalculator<ITargetIndividualDayExposure>();
            var exposures = dietaryIndividualDayExposures.Cast<ITargetIndividualDayExposure>().ToList();
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

            var targetExposuresCalculator = new ExternalTargetExposuresCalculator();
            foreach (var item in exposures) {
                aggregateIndividualDayExposures.Single(c => c.SimulatedIndividualDayId == item.SimulatedIndividualDayId).IntraSpeciesDraw = item.IntraSpeciesDraw;
            }
            var targetIndividualDayExposures2 = targetExposuresCalculator.ComputeTargetIndividualDayExposures(
                    aggregateIndividualDayExposures.Cast<IExternalIndividualDayExposure>().ToList(),
                    substances,
                    referenceSubstances,
                    new List<ExposureRouteType> { ExposureRouteType.Dietary },
                    dietaryExposureUnit,
                    null,
                    null,
                    new ProgressState()
                ).ToDictionary(c => c.SimulatedIndividualDayId);

            aggregateIndividualDayExposures.ForEach(c => c.TargetExposuresBySubstance = targetIndividualDayExposures2[c.SimulatedIndividualDayId].TargetExposuresBySubstance);

            exposures = aggregateIndividualDayExposures.Cast<ITargetIndividualDayExposure>().ToList();
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
