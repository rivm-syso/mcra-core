using MCRA.General;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using MCRA.Utils.Statistics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.TargetExposures {
    /// <summary>
    /// OutputGeneration, ActionSummaries, TargetExposures, ExposureByCompound, ByCompound
    /// </summary>
    [TestClass]
    public class ExposureBySubstanceSectionTests : SectionTestBase {

        /// <summary>
        /// Summarize (uncertainty) chronic dietary and nondietary aggregate
        /// </summary>
        [TestMethod]
        public void TotalDistributionCompoundSection_TestAggregateChronic() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var individualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(100, 2, false, random);
            var substances = FakeSubstancesGenerator.Create(random.Next(1, 4));
            var routes = new[] { ExposureRoute.Dermal, ExposureRoute.Oral };
            var paths = FakeExposurePathGenerator.Create(routes);
            var rpfs = substances.ToDictionary(r => r, r => 1d);
            var memberships = substances.ToDictionary(r => r, r => 1d);
            var targetUnit = TargetUnit.FromInternalDoseUnit(DoseUnit.ugPerL, BiologicalMatrix.Liver);
            var kineticConversionFactors = FakeKineticModelsGenerator.CreateAbsorptionFactors(substances, .1);
            var externalExposuresUnit = ExposureUnitTriple.FromExposureUnit(ExternalExposureUnit.ugPerKgBWPerDay);
            var individualExposures = FakeExternalExposureGenerator.CreateExternalIndividualExposures(individualDays, substances, paths, seed);

            var section = new ExposureBySubstanceSection();
            section.Summarize(
                individualExposures,
                rpfs,
                memberships,
                kineticConversionFactors,
                substances,
                25,
                75,
                targetUnit,
                false,
                false
            );
            Assert.HasCount(substances.Count, section.Records);
        }

        /// <summary>
        /// Summarize (uncertainty) acute dietary and nondietary aggregate
        /// </summary>
        [TestMethod]
        public void TotalDistributionCompoundSection_TestAggregateAcute() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var individualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(100, 2, false, random);
            var substances = FakeSubstancesGenerator.Create(random.Next(1, 4));
            var rpfs = substances.ToDictionary(r => r, r => 1d);
            var routes = new[] { ExposureRoute.Dermal, ExposureRoute.Oral };
            var paths = FakeExposurePathGenerator.Create(routes);
            var memberships = substances.ToDictionary(r => r, r => 1d);
            var kineticConversionFactors = FakeKineticModelsGenerator.CreateAbsorptionFactors(substances, .1);
            var externalExposuresUnit = ExposureUnitTriple.FromExposureUnit(ExternalExposureUnit.ugPerKgBWPerDay);
            var targetUnit = TargetUnit.FromInternalDoseUnit(DoseUnit.ugPerL, BiologicalMatrix.Liver);
            var individualExposures = FakeExternalExposureGenerator.CreateExternalIndividualExposures(individualDays, substances, paths, seed);

            var section = new ExposureBySubstanceSection();
            section.Summarize(
                individualExposures,
                rpfs,
                memberships,
                kineticConversionFactors,
                substances,
                25,
                75,
                targetUnit,
                false,
                false
            );
            Assert.HasCount(substances.Count, section.Records);
        }
    }
}
