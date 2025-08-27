using MCRA.Simulation.OutputGeneration;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.DietaryExposures {
    /// <summary>
    /// OutputGeneration, ActionSummaries, DietaryExposures, DietaryExposurePointEstimates
    /// </summary>
    [TestClass]
    public class DietaryExposurePointEstimatesSectionTests : SectionTestBase {
        /// <summary>
        /// Test DietaryExposurePointEstimatesSection view
        /// </summary>
        [TestMethod]
        public void DietaryExposurePointEstimatesSection_Tests() {
            var section = new AcuteSingleValueDietaryExposuresSection();
            section.Records = [
                new AcuteSingleValueDietaryExposureRecord() { }
            ];
            AssertIsValidView(section);
        }
    }
}
