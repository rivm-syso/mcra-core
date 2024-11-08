using MCRA.Utils.Statistics;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.TargetExposures {
    /// <summary>
    /// OutputGeneration, ActionSummaries, TargetExposures, ExposureByRoute, ExposureDistribution, DietaryExposureDistribution
    /// </summary>
    [TestClass]
    public class DietaryIntakeDistributionSectionTests : SectionTestBase {

        /// <summary>
        /// Summarize dietary exposure (uncertainty) and test DietaryTotalIntakeDistributionSection,
        /// DietaryUpperIntakeDistributionSection, DietaryIntakePercentageSection, DietaryIntakePercentileSection
        /// UntransformedTotalIntakeDistributionSection view
        /// </summary>
        [TestMethod]
        public void DietaryIntakeDistributionSection_TestMultiple() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var individuals = FakeIndividualsGenerator.Create(25, 2, random);
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var foods = MockFoodsGenerator.Create(3);
            var substances = MockSubstancesGenerator.Create(3);
            var rpfs = substances.ToDictionary(r => r, r => 1d);
            var memberships = substances.ToDictionary(r => r, r => 1d);
            var kineticConversionFactors = MockKineticModelsGenerator.CreateAbsorptionFactors(substances, 1D);
            var dietaryIndividualDayIntakes = MockDietaryIndividualDayIntakeGenerator.Create(individualDays, foods, substances, 0, true, random);
            var header = new SectionHeader();
            var section = new DietaryIntakeDistributionSection();
            section.Summarize(
                header,
                dietaryIndividualDayIntakes,
                rpfs,
                memberships,
                substances.First(),
                new double[] { 50, 95 },
                new double[] { 005, .1, 1, 2, 3, 95 },
                34,
                false,
                2.5,
                97.5
            );
            var subHeader = header.GetSubSectionHeader<IntakePercentileSection>();
            var section1 = subHeader.GetSummarySection() as IntakePercentileSection;
            subHeader = header.GetSubSectionHeader<IntakePercentageSection>();
            var section2 = subHeader.GetSummarySection() as IntakePercentageSection;
            subHeader = header.GetSubSectionHeader<DietaryTotalIntakeDistributionSection>();
            var section3 = subHeader.GetSummarySection() as DietaryTotalIntakeDistributionSection;
            subHeader = header.GetSubSectionHeader<UntransformedTotalIntakeDistributionSection>();
            var section4 = subHeader.GetSummarySection() as UntransformedTotalIntakeDistributionSection;
            section.SummarizeUncertainty(
                header,
                dietaryIndividualDayIntakes,
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

        /// <summary>
        /// Summarize dietary exposure, single substance.
        /// </summary>
        [TestMethod]
        public void DietaryIntakeDistributionSection_TestSingle() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var individuals = FakeIndividualsGenerator.Create(25, 2, random);
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var foods = MockFoodsGenerator.Create(3);
            var substances = MockSubstancesGenerator.Create(1);
            var rpfs = substances.ToDictionary(r => r, r => 1d);
            var memberships = substances.ToDictionary(r => r, r => 1d);
            var kineticConversionFactors = MockKineticModelsGenerator.CreateAbsorptionFactors(substances, 1D);
            var dietaryIndividualDayIntakes = MockDietaryIndividualDayIntakeGenerator.Create(individualDays, foods, substances, 0, true, random);

            var section = new DietaryIntakeDistributionSection();
            var header = new SectionHeader();
            section.Summarize(
                header,
                dietaryIndividualDayIntakes,
                rpfs,
                memberships,
                substances.First(),
                new double[] { 50, 95 },
                new double[] { 005, .1, 1, 2, 3, 95 },
                34,
                false,
                2.5,
                97.5
            );
            var subHeader = header.GetSubSectionHeader<IntakePercentileSection>();
            var sectionPercentile = subHeader.GetSummarySection() as IntakePercentileSection;
            section.SummarizeUncertainty(
                header,
                dietaryIndividualDayIntakes,
                rpfs,
                memberships,
                5,
                95,
                false
            );
        }
    }
}