using MCRA.Utils.Statistics;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.Concentrations {
    /// <summary>
    /// OutputGeneration, ActionSummaries, Concentrations
    /// </summary>
    [TestClass]
    public class ConcentrationLimitExceedancesByFoodDataSectionTests : SectionTestBase {

        /// <summary>
        /// Tests view availability of ConcentrationLimitExceedancesByFoodDataSection.
        /// </summary>
        [TestMethod]
        public void ConcentrationLimitExceedancesByFoodDataSection_TestView() {
            var section = new ConcentrationLimitExceedancesByFoodDataSection();
            AssertIsValidView(section);
        }

        /// <summary>
        /// Tests summarize method of ConcentrationLimitExceedancesByFoodDataSection.
        /// </summary>
        [TestMethod]
        public void ConcentrationLimitExceedancesByFoodDataSection_TestSummarize() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var foods = FakeFoodsGenerator.Create(20);
            var substances = FakeSubstancesGenerator.Create(3);
            var mrls = FakeMaximumConcentrationLimitsGenerator.Create(foods, substances, random);
            var foodSamples = FakeSamplesGenerator.CreateFoodSamples(foods, substances, numberOfSamples: 50);

            var section = new ConcentrationLimitExceedancesByFoodDataSection();
            section.Summarize(mrls.Values, foodSamples.ToLookup(r => r.Food), 1D);
            RenderView(section, filename: "TestSummarize.html");
        }
    }
}
