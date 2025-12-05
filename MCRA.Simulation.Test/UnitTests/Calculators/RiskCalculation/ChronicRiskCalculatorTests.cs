using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.RiskCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using MCRA.Utils.Statistics;

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
            var substances = FakeSubstancesGenerator.Create(4);
            var referenceSubstance = substances.First();
            var individuals = FakeIndividualsGenerator.Create(25, 2, random, useSamplingWeights: true);
            var individualDays = FakeIndividualDaysGenerator.Create(individuals);
            var pointsOfDeparture = FakeHazardCharacterisationModelsGenerator.Create(new Effect(), substances, seed);
            var rpfs = pointsOfDeparture.ToDictionary(r => r.Key, r => pointsOfDeparture[referenceSubstance].Value / r.Value.Value);
            var memberships = substances.ToDictionary(r => r, r => 1d);
            var targetUnit = TargetUnit.FromExternalExposureUnit(ExternalExposureUnit.ugPerKgBWPerDay);
            var exposures = FakeTargetExposuresGenerator.MockIndividualExposures(individuals, substances, random);
            var effectCalculator = new RiskCalculator<ITargetIndividualExposure>(HealthEffectType.Risk);
            exposures.ForEach(c => c.IntraSpeciesDraw = random.NextDouble());
            var individualEffects = effectCalculator.ComputeRpfWeighted(
                exposures,
                targetUnit,
                pointsOfDeparture[referenceSubstance],
                targetUnit,
                rpfs,
                memberships,
                referenceSubstance
            );

            Assert.HasCount(25, individualEffects);
        }

        /// <summary>
        /// Calculate risk based on rpfs and memberships: chronic, cumulative, Sum of ratios
        /// </summary>
        [TestMethod]
        public void ChronicRiskCalculator_TestCumulativeSumOfRatios() {
            int seed = 1;
            var random = new McraRandomGenerator(seed);
            var substances = FakeSubstancesGenerator.Create(4);
            var referenceSubstance = substances.First();
            var individuals = FakeIndividualsGenerator.Create(25, 2, random, useSamplingWeights: true);
            var individualDays = FakeIndividualDaysGenerator.Create(individuals);
            var memberships = substances.ToDictionary(r => r, r => 1d);
            var targetUnit = TargetUnit.FromExternalExposureUnit(ExternalExposureUnit.ugPerKgBWPerDay);
            var exposures = FakeTargetExposuresGenerator.MockIndividualExposures(individuals, substances, random);
            var hazardCharacterisations = FakeHazardCharacterisationModelsGenerator.Create(new Effect(), substances, seed);
            var effectCalculator = new RiskCalculator<ITargetIndividualExposure>(HealthEffectType.Risk);
            exposures.ForEach(c => c.IntraSpeciesDraw = random.NextDouble());
            var individualEffectsDictionary = effectCalculator
                .ComputeBySubstance(
                    exposures,
                    targetUnit,
                    hazardCharacterisations,
                    targetUnit,
                    substances);
            var individualEffects = effectCalculator.ComputeSumOfRatios(
                individualEffectsDictionary,
                memberships
            );

            Assert.HasCount(25, individualEffects);
        }

        /// <summary>
        /// Calculate risk based on intraspecies factor models: chronic, cumulative
        /// </summary>
        [TestMethod]
        public void ChronicRiskCalculator_TestMultiple() {
            int seed = 1;
            var random = new McraRandomGenerator(seed);
            var substances = FakeSubstancesGenerator.Create(100);
            var referenceSubstance = substances.First();
            var individuals = FakeIndividualsGenerator.Create(100, 2, random, useSamplingWeights: true);
            var hazardCharacterisations = FakeHazardCharacterisationModelsGenerator.Create(new Effect(), substances, seed);
            var exposures = FakeTargetExposuresGenerator.MockIndividualExposures(individuals, substances, random);
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
                    substances);

                foreach (var substance in substances) {
                    sum += individualEffectsDictionary[substance].Sum(c => c.HazardExposureRatio);
                }

                Assert.HasCount(individuals.Count, individualEffectsDictionary.First().Value);
            }
        }

        /// <summary>
        /// Calculate risk based on rpfs and memberships: chronic, cumulative, Sum of ratios
        /// </summary>
        [TestMethod]
        public void ChronicRiskCalculator_TestCumulativeSumOfRatios1() {
            int seed = 1;
            var random = new McraRandomGenerator(seed);
            var substances = FakeSubstancesGenerator.Create(3);
            var referenceSubstance = substances.First();
            var individuals = FakeIndividualsGenerator.Create(3, 2, random, useSamplingWeights: true);
            var individualDays = FakeIndividualDaysGenerator.Create(individuals);
            var memberships = substances.ToDictionary(r => r, r => 1d);
            var target = ExposureTarget.DietaryExposureTarget;
            var hazardCharacterisations = FakeHazardCharacterisationModelsGenerator.Create(new Effect(), substances, seed);

            var targetIndividualExposures = new List<ITargetIndividualExposure>();
            var exposuresMultiplier = new List<double>() { 0, 1, 2 };
            var counter = 0;
            foreach (var individual in individuals) {
                individual.BodyWeight = 1;
                var targetIndividualExposure = new TargetIndividualExposure() {
                    SimulatedIndividual = new(individual, counter),
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
                    substances);
            var individualEffects = effectCalculator.ComputeSumOfRatios(
                individualEffectsDictionary,
                memberships
            );

            Assert.HasCount(3, individualEffects);
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
            var substances = FakeSubstancesGenerator.Create(3);
            var referenceSubstance = substances.First();
            var individuals = FakeIndividualsGenerator.Create(1, 2, random, useSamplingWeights: true);
            var individualDays = FakeIndividualDaysGenerator.Create(individuals);
            var memberships = substances.ToDictionary(r => r, r => 1d);
            var target = ExposureTarget.DietaryExposureTarget;
            var hazardCharacterisations = FakeHazardCharacterisationModelsGenerator.Create(new Effect(), substances, seed);
            var targetIndividualExposures = new List<ITargetIndividualExposure>();
            var counter = 0;
            foreach (var individual in individuals) {
                individual.BodyWeight = 1;
                var targetIndividualExposure = new TargetIndividualExposure() {
                    SimulatedIndividual = new(individual, counter),
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
                    substances);
            var individualEffects = effectCalculator.ComputeSumOfRatios(
                individualEffectsDictionary,
                memberships
            );

            Assert.HasCount(1, individualEffects);
            // Exposures for an individual are 0, 1 or 3 times HC
            Assert.AreEqual(333.3333, individualEffects[0].HazardExposureRatio, 0.01);
            Assert.AreEqual(0.003, individualEffects[0].ExposureHazardRatio, 0.0001);
        }
    }
}
