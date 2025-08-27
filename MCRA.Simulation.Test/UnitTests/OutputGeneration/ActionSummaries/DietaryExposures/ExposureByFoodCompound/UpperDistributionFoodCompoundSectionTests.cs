using MCRA.General;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.DietaryExposures {
    /// <summary>
    /// OutputGeneration, ActionSummaries, DietaryExposures, ExposureByFoodCompound
    /// </summary>
    [TestClass()]
    public class UpperDistributionFoodCompoundSectionTests : SectionTestBase {
        /// <summary>
        /// No imputation, acute, test UpperDistributionFoodCompoundSection view
        /// </summary>
        [TestMethod]
        public void UpperDistributionFoodCompoundSectionSummary_SummarizeAcute1() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var foods = FakeFoodsGenerator.MockFoods("Apple", "Pear", "Bananas");
            var substances = FakeSubstancesGenerator.Create(3);
            var individualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(20, 2, true, random);
            var rpfs = substances.ToDictionary(r => r, r => 1d);
            var memberships = substances.ToDictionary(r => r, r => 1d);
            var exposures = FakeDietaryIndividualDayIntakeGenerator.Create(individualDays, foods, substances, 0.5, true, random);

            var section = new TotalDistributionFoodCompoundSection();
            section.Summarize(exposures, rpfs, memberships, foods, substances, ExposureType.Acute, 2.5, 97.5, 2.5, 97.5, false);
            var sectionUpper = new UpperDistributionFoodCompoundSection();
            sectionUpper.Summarize(exposures, rpfs, memberships, foods, substances, ExposureType.Acute, 2.5, 97.5, 2.5, 97.5, 94, false);
            Assert.IsTrue(sectionUpper.Records.Sum(c => c.ContributionPercentage) > 99.99 && sectionUpper.Records.Sum(c => c.ContributionPercentage) < 100.01);
            AssertIsValidView(sectionUpper);
        }

        /// <summary>
        /// With imputation, acute, test UpperDistributionFoodCompoundSection view
        /// </summary>
        [TestMethod]
        public void UpperDistributionFoodCompoundSectionSummary_SummarizeAcute2() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var foods = FakeFoodsGenerator.MockFoods("Apple", "Pear", "Bananas");
            var substances = FakeSubstancesGenerator.Create(3);
            var individualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(20, 2, true, random);
            var rpfs = substances.ToDictionary(r => r, r => 1d);
            var memberships = substances.ToDictionary(r => r, r => 1d);
            var exposures = FakeDietaryIndividualDayIntakeGenerator.GenerateImputed(individualDays, foods, substances, 0.5, true, random);

            var section = new TotalDistributionFoodCompoundSection();
            section.Summarize(exposures, rpfs, memberships, foods, substances, ExposureType.Acute, 2.5, 97.5, 2.5, 97.5, false);
            var sectionUpper = new UpperDistributionFoodCompoundSection();
            sectionUpper.Summarize(exposures, rpfs, memberships, foods, substances, ExposureType.Acute, 2.5, 97.5, 2.5, 97.5, 94, false);
            Assert.IsTrue(sectionUpper.Records.Sum(c => c.ContributionPercentage) > 99.99 && sectionUpper.Records.Sum(c => c.ContributionPercentage) < 100.01);
            AssertIsValidView(sectionUpper);
        }

        /// <summary>
        /// With imputation, acute uncertainty, test UpperDistributionFoodCompoundSection view
        /// </summary>
        [TestMethod]
        public void UpperDistributionFoodCompoundSectionSummary_SummarizeUncertaintyAcute1() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var foods = FakeFoodsGenerator.MockFoods("Apple", "Pear", "Bananas");
            var substances = FakeSubstancesGenerator.Create(3);
            var individualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(20, 2, true, random);
            var rpfs = substances.ToDictionary(r => r, r => 1d);
            var memberships = substances.ToDictionary(r => r, r => 1d);
            var exposures = FakeDietaryIndividualDayIntakeGenerator.GenerateImputed(individualDays, foods, substances, 0.5, true, random);

            var section = new TotalDistributionFoodCompoundSection();
            section.Summarize(exposures, rpfs, memberships, foods, substances, ExposureType.Acute, 2.5, 97.5, 2.5, 97.5, false);
            var sectionUpper = new UpperDistributionFoodCompoundSection();
            sectionUpper.Summarize(exposures, rpfs, memberships, foods, substances, ExposureType.Acute, 2.5, 97.5, 2.5, 97.5, 94, false);
            Assert.IsTrue(sectionUpper.Records.Sum(c => c.ContributionPercentage) > 99.99 && sectionUpper.Records.Sum(c => c.ContributionPercentage) < 100.01);
            sectionUpper.SummarizeUncertainty(exposures, rpfs, memberships, substances, ExposureType.Acute, 97.5, false);
            AssertIsValidView(sectionUpper);
        }

        /// <summary>
        /// No imputation, chronic, test UpperDistributionFoodCompoundSection view
        /// </summary>
        [TestMethod]
        public void UpperDistributionFoodCompoundSectionSummary_SummarizeChronic1() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var foods = FakeFoodsGenerator.MockFoods("Apple", "Pear", "Bananas");
            var substances = FakeSubstancesGenerator.Create(3);
            var individualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(20, 2, true, random);
            var rpfs = substances.ToDictionary(r => r, r => 1d);
            var memberships = substances.ToDictionary(r => r, r => 1d);
            var exposures = FakeDietaryIndividualDayIntakeGenerator.Create(individualDays, foods, substances, 0.5, true, random);

            var section = new TotalDistributionFoodCompoundSection();
            section.Summarize(exposures, rpfs, memberships, foods, substances, ExposureType.Chronic, 2.5, 97.5, 2.5, 97.5, false);
            var sectionUpper = new UpperDistributionFoodCompoundSection();
            sectionUpper.Summarize(exposures, rpfs, memberships, foods, substances, ExposureType.Acute, 2.5, 97.5, 2.5, 97.5, 94, false);
            Assert.IsTrue(sectionUpper.Records.Sum(c => c.ContributionPercentage) > 99.99 && sectionUpper.Records.Sum(c => c.ContributionPercentage) < 100.01);
            AssertIsValidView(sectionUpper);
        }

        /// <summary>
        /// With imputation, chronic, test UpperDistributionFoodCompoundSection view
        /// </summary>
        [TestMethod]
        public void UpperDistributionFoodCompoundSectionSummary_SummarizeChronic2() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var foods = FakeFoodsGenerator.MockFoods("Apple", "Pear", "Bananas");
            var substances = FakeSubstancesGenerator.Create(3);
            var individualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(20, 2, true, random);
            var rpfs = substances.ToDictionary(r => r, r => 1d);
            var memberships = substances.ToDictionary(r => r, r => 1d);
            var exposures = FakeDietaryIndividualDayIntakeGenerator.GenerateImputed(individualDays, foods, substances, 0.5, true, random);

            var section = new TotalDistributionFoodCompoundSection();
            section.Summarize(exposures, rpfs, memberships, foods, substances, ExposureType.Chronic, 2.5, 97.5, 2.5, 97.5, false);
            var sectionUpper = new UpperDistributionFoodCompoundSection();
            sectionUpper.Summarize(exposures, rpfs, memberships, foods, substances, ExposureType.Chronic, 2.5, 97.5, 2.5, 97.5, 94, false);
            Assert.IsTrue(sectionUpper.Records.Sum(c => c.ContributionPercentage) > 99.99 && sectionUpper.Records.Sum(c => c.ContributionPercentage) < 100.01);
            AssertIsValidView(sectionUpper);
        }

        /// <summary>
        /// With imputation, chronic, test UpperDistributionFoodCompoundSection view
        /// </summary>
        [TestMethod]
        public void UpperDistributionFoodCompoundSectionSummary_SummarizeUncertaintyChronic1() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var foods = FakeFoodsGenerator.MockFoods("Apple", "Pear", "Bananas");
            var substances = FakeSubstancesGenerator.Create(3);
            var individualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(20, 2, true, random);
            var rpfs = substances.ToDictionary(r => r, r => 1d);
            var memberships = substances.ToDictionary(r => r, r => 1d);
            var exposures = FakeDietaryIndividualDayIntakeGenerator.GenerateImputed(individualDays, foods, substances, 0.5, true, random);

            var section = new TotalDistributionFoodCompoundSection();
            section.Summarize(exposures, rpfs, memberships, foods, substances, ExposureType.Chronic, 2.5, 97.5, 2.5, 97.5, false);
            var sectionUpper = new UpperDistributionFoodCompoundSection();
            sectionUpper.Summarize(exposures, rpfs, memberships, foods, substances, ExposureType.Chronic, 2.5, 97.5, 2.5, 97.5, 94, false);
            Assert.IsTrue(sectionUpper.Records.Sum(c => c.ContributionPercentage) > 99.99 && sectionUpper.Records.Sum(c => c.ContributionPercentage) < 100.01);
            sectionUpper.SummarizeUncertainty(exposures, rpfs, memberships, substances, ExposureType.Chronic, 97.5, false);
            AssertIsValidView(sectionUpper);
        }
    }
}
