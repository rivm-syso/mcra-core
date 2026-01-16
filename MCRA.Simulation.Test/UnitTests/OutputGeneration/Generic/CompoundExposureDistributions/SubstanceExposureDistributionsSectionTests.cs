using MCRA.Simulation.OutputGeneration;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.Generic {
    /// <summary>
    /// OutputGeneration, Generic, CompoundExposureDistributions
    /// </summary>
    [TestClass]
    public class SubstanceExposureDistributionsSectionTests : SectionTestBase {
        /// <summary>
        /// Test CompoundExposureDistributionsSection view
        /// </summary>
        [TestMethod]
        public void CompoundExposureDistributionSection_Test1() {
            var section = new SubstanceExposureDistributionsSection() {
                SubstanceExposureDistributionRecords = [
                    new SubstanceExposureDistributionRecord(){
                        HistogramBins = [],
                    }
                ],
            };
            AssertIsValidView(section);
        }
    }
}