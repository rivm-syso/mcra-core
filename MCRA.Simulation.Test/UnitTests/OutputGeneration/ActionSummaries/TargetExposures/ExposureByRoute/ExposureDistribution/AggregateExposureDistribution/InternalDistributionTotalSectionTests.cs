using MCRA.General;
using MCRA.Simulation.OutputGeneration;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.TargetExposures {
    /// <summary>
    /// OutputGeneration, ActionSummaries, TargetExposures, ExposureByRoute, ExposureDistribution, AggregateExposureDistribution
    /// </summary>
    [TestClass]
    public class InternalDistributionTotalSectionTests : SectionTestBase {
        /// <summary>
        /// Test AggregateTotalIntakeDistributionSection view
        /// </summary>
        [TestMethod]
        public void InternalDistributionTotalSection_TestView() {
            var section = new InternalDistributionTotalSection() {
                TargetUnit = TargetUnit.FromInternalDoseUnit(DoseUnit.ugPerL, BiologicalMatrix.Blood),
                Records = [],
                BoxPlotRecords = [],
                IntakeDistributionBins = [],
                CategorizedHistogramBins = [],
            };

            AssertIsValidView(section);
        }
    }
}
