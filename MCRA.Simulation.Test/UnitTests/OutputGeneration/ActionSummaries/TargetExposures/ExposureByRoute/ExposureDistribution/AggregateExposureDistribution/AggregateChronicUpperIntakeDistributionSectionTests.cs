using MCRA.Utils.Statistics.Histograms;
using MCRA.General;
using MCRA.Simulation.OutputGeneration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.TargetExposures {
    /// <summary>
    /// OutputGeneration, ActionSummaries,TargetExposures, ExposureByRoute, ExposureDistribution, AggregateExposureDistribution
    /// </summary>
    [TestClass]
    public class AggregateChronicUpperIntakeDistributionSectionTests : SectionTestBase {
        /// <summary>
        /// Test AggregateUpperIntakeDistributionSection view
        /// </summary>
        [TestMethod]
        public void AggregateChronicUpperIntakeDistributionSection_Test1() {
            var section = new AggregateUpperIntakeDistributionSection() {
                IntakeDistributionBins = [],
                CategorizedHistogramBins = [],
            };

            AssertIsValidView(section);
        }
    }
}