using MCRA.Utils.Statistics;
using MCRA.General;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.TargetExposures {
    /// <summary>
    /// OutputGeneration, ActionSummaries, TargetExposures, ExposureByRoute, ExposureDistribution, NonDietaryExposureDistribution
    /// </summary>
    [TestClass]
    public class NonDietaryIntakeDistributionSectionTests : SectionTestBase {

        /// <summary>
        /// Summarize nondietary exposure (uncertainty) and test NonDietaryTotalIntakeDistributionSection,
        /// NonDietaryUpperIntakeDistributionSection, NonDietaryIntakePercentageSection, NonDietaryIntakePercentileSection view
        /// </summary>
        [TestMethod]
        public void NonDietaryIntakeDistributionSection_Test1() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var exposureRoutes = new[] { ExposurePathType.Dermal, ExposurePathType.Oral, ExposurePathType.Inhalation };
            var individuals = FakeIndividualsGenerator.Create(25, 1, random);
            var substances = FakeSubstancesGenerator.Create(3);
            var rpfs = substances.ToDictionary(r => r, r => 1d);
            var memberships = substances.ToDictionary(r => r, r => 1d);
            var kineticConversionFactors = FakeKineticModelsGenerator.CreateAbsorptionFactors(substances, exposureRoutes, 1D);
            var nonDietaryIntakes = FakeNonDietaryIndividualDayIntakeGenerator.Generate(individuals, substances, exposureRoutes, 0, random);
            var header = new SectionHeader();
            var section = new NonDietaryIntakeDistributionSection();
            section.Summarize(
                header,
                nonDietaryIntakes,
                rpfs,
                memberships,
                substances.First(),
                [50, 95],
                [005, .1, 1, 2, 3, 95],
                34,
                false,
                2.5,
                97.5
            );

            var subHeader = header.GetSubSectionHeader<IntakePercentileSection>();
            var section1 = subHeader.GetSummarySection() as IntakePercentileSection;

            subHeader = header.GetSubSectionHeader<IntakePercentageSection>();
            var section2 = subHeader.GetSummarySection() as IntakePercentageSection;

            subHeader = header.GetSubSectionHeader<NonDietaryTotalIntakeDistributionSection>();
            var section3 = subHeader.GetSummarySection() as NonDietaryTotalIntakeDistributionSection;

            subHeader = header.GetSubSectionHeader<NonDietaryUpperIntakeDistributionSection>();
            var section4 = subHeader.GetSummarySection() as NonDietaryUpperIntakeDistributionSection;
            section.SummarizeUncertainty(
                header,
                nonDietaryIntakes,
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