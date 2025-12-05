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
    public class AcuteRiskCalculatorTests {

        /// <summary>
        /// Calculate risk based on rpfs and memberships: acute, cumulative
        /// </summary>
        [TestMethod]
        public void AcuteRiskCalculator_TestCumulative() {
            int seed = 1;
            var random = new McraRandomGenerator(seed);
            var substances = FakeSubstancesGenerator.Create(4);
            var targetUnit = TargetUnit.FromExternalExposureUnit(ExternalExposureUnit.ugPerKgBWPerDay);
            var referenceSubstance = substances.First();
            var individuals = FakeIndividualsGenerator.Create(25, 2, random, useSamplingWeights: true);
            var individualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var exposures = FakeTargetExposuresGenerator.MockIndividualDayExposures(individualDays, substances, random);
            var pointsOfDeparture = FakeHazardCharacterisationModelsGenerator.Create(new Effect(), substances, seed);
            var rpfs = pointsOfDeparture.ToDictionary(r => r.Key, r => pointsOfDeparture[referenceSubstance].Value / r.Value.Value);
            var memberships = substances.ToDictionary(r => r, r => 1d);
            var intraSpeciesFactorModels = FakeIntraSpeciesFactorModelsGenerator.Create(substances);
            exposures.ForEach(c => c.IntraSpeciesDraw = random.NextDouble());
            var effectCalculator = new RiskCalculator<ITargetIndividualDayExposure>(HealthEffectType.Risk);
            var individualEffects = effectCalculator
                .ComputeRpfWeighted(
                    exposures,
                    targetUnit,
                    pointsOfDeparture[referenceSubstance],
                    targetUnit,
                    rpfs,
                    memberships,
                    referenceSubstance
                 );
            Assert.HasCount(50, individualEffects);
        }

        /// <summary>
        /// Calculate risk based on intraspecies factor models: acute, cumulative
        /// </summary>
        [TestMethod]
        public void AcuteRiskCalculator_TestMultiple() {
            int seed = 1;
            var random = new McraRandomGenerator(seed);
            var substances = FakeSubstancesGenerator.Create(4);
            var individuals = FakeIndividualsGenerator.Create(25, 2, random, useSamplingWeights: true);
            var individualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var targetUnit = TargetUnit.FromExternalExposureUnit(ExternalExposureUnit.ugPerKgBWPerDay);
            var exposures = FakeTargetExposuresGenerator.MockIndividualDayExposures(individualDays, substances, random);
            var hazardCharacterisations = FakeHazardCharacterisationModelsGenerator.Create(new Effect(), substances, seed);
            exposures.ForEach(c => c.IntraSpeciesDraw = random.NextDouble());
            var effectCalculator = new RiskCalculator<ITargetIndividualDayExposure>(HealthEffectType.Risk);
            var individualEffectsDictionary = effectCalculator.ComputeBySubstance(
                exposures,
                targetUnit,
                hazardCharacterisations,
                targetUnit,
                substances);
            Assert.HasCount(individualDays.Count, individualEffectsDictionary.First().Value);
        }
    }
}
