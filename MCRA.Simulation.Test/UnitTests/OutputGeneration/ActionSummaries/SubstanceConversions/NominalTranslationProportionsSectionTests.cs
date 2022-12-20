using MCRA.Simulation.OutputGeneration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

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
                Records = new List<NominalTranslationProportionRecord>() {
                    new NominalTranslationProportionRecord() {
                         ActiveSubstanceCodes = new List<string>(){"A"},
                         ActiveSubstanceNames = new List<string>(){"A"},
                         ConversionFactors = new List<double>(){100},
                    }
                },
            };
            AssertIsValidView(section);
        }
    }
}