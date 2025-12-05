using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.AuthorisedUses {
    /// <summary>
    /// OutputGeneration, ActionSummaries, SubstanceAuthorisations
    /// </summary>
    [TestClass]
    public class AuthorisedUsesByFoodSubstanceSummarySectionTests : SectionTestBase {
        /// <summary>
        /// Summarize and test AuthorisedUsesByFoodSubstanceSummarySection view
        /// </summary>
        [TestMethod]
        public void AuthorisedUsesByFoodSubstanceSummarySection_Test1() {
            var foods = FakeFoodsGenerator.Create(3);
            var substances = FakeSubstancesGenerator.Create(3);
            var allSubstanceAuthorisations = FakeSubstanceAuthorisationsGenerator.Create(foods, substances);
            var substanceAuthorisations = new Dictionary<(Food, Compound), SubstanceAuthorisation>();
            foreach (var item in allSubstanceAuthorisations) {
                substanceAuthorisations[(item.Food, item.Substance)] = item;
            }
            var section = new AuthorisationsByFoodSubstanceSummarySection();
            section.Summarize(substanceAuthorisations);
            Assert.HasCount(9, section.Records);
            AssertIsValidView(section);
        }
    }
}