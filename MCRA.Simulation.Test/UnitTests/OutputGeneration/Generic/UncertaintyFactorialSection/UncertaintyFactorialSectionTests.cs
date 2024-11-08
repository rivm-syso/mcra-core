using MCRA.Simulation.OutputGeneration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.Generic {
    /// <summary>
    /// OutputGeneration, Generic, UncertaintyFactorialSection
    /// </summary>
    [TestClass]
    public class UncertaintyFactorialSectionTests : SectionTestBase {
        /// <summary>
        /// Test UncertaintyFactorialSection view
        /// </summary>
        [TestMethod]
        public void UncertaintyFactorialSection_Test1() {
            var section = new UncertaintyFactorialSection() {
                Contributions = [new List<double>() { 100 }],
                Design = [new List<double>() { 100 }],
                UncertaintySources = ["A"],
            };
            AssertIsValidView(section);
        }
    }
}