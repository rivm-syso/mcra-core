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
            var effectCalculator = new RiskCalculator<ITargetIndividualDayExposure>(HealthEffectType.Risk);
            var individualEffects = effectCalculator
                .ComputeRpfWeighted(
                    exposures,
                    TargetUnit.FromExternalExposureUnit(ExternalExposureUnit.ugPerKgBWPerDay),
                    pointsOfDeparture[referenceSubstance],
                    TargetUnit.FromInternalDoseUnit(DoseUnit.ugPerKg),
                    rpfs,
                    memberships,
                    referenceSubstance
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
            var effectCalculator = new RiskCalculator<ITargetIndividualDayExposure>(HealthEffectType.Risk);
            var individualEffectsDictionary = effectCalculator.ComputeBySubstance(
                exposures,
                TargetUnit.FromExternalExposureUnit(ExternalExposureUnit.ugPerKgBWPerDay),
                hazardCharacterisations,
                TargetUnit.FromInternalDoseUnit(DoseUnit.ugPerKg),
                substances);
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
            var effect = MockEffectsGenerator.Create();
            var substances = MockSubstancesGenerator.Create(5);
            var correctedRelativePotencyFactors = substances.ToDictionary(c => c, c => 1d);
            var membershipProbabilities = substances.ToDictionary(c => c, c => 1d);
            var individuals = MockIndividualsGenerator.Create(50, 2, random, useSamplingWeights: true);
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var foodsAsMeasured = MockFoodsGenerator.Create(3);
            var dietaryIndividualDayIntakes = MockDietaryIndividualDayIntakeGenerator.Create(individualDays, foodsAsMeasured, substances, 0, true, random);
            var dietaryExposureUnit = ExposureUnitTriple.FromExposureUnit(ExternalExposureUnit.ugPerKgBWPerDay);
            var hazardCharacterisations = MockHazardCharacterisationModelsGenerator.Create(effect, substances.ToList(), seed);
            var referenceSubstances = substances.First();
            var relativePotencyFactors = MockRelativePotencyFactorsGenerator.Create(substances, referenceSubstances)
                .ToDictionary(r => r.Compound, r => r.RPF);
            var hazardCharacterisationsUnit = TargetUnit.FromInternalDoseUnit(DoseUnit.ugPerKg);
            var exposureUnit = TargetUnit.FromExternalExposureUnit(ExternalExposureUnit.ugPerKgBWPerDay);
            // Calculate based on dietary exposures, chronic
            var dietaryIndividualDayExposures = dietaryIndividualDayIntakes
                   .AsParallel()
                   .Select(c => new DietaryIndividualDayTargetExposureWrapper(c))
                   .OrderBy(r => r.SimulatedIndividualDayId);

            var iec = new RiskCalculator<ITargetIndividualDayExposure>(HealthEffectType.Risk);
            var exposures = dietaryIndividualDayExposures.Cast<ITargetIndividualDayExposure>().ToList();
            exposures.ForEach(c => c.IntraSpeciesDraw = random.NextDouble());
            var cumulativeIndividualEffects1 = iec.ComputeRpfWeighted(
                exposures,
                exposureUnit,
                hazardCharacterisations[referenceSubstances],
                hazardCharacterisationsUnit,
                correctedRelativePotencyFactors,
                membershipProbabilities,
                referenceSubstances
            );
            var dietaryExposureSum = cumulativeIndividualEffects1.Sum(c => c.Exposure);

            var individualEffectsBySubstance1 = iec.ComputeBySubstance(
                exposures,
                exposureUnit,
                hazardCharacterisations,
                hazardCharacterisationsUnit,
                substances);

            // Create target exposure individual day exposures based on dietary exposures
            var aggregateIndividualDayExposures = AggregateIntakeCalculator.CreateAggregateIndividualDayExposures(
                dietaryIndividualDayIntakes,
                null,
                new List<ExposureRouteType>() { ExposureRouteType.Dietary }
            );

            var targetExposuresCalculator = new ExternalTargetExposuresCalculator();
            foreach (var item in exposures) {
                aggregateIndividualDayExposures
                    .Single(c => c.SimulatedIndividualDayId == item.SimulatedIndividualDayId).IntraSpeciesDraw = item.IntraSpeciesDraw;
            }
            var targetIndividualDayExposures2 = targetExposuresCalculator
                .ComputeTargetIndividualDayExposures(
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
            var cumulativeIndividualEffects2 = iec.ComputeRpfWeighted(
                exposures,
                exposureUnit,
                hazardCharacterisations[referenceSubstances],
                hazardCharacterisationsUnit,
                correctedRelativePotencyFactors,
                membershipProbabilities,
                referenceSubstances
            );
            var targetExposureSum = cumulativeIndividualEffects2.Sum(c => c.Exposure);
            var individualEffectsBySubstance2 = iec.ComputeBySubstance(
                exposures,
                exposureUnit,
                hazardCharacterisations,
                hazardCharacterisationsUnit,
                substances);

            Assert.AreEqual(dietaryExposureSum, targetExposureSum, 1e-4);
            foreach (var substance in substances) {
                var dietaryExposureBySubstanceSum = individualEffectsBySubstance1[substance].Sum(c => c.Exposure);
                var targetExposureBySubstanceSum = individualEffectsBySubstance2[substance].Sum(c => c.Exposure);
                Assert.AreEqual(dietaryExposureBySubstanceSum, targetExposureBySubstanceSum, 1e-4);
            }
        }
    }
}
