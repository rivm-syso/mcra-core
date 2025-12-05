using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.DietaryExposures {
    /// <summary>
    /// OutputGeneration, ActionSummaries, DietaryExposures, TDSReductionFactors
    /// </summary>
    [TestClass()]
    public class TdsReductionFactorsSectionTests : SectionTestBase {

        /// <summary>
        /// Test reduction factors smaller than 1
        /// </summary>
        [TestMethod]
        public void TdsReductionFactorsSection_TestSummary1() {
            var foods = FakeFoodsGenerator.Create(3);
            var substances = FakeSubstancesGenerator.Create(3);
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var reductionFactors = new Dictionary<(Food, Compound), double>();
            foreach (var food in foods) {
                foreach (var substance in substances) {
                    reductionFactors[(food, substance)] = random.NextDouble();
                }
            }
            var section = new TdsReductionFactorsSection();
            section.Summarize(reductionFactors);
            Assert.HasCount(9, section.Records);
            AssertIsValidView(section);
        }

        /// <summary>
        /// Test reduction factors equal to 1
        /// </summary>
        [TestMethod]
        public void TdsReductionFactorsSection_TestSummary2() {
            var foods = FakeFoodsGenerator.Create(3);
            var substances = FakeSubstancesGenerator.Create(3);
            var reductionFactors = new Dictionary<(Food, Compound), double>();
            foreach (var food in foods) {
                foreach (var substance in substances) {
                    reductionFactors[(food, substance)] = 1;
                }
            }
            var section = new TdsReductionFactorsSection();
            section.Summarize(reductionFactors);
            Assert.IsEmpty(section.Records);
            AssertIsValidView(section);
        }
    }
}
