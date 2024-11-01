using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.TargetExposures {

    [TestClass]
    public class CoExposureUpperDistributionSectionTests : SectionTestBase {

        /// <summary>
        /// Summarize co-exposure target exposures chronic, test CoExposureUpperDistributionSection view
        /// </summary>
        [TestMethod]
        public void CoExposureUpperDistributionSection_TestSummarizeTargetExposuresChronic() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var substances = MockSubstancesGenerator.Create(4);
            var referenceSubstance = substances.First();
            var individuals = FakeIndividualsGenerator.Create(25, 2, random, useSamplingWeights: true);
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var pointsOfDeparture = MockHazardCharacterisationModelsGenerator.Create(new Effect(), substances, seed);
            var rpfs = pointsOfDeparture.ToDictionary(r => r.Key, r => pointsOfDeparture[referenceSubstance].Value / r.Value.Value);
            var memberships = substances.ToDictionary(r => r, r => 1d);
            var intraSpeciesFactorModels = MockIntraSpeciesFactorModelsGenerator.Create(substances);
            var targetUnit = TargetUnit.FromInternalDoseUnit(DoseUnit.ugPerL, BiologicalMatrix.Blood);
            var kineticConversionFactors = MockKineticModelsGenerator.CreateAbsorptionFactors(substances, .1);
            var externalExposuresUnit = ExposureUnitTriple.FromExposureUnit(ExternalExposureUnit.ugPerKgBWPerDay);
            var exposures = FakeAggregateIndividualExposuresGenerator
                .Create(
                    individualDays,
                    substances,
                    new List<TargetUnit>() { targetUnit },
                    random
                );

            var section = new CoExposureUpperDistributionSection();
            section.Summarize(exposures, null, substances, rpfs, memberships, kineticConversionFactors, 97.5, externalExposuresUnit,targetUnit);
            Assert.IsNotNull(section.AggregatedExposureRecords);
            AssertIsValidView(section);
        }

        /// <summary>
        /// Summarize co-exposure target exposures acute, test CoExposureUpperDistributionSection view
        /// </summary>
        [TestMethod]
        public void CoExposureUpperDistributionSection_TestSummarizeTargetExposuresAcute() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var substances = MockSubstancesGenerator.Create(4);
            var referenceSubstance = substances.First();
            var individuals = FakeIndividualsGenerator.Create(25, 2, random, useSamplingWeights: true);
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var pointsOfDeparture = MockHazardCharacterisationModelsGenerator.Create(new Effect(), substances, seed);
            var rpfs = pointsOfDeparture.ToDictionary(r => r.Key, r => pointsOfDeparture[referenceSubstance].Value / r.Value.Value);
            var memberships = substances.ToDictionary(r => r, r => 1d);
            var intraSpeciesFactorModels = MockIntraSpeciesFactorModelsGenerator.Create(substances);
            var targetUnit = TargetUnit.FromInternalDoseUnit(DoseUnit.ugPerL, BiologicalMatrix.Blood);
            var kineticConversionFactors = MockKineticModelsGenerator.CreateAbsorptionFactors(substances, .1);
            var externalExposuresUnit = ExposureUnitTriple.FromExposureUnit(ExternalExposureUnit.ugPerKgBWPerDay);
            var exposures = FakeAggregateIndividualDayExposuresGenerator
                .Create(
                    individualDays,
                    substances,
                    [targetUnit],
                    random
                );

            var section = new CoExposureUpperDistributionSection();
            section.Summarize(null, exposures, substances, rpfs, memberships, kineticConversionFactors, 97.5, externalExposuresUnit, targetUnit);
            Assert.IsNotNull(section.AggregatedExposureRecords);
            AssertIsValidView(section);
        }
        /// <summary>
        /// Summarize co-exposure dietary exposures acute, test CoExposureUpperDistributionSection view
        /// </summary>
        [TestMethod]
        public void CoExposureUpperDistributionSection_TestSummarizeDietaryExposuresAcute() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var foods = MockFoodsGenerator.MockFoods("Apple", "Pear", "Bananas");
            var substances = MockSubstancesGenerator.Create(3);
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(20, 2, true, random);
            var rpfs = substances.ToDictionary(r => r, r => 1d);
            var memberships = substances.ToDictionary(r => r, r => 1d);
            var exposures = MockDietaryIndividualDayIntakeGenerator.Create(individualDays, foods, substances, 0.5, true, random);

            var section = new CoExposureUpperDistributionSection();
            section.Summarize(exposures, substances, rpfs, memberships, ExposureType.Acute, 97.5, false);
            Assert.IsNotNull(section.AggregatedExposureRecords);
            AssertIsValidView(section);
        }
        /// <summary>
        /// Summarize co-exposure dietary exposures chronic, test CoExposureUpperDistributionSection view
        /// </summary>
        [TestMethod]
        public void CoExposureUpperDistributionSection_TestSummarizeDietaryExposuresChronic() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var foods = MockFoodsGenerator.MockFoods("Apple", "Pear", "Bananas");
            var substances = MockSubstancesGenerator.Create(3);
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(20, 2, true, random);
            var rpfs = substances.ToDictionary(r => r, r => 1d);
            var memberships = substances.ToDictionary(r => r, r => 1d);
            var exposures = MockDietaryIndividualDayIntakeGenerator.Create(individualDays, foods, substances, 0.5, true, random);

            var section = new CoExposureUpperDistributionSection();
            section.Summarize(exposures, substances, rpfs, memberships, ExposureType.Chronic, 97.5, false);
            Assert.IsNotNull(section.AggregatedExposureRecords);
            AssertIsValidView(section);
        }
    }
}
