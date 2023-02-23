using MCRA.General;
using MCRA.Simulation.Action.UncertaintyFactorial;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.Action.UncertaintyFactorial {

    /// <summary>
    /// Test creation of uncertainty factorial design.
    /// </summary>
    [TestClass]
    public class UncertaintyFactorialDesignGeneratorTests {

        /// <summary>
        /// Tests creating uncertainty factorial design for some
        /// uncertainty sources. Check whether design is properly
        /// created.
        /// </summary>
        [TestMethod]
        public void UncertaintyFactorialDesignGenerator_TestCreate() {
            var sources = new List<UncertaintySource>() {
                UncertaintySource.Concentrations,
                UncertaintySource.Individuals
            };
            var design = UncertaintyFactorialDesignGenerator.Create(sources);
            CollectionAssert.AreEquivalent(
                new List<string>() { "Concentrations", "Individuals", "MC"},
                design.UncertaintySources
            );
            Assert.AreEqual(4, design.TruthTable.Count);
            Assert.AreEqual(4, design.Count);
            Assert.AreEqual(4, design.DesignMatrix.GetLength(0));
            Assert.AreEqual(3, design.DesignMatrix.GetLength(1));
        }
    }
}
