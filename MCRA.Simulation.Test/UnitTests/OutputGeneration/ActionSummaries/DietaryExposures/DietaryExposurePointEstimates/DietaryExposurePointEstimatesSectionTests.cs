using MCRA.Simulation.OutputGeneration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
            section.Records = new List<AcuteSingleValueDietaryExposureRecord>();
            section.Records.Add(new AcuteSingleValueDietaryExposureRecord() { });
            AssertIsValidView(section);
        }
    }
}
