using MCRA.Utils.Statistics.Histograms;
using MCRA.Simulation.OutputGeneration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.DietaryExposures {
    /// <summary>
    /// OutputGeneration, ActionSummaries, DietaryExposures, IntakeModels
    /// </summary>
    [TestClass]
    public class UsualIntakeDistributionSectionTests : SectionTestBase {
        /// <summary>
        /// Test ModelBasedDistributionSection model based view
        /// </summary>
        [TestMethod]
        public void UsualIntakeDistributionSection_Test1() {
            var section = new ModelBasedDistributionSection() {
                IntakeDistributionBins = new List<HistogramBin>(),
            };
            AssertIsValidView(section);
        }

        /// <summary>
        /// Test ModelAssistedDistributionSection model assisted view
        /// </summary>
        [TestMethod]
        public void UsualIntakeDistributionSection_Test2() {
            var section = new ModelAssistedDistributionSection() {
                IntakeDistributionBins = new List<HistogramBin>(),
            };
            AssertIsValidView(section);
        }

        /// <summary>
        /// Test OIMDistributionSection model oim view
        /// </summary>
        [TestMethod]
        public void UsualIntakeDistributionSection_Test3() {
            var section = new OIMDistributionSection() {
                IntakeDistributionBins = new List<HistogramBin>(),
            };
            AssertIsValidView(section);
        }

        /// <summary>
        /// Test ModelThenAddSummarySection model MTA view
        /// </summary>
        [TestMethod]
        public void UsualIntakeDistributionSection_Test4() {
            var section = new ModelThenAddSummarySection() {
                FoodNames = new List<string> { "apple", "pineapple" }
            };
            AssertIsValidView(section);
        }
    }
}