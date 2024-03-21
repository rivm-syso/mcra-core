using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.RiskCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using MCRA.Utils.Statistics;
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
    }
}
