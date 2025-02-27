using MCRA.Simulation.OutputGeneration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.TargetExposures {
    /// <summary>
    /// OutputGeneration, ActionSummaries,TargetExposures, ExposureByRoute, ExposureDistribution, AggregateExposureDistribution
    /// </summary>
    [TestClass]
    public class InternalAcuteDistributionUpperSectionTests : SectionTestBase {
        /// <summary>
        /// Test AggregateUpperIntakeDistributionSection view
        /// </summary>
        [TestMethod]
        public void AggregateChronicUpperIntakeDistributionSection_Test1() {
            var section = new InternalAcuteDistributionUpperSection() {
                IntakeDistributionBins = [],
                CategorizedHistogramBins = [],
            };

            AssertIsValidView(section);
        }
    }
}