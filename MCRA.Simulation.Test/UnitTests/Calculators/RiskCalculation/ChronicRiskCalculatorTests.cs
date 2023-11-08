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
            var effectCalculator = new RiskCalculator<ITargetIndividualExposure>(HealthEffectType.Risk);
            exposures.ForEach(c => c.IntraSpeciesDraw = random.NextDouble());
            var individualEffects = effectCalculator.ComputeRpfWeighted(
                exposures,
                TargetUnit.FromExternalExposureUnit(ExternalExposureUnit.ugPerKgBWPerDay),
                pointsOfDeparture[referenceSubstance],
                TargetUnit.FromInternalDoseUnit(DoseUnit.ugPerKg),
                rpfs,
                memberships,
                referenceSubstance
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
            var effectCalculator = new RiskCalculator<ITargetIndividualExposure>(HealthEffectType.Risk);
            exposures.ForEach(c => c.IntraSpeciesDraw = random.NextDouble());
            var individualEffectsDictionary = effectCalculator
                .ComputeBySubstance(
                    exposures,
                    TargetUnit.FromExternalExposureUnit(ExternalExposureUnit.ugPerKgBWPerDay),
                    hazardCharacterisations,
                    TargetUnit.FromInternalDoseUnit(DoseUnit.ugPerKg),
                    substances
                );
            var individualEffects = effectCalculator.ComputeSumOfRatios(
                individualEffectsDictionary,
                memberships
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
                var effectCalculator = new RiskCalculator<ITargetIndividualExposure>(HealthEffectType.Risk);
                var individualEffectsDictionary = effectCalculator.ComputeBySubstance(
                    exposures,
                    TargetUnit.FromExternalExposureUnit(ExternalExposureUnit.ugPerKgBWPerDay),
                    hazardCharacterisations,
                    TargetUnit.FromExternalExposureUnit(ExternalExposureUnit.mgPerKgBWPerDay),
                    substances
                );

                foreach (var substance in substances) {
                    sum += individualEffectsDictionary[substance].Sum(c => c.HazardExposureRatio);
                }

                Assert.AreEqual(individuals.Count, individualEffectsDictionary.First().Value.Count);
            }
            var test = sum / ncounter;
            Assert.AreEqual(13668877698.889038, test, 0.001);
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
            var effect = MockEffectsGenerator.Create();
            var correctedRelativePotencyFactors = substances.ToDictionary(c => c, c => 1d);
            var membershipProbabilities = substances.ToDictionary(c => c, c => 1d);
            var individuals = MockIndividualsGenerator.Create(50, 2, random, useSamplingWeights: true);
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var foodsAsMeasured = MockFoodsGenerator.Create(3);
            var dietaryIndividualDayIntakes = MockDietaryIndividualDayIntakeGenerator
                .Create(individualDays, foodsAsMeasured, substances, 0, true, random);
            var dietaryExposureUnit = ExposureUnitTriple.FromExposureUnit(ExternalExposureUnit.ugPerKgBWPerDay);
            var hazardCharacterisations = MockHazardCharacterisationModelsGenerator
                .Create(effect, substances.ToList(), seed);
            var referenceSubstances = substances.First();
            var hazardCharacterisationsUnit = TargetUnit.FromExternalExposureUnit(ExternalExposureUnit.ugPerKg);
            var exposureUnit = TargetUnit.FromExternalExposureUnit(ExternalExposureUnit.ugPerKgBWPerDay);
            // Calculate based on dietary exposures, chronic
            var dietaryIndividualExposures = dietaryIndividualDayIntakes
               .AsParallel()
               .GroupBy(c => c.SimulatedIndividualId)
               .Select(c => new DietaryIndividualTargetExposureWrapper(c.ToList()))
               .OrderBy(r => r.SimulatedIndividualId)
               .ToList();

            var iec = new RiskCalculator<ITargetIndividualExposure>(HealthEffectType.Risk);
            var exposures = dietaryIndividualExposures.Cast<ITargetIndividualExposure>().ToList();
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
                substances
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
            ).ToDictionary(c => c.SimulatedIndividualId);

            aggregateIndividualExposures.ForEach(c => c.TargetExposuresBySubstance = targetIndividualExposures2[c.SimulatedIndividualId].TargetExposuresBySubstance);

            exposures = aggregateIndividualExposures.Cast<ITargetIndividualExposure>().ToList();
            exposures.ForEach(c => c.IntraSpeciesDraw = random.NextDouble());
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
                substances
            );

            Assert.AreEqual(dietaryExposureSum, targetExposureSum, 1e-4);
            foreach (var substance in substances) {
                var dietaryExposureBySubstanceSum = individualEffectsBySubstance1[substance].Sum(c => c.Exposure);
                var targetExposureBySubstanceSum = individualEffectsBySubstance2[substance].Sum(c => c.Exposure);
                Assert.AreEqual(dietaryExposureBySubstanceSum, targetExposureBySubstanceSum, 1e-4);
            }
        }

        /// <summary>
        /// Calculate risk based on rpfs and memberships: chronic, cumulative, Sum of ratios
        /// </summary>
        [TestMethod]
        public void ChronicRiskCalculator_TestCumulativeSumOfRatios1() {
            int seed = 1;
            var random = new McraRandomGenerator(seed);
            var substances = MockSubstancesGenerator.Create(3);
            var referenceSubstance = substances.First();
            var individuals = MockIndividualsGenerator.Create(3, 2, random, useSamplingWeights: true);
            var individualDays = MockIndividualDaysGenerator.Create(individuals);
            var memberships = substances.ToDictionary(r => r, r => 1d);
            var target = ExposureTarget.DietaryExposureTarget;
            var hazardCharacterisations = MockHazardCharacterisationModelsGenerator.Create(new Effect(), substances, seed);

            var targetIndividualExposures = new List<ITargetIndividualExposure>();
            var exposuresMultiplier = new List<double>() { 0, 1, 2 };
            var counter = 0;
            foreach (var individual in individuals) {
                individual.BodyWeight = 1;
                var targetIndividualExposure = new TargetIndividualExposure() {
                    SimulatedIndividualId = individual.Id,
                    IndividualSamplingWeight = individual.SamplingWeight,
                    Individual = individual,
                    RelativeCompartmentWeight = 1D,
                    TargetExposuresBySubstance = substances
                            .Select((c, ixs) => new SubstanceTargetExposure(c, hazardCharacterisations[c].Value * exposuresMultiplier[counter]))
                            .ToDictionary(c => c.Substance, c => c as ISubstanceTargetExposure)
                };
                targetIndividualExposures.Add(targetIndividualExposure);
                counter++;
            }

            var effectCalculator = new RiskCalculator<ITargetIndividualExposure>(HealthEffectType.Risk);
            targetIndividualExposures.ForEach(c => c.IntraSpeciesDraw = random.NextDouble());
            var individualEffectsDictionary = effectCalculator
                .ComputeBySubstance(
                    targetIndividualExposures,
                    new TargetUnit(target, ExposureUnitTriple.FromExposureUnit(ExternalExposureUnit.ugPerKgBWPerDay)),
                    hazardCharacterisations,
                    new TargetUnit(target, ExposureUnitTriple.FromExposureUnit(ExternalExposureUnit.ugPerKgBWPerDay)),
                    substances
                 );
            var individualEffects = effectCalculator.ComputeSumOfRatios(
                individualEffectsDictionary,
                memberships
            );

            Assert.AreEqual(3, individualEffects.Count);
            // All exposures per substance for individual 1 are zero
            Assert.AreEqual(33333333, individualEffects[0].HazardExposureRatio, 1);
            Assert.AreEqual(0, individualEffects[0].ExposureHazardRatio);

            // All exposures per substance for individual 2 are 1 x HC
            Assert.AreEqual(0.3333333, individualEffects[1].HazardExposureRatio, 0.001);
            Assert.AreEqual(3, individualEffects[1].ExposureHazardRatio, 0.001);

            // All exposures per substance for individual 3 are 2 x HC
            Assert.AreEqual(0.1666667, individualEffects[2].HazardExposureRatio, 0.001);
            Assert.AreEqual(6, individualEffects[2].ExposureHazardRatio);
        }

        /// <summary>
        /// // Exposures for an individual are 0, 1 or 2 times HC
        /// </summary>
        [TestMethod]
        public void ChronicRiskCalculator_TestCumulativeSumOfRatios2() {
            int seed = 1;
            var random = new McraRandomGenerator(seed);
            var substances = MockSubstancesGenerator.Create(3);
            var referenceSubstance = substances.First();
            var individuals = MockIndividualsGenerator.Create(1, 2, random, useSamplingWeights: true);
            var individualDays = MockIndividualDaysGenerator.Create(individuals);
            var memberships = substances.ToDictionary(r => r, r => 1d);
            var target = ExposureTarget.DietaryExposureTarget;
            var hazardCharacterisations = MockHazardCharacterisationModelsGenerator.Create(new Effect(), substances, seed);
            var targetIndividualExposures = new List<ITargetIndividualExposure>();
            var counter = 0;
            foreach (var individual in individuals) {
                individual.BodyWeight = 1;
                var targetIndividualExposure = new TargetIndividualExposure() {
                    SimulatedIndividualId = individual.Id,
                    IndividualSamplingWeight = individual.SamplingWeight,
                    Individual = individual,
                    RelativeCompartmentWeight = 1D,
                    TargetExposuresBySubstance = substances
                        .Select((c, ixs) => new SubstanceTargetExposure(c, hazardCharacterisations[c].Value * ixs))
                        .ToDictionary(c => c.Substance, c => c as ISubstanceTargetExposure)
                };
                targetIndividualExposures.Add(targetIndividualExposure);
                counter++;
            }
            var effectCalculator = new RiskCalculator<ITargetIndividualExposure>(HealthEffectType.Risk);
            targetIndividualExposures.ForEach(c => c.IntraSpeciesDraw = random.NextDouble());
            var individualEffectsDictionary = effectCalculator
                .ComputeBySubstance(
                    targetIndividualExposures,
                    new TargetUnit(target, ExposureUnitTriple.FromExposureUnit(ExternalExposureUnit.ugPerKgBWPerDay)),
                    hazardCharacterisations,
                    new TargetUnit(target, ExposureUnitTriple.FromExposureUnit(ExternalExposureUnit.mgPerKgBWPerDay)),
                    substances
                 );
            var individualEffects = effectCalculator.ComputeSumOfRatios(
                individualEffectsDictionary,
                memberships
            );

            Assert.AreEqual(1, individualEffects.Count);
            // Exposures for an individual are 0, 1 or 3 times HC
            Assert.AreEqual(333.3333, individualEffects[0].HazardExposureRatio, 0.01);
            Assert.AreEqual(0.003, individualEffects[0].ExposureHazardRatio, 0.0001);
        }
    }
}
