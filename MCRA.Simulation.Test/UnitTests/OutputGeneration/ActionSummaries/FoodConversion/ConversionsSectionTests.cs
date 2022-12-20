using MCRA.Simulation.OutputGeneration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.FoodConversions {
    /// <summary>
    /// OutputGeneration, ActionSummaries, FoodConversions
    /// </summary>
    [TestClass]
    public class ConversionsSectionTests : SectionTestBase {
        /// <summary>
        /// Test ConversionsSection view
        /// </summary>
        [TestMethod]
        public void ConversionsSection_Test1() {
            var section = new ConversionsSection();
            AssertIsValidView(section);
        }
    }
}