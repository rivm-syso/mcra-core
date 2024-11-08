using MCRA.Utils.Statistics;
using MCRA.General;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.Concentrations {
    /// <summary>
    /// OutputGeneration, ActionSummaries, Concentrations
    /// </summary>
    [TestClass]
    public class ConcentrationLimitExceedancesDataSectionTests : SectionTestBase {

        /// <summary>
        /// Tests view availability of ConcentrationLimitExceedancesDataSection.
        /// </summary>
        [TestMethod]
        public void ConcentrationLimitExceedancesDataSection_TestView() {
            var section = new ConcentrationLimitExceedancesDataSection();
            AssertIsValidView(section);
        }

        /// <summary>
        /// Tests summarize method of ConcentrationLimitExceedancesDataSection.
        /// </summary>
        [TestMethod]
        public void ConcentrationLimitExceedancesDataSection_TestSummarize() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var foods = FakeFoodsGenerator.Create(5);
            var substances = FakeSubstancesGenerator.Create(5);
            var mrls = FakeMaximumConcentrationLimitsGenerator.Create(foods, substances, random);
            var foodSamples = FakeSamplesGenerator.CreateFoodSamples(foods, substances, numberOfSamples: 50);

            var section = new ConcentrationLimitExceedancesDataSection();
            section.Summarize(mrls.Values, foodSamples.ToLookup(r => r.Food), ConcentrationUnit.mgPerKg , 1D);
            RenderView(section, filename: "TestSummarize.html");
        }
    }
}
