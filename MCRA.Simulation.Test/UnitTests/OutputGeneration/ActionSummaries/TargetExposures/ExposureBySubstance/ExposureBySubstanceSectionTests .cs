using MCRA.General;
using MCRA.Simulation.Calculators.TargetExposuresCalculation.AggregateExposures;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.TargetExposures {

    [TestClass]
    public class ExposureBySubstanceSectionTests : SectionTestBase {

        /// <summary>
        /// Summarize (uncertainty) chronic dietary and nondietary aggregate
        /// </summary>
        [TestMethod]
        public void ExposureBySubstanceSection_TestAggregateChronic() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var individualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(100, 2, false, random);
            var substances = FakeSubstancesGenerator.Create(random.Next(1, 4));
            var rpfs = substances.ToDictionary(r => r, r => 1d);
            var memberships = substances.ToDictionary(r => r, r => 1d);
            var targetUnit = TargetUnit.FromInternalDoseUnit(DoseUnit.ugPerL, BiologicalMatrix.Liver);
            var kineticConversionFactors = FakeAbsorptionFactorsGenerator.CreateAbsorptionFactors(substances, .1);
            var externalExposuresUnit = ExposureUnitTriple.FromExternalExposureUnit(ExternalExposureUnit.ugPerKgBWPerDay);
            var aggregateIndividualExposures = FakeAggregateIndividualExposuresGenerator.Create(
                individualDays,
                substances,
                targetUnit,
                random
            );

            var section = new ExposureBySubstanceSection();
            section.Summarize(
                aggregateIndividualExposures,
                substances,
                kineticConversionFactors,
                outputStratifier: null,
                targetUnit,
                25,
                75,
                false,
                false
            );
            Assert.HasCount(substances.Count, section.Records);
        }

        /// <summary>
        /// Summarize (uncertainty) acute dietary and nondietary aggregate
        /// </summary>
        [TestMethod]
        public void ExposureBySubstanceSection_TestAggregateAcute() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var individualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(100, 2, false, random);
            var substances = FakeSubstancesGenerator.Create(random.Next(1, 4));
            var rpfs = substances.ToDictionary(r => r, r => 1d);
            var memberships = substances.ToDictionary(r => r, r => 1d);
            var kineticConversionFactors = FakeAbsorptionFactorsGenerator.CreateAbsorptionFactors(substances, .1);
            var externalExposuresUnit = ExposureUnitTriple.FromExternalExposureUnit(ExternalExposureUnit.ugPerKgBWPerDay);
            var targetUnit = TargetUnit.FromInternalDoseUnit(DoseUnit.ugPerL, BiologicalMatrix.Liver);
            var aggregates = FakeAggregateIndividualDayExposuresGenerator
                .Create(
                    individualDays,
                    substances,
                    [targetUnit],
                    random
                ).Cast<AggregateIndividualExposure>();

            var section = new ExposureBySubstanceSection();
            section.Summarize(
                [ ..aggregates],
                substances,
                kineticConversionFactors,
                outputStratifier: null,
                targetUnit,
                25,
                75,
                false,
                false
            );
        }
    }
}
