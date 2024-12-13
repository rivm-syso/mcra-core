using MCRA.Simulation.OutputGeneration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.TargetExposures {
    /// <summary>
    /// OutputGeneration, ActionSummaries, TargetExposures, ExposureByRoute, ExposureDistribution, AggregateExposureDistribution
    /// </summary>
    [TestClass]
    public class AggregateChronicTotalIntakeDistributionSectionTests : SectionTestBase {
        /// <summary>
        /// Test AggregateTotalIntakeDistributionSection view
        /// </summary>
        [TestMethod]
        public void AggregateChronicTotalIntakeDistributionSection_Test1() {
            var section = new AggregateTotalIntakeDistributionSection() {
                IntakeDistributionBins = [],
                CategorizedHistogramBins = [],
            };

            AssertIsValidView(section);
        }
    }
}