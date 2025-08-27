using MCRA.Simulation.OutputGeneration;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.DietaryExposures {
    /// <summary>
    /// OutputGeneration, ActionSummaries,DietaryExposures, IntakeModels, LNN0BBN
    /// </summary>
    [TestClass]
    public class UncorrelatedModelSectionTests : SectionTestBase {
        /// <summary>
        /// Test UncorrelatedModelSection view
        /// </summary>
        [TestMethod]
        public void UncorrelatedModelSection_Test1() {
            var section = new UncorrelatedModelSection() {
            };
            AssertIsValidView(section);
        }
    }
}


