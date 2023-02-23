using MCRA.Utils.Statistics.Histograms;
using MCRA.Simulation.OutputGeneration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.DietaryExposures {
    /// <summary>
    /// OutputGeneration, DietaryExposures, IntakeModels, ModelThenAdd
    /// </summary>
    [TestClass]
    public class UsualIntakeDistributionPerCategoryModelSectionTests : SectionTestBase {
        /// <summary>
        /// Test UsualIntakeDistributionPerCategoryModelSection view
        /// </summary>
        [TestMethod]
        public void UsualIntakeDistributionPerCategoryModelSection_TestView() {
            var section = new UsualIntakeDistributionPerCategoryModelSection() {
                IntakeDistributionBins = new List<HistogramBin> (),
            };
            AssertIsValidView(section);
        }
    }
}