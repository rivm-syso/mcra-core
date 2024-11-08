using MCRA.Simulation.OutputGeneration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.TargetExposures {
    /// <summary>
    /// OutputGeneration, ActionSummaries, TargetExposures, ExposureByCompound, LinearModel
    /// </summary>
    [TestClass]
    public class LinearModelSectionTests : SectionTestBase {
        /// <summary>
        /// Test LinearModelSection view
        /// </summary>
        [TestMethod]
        public void LinearModelSection_Test1() {
            var section = new LinearModelSection() {
                Records = [
                    new CompoundRecord() {
                        Code = "C",
                        Name = "C"
                    }
                ],
            };
            AssertIsValidView(section);
        }
    }
}