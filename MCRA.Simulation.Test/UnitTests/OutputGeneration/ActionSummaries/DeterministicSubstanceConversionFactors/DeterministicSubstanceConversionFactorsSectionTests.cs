using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.DeterministicSubstanceConversionFactors {

    /// <summary>
    /// Tests create and summarize deterministic substance conversion factors section.
    /// </summary>
    [TestClass]
    public class DeterministicSubstanceConversionFactorsSectionTests : SectionTestBase {

        /// <summary>
        /// Test summarize and render deterministic substance conversion factors section.
        /// </summary>
        [TestMethod]
        public void DeterministicSubstanceConversionFactorsSection_TestSummarize() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var substances = FakeSubstancesGenerator.Create(6);
            var conversionFactors = FakeDeterministicSubstanceConversionFactorsGenerator.Create(
                substances.Take(4).ToList(),
                substances.Skip(2).ToList(),
                random
            );
            var section = new DeterministicSubstanceConversionFactorsSection();
            section.Summarize(conversionFactors);
            RenderView(section, filename: "TestSummarize.html");
        }

        /// <summary>
        /// Test summarize and render deterministic substance conversion factors section with empty collection
        /// of conversion factors.
        /// </summary>
        [TestMethod]
        public void DeterministicSubstanceConversionFactorsSection_TestSummarizeEmptyCollection() {
            var section = new DeterministicSubstanceConversionFactorsSection();
            section.Summarize([]);
            RenderView(section, filename: "TestSummarizeEmptyCollection.html");
        }

        /// <summary>
        /// Test summarize and render deterministic substance conversion factors section with null collection
        /// of conversion factors.
        /// </summary>
        [TestMethod]
        public void DeterministicSubstanceConversionFactorsSection_TestSummarizeNullCollection() {
            var section = new DeterministicSubstanceConversionFactorsSection();
            section.Summarize(null);
            RenderView(section, filename: "TestSummarizeEmptyCollection.html");
        }
    }
}