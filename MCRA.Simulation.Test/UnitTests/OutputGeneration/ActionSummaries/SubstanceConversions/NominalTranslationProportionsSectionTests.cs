using MCRA.Simulation.OutputGeneration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.SubstanceConversions {
    /// <summary>
    /// OutputGeneration, ActionSummaries, SubstanceConversions
    /// </summary>
    [TestClass]
    public class NominalTranslationProportionsSectionTests : SectionTestBase {
        /// <summary>
        /// Test NominalTranslationProportionsSection view
        /// </summary>
        [TestMethod]
        public void NominalTranslationProportionsSection_Test1() {
            var section = new NominalTranslationProportionsSection() {
                Records = [
                    new NominalTranslationProportionRecord() {
                         ActiveSubstanceCodes = ["A"],
                         ActiveSubstanceNames = ["A"],
                         ConversionFactors = [100],
                    }
                ],
            };
            AssertIsValidView(section);
        }
    }
}