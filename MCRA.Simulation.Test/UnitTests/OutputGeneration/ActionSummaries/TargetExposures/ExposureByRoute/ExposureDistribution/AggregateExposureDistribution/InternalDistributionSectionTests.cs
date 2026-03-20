using MCRA.General;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.TargetExposures {
    /// <summary>
    /// OutputGeneration, ActionSummaries, TargetExposures, ExposureByRoute, ExposureDistribution, AggregateExposureDistribution
    /// </summary>
    [TestClass]
    public class InternalDistributionSectionTests : SectionTestBase {

        /// <summary>
        /// Summarize aggregate exposure (uncertainty) nand test view
        /// </summary>
        [TestMethod]
        public void InternalDistributionTotalSection_Test1() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var individuals = FakeIndividualsGenerator.Create(25, 2, random);
            var individualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var routes = new[] { ExposureRoute.Dermal, ExposureRoute.Oral, ExposureRoute.Inhalation };
            var substances = FakeSubstancesGenerator.Create(3);
            var kineticConversionFactors = FakeAbsorptionFactorsGenerator.CreateAbsorptionFactors(substances, routes, 1D);
            var rpfs = substances.ToDictionary(r => r, r => 1d);
            var memberships = substances.ToDictionary(r => r, r => 1d);
            var targetUnit = TargetUnit.FromInternalDoseUnit(DoseUnit.ugPerL, BiologicalMatrix.Liver);
            var externalExposuresUnit = ExposureUnitTriple.FromExposureUnit(ExternalExposureUnit.ugPerKgBWPerDay);
            var aggregateIndividualExposures = FakeAggregateIndividualExposuresGenerator.Create(
                individualDays,
                substances,
                targetUnit,
                random
            );
            var header = new SectionHeader();
            var section = new InternalDistributionTotalSection();
            section.Summarize(
                aggregateIndividualExposures,
                substances,
                rpfs,
                memberships,
                targetUnit,
                null,
                2.5,
                97.5,
                false
            );
            var subHeader = header.AddSubSectionHeaderFor(section, "Summary", 3);
            var section1 = subHeader.GetSummarySection() as InternalDistributionTotalSection;
            AssertIsValidView(section1);
        }
    }
}