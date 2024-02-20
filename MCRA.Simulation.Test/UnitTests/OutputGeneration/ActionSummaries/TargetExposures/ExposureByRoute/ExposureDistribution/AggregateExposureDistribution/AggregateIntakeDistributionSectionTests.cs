using MCRA.Utils.Statistics;
using MCRA.General;
using MCRA.Simulation.Calculators.TargetExposuresCalculation.TargetExposuresCalculators;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.TargetExposures {
    /// <summary>
    /// OutputGeneration, ActionSummaries, TargetExposures, ExposureByRoute, ExposureDistribution, AggregateExposureDistribution
    /// </summary>
    [TestClass]
    public class AggregateIntakeDistributionSectionTests : SectionTestBase {

        /// <summary>
        /// Summarize aggregate exposure (uncertainty) nand test view
        /// </summary>
        [TestMethod]
        public void AggregateIntakeDistributionSection_Test1() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var individuals = MockIndividualsGenerator.Create(25, 2, random);
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var exposureRoutes = new List<ExposurePathType>() { ExposurePathType.Dietary, ExposurePathType.Dermal, ExposurePathType.Oral, ExposurePathType.Inhalation };
            var substances = MockSubstancesGenerator.Create(3);
            var absorptionFactors = MockKineticModelsGenerator.CreateAbsorptionFactors(substances, exposureRoutes, 1D);
            var rpfs = substances.ToDictionary(r => r, r => 1d);
            var memberships = substances.ToDictionary(r => r, r => 1d);

            var kineticModelCalculators = MockKineticModelsGenerator.CreateAbsorptionFactorKineticModelCalculators(substances, absorptionFactors);
            var targetExposuresCalculator = new InternalTargetExposuresCalculator(kineticModelCalculators);
            var externalExposuresUnit = ExposureUnitTriple.FromExposureUnit(ExternalExposureUnit.ugPerKgBWPerDay);
            var aggregateIndividualDayExposures = MockAggregateIndividualDayIntakeGenerator.Create(
                individualDays,
                substances,
                exposureRoutes,
                targetExposuresCalculator,
                externalExposuresUnit,
                random
            );
            var header = new SectionHeader();
            var section = new AggregateIntakeDistributionSection();
            section.Summarize(
                header,
                aggregateIndividualDayExposures,
                substances,
                rpfs,
                memberships,
                absorptionFactors,
                exposureRoutes,
                substances.First(),
                ExposureMethod.Automatic,
                new double[] { 005, .1 },
                new double[] { 50, 90, 95 },
                34,
                false,
                2.5,
                97.5
            );
            var subHeader = header.GetSubSectionHeader<IntakePercentileSection>();
            var section1 = subHeader.GetSummarySection() as IntakePercentileSection;
            subHeader = header.GetSubSectionHeader<IntakePercentageSection>();
            var section2 = subHeader.GetSummarySection() as IntakePercentageSection;
            subHeader = header.GetSubSectionHeader<AggregateTotalIntakeDistributionSection>();
            var section3 = subHeader.GetSummarySection() as AggregateTotalIntakeDistributionSection;
            subHeader = header.GetSubSectionHeader<UntransformedTotalIntakeDistributionSection>();
            var section4 = subHeader.GetSummarySection() as UntransformedTotalIntakeDistributionSection;

            Assert.IsTrue(section1.IntakePercentileRecords[0].ReferenceValue > 0);
            section.SummarizeUncertainty(
                 header,
                aggregateIndividualDayExposures,
                substances,
                rpfs,
                memberships,
                5,
                95,
                false
            );
            AssertIsValidView(section1);
            AssertIsValidView(section2);
            AssertIsValidView(section3);
            AssertIsValidView(section4);
        }
    }
}