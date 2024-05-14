using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.KineticModelCalculation.AbsorptionFactorsGeneration;
using MCRA.Simulation.Constants;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.Calculators.KineticModelCalculation {

    /// <summary>
    /// KineticModelCalculation calculator
    /// </summary>
    [TestClass]
    public class AbsorptionFactorsExtensionsTests {

        [TestMethod]
        public void AbsorptionFactorsExtensions_TestGet() {
            var cmpA = new Compound("A");
            var cmpB = new Compound("B");
            var dict = new Dictionary<(ExposurePathType, Compound), double> {
                { (ExposurePathType.Oral, cmpA), 1 },
                { (ExposurePathType.Oral, SimulationConstants.NullSubstance), 2 },
                { (ExposurePathType.Dermal, SimulationConstants.NullSubstance), 3 }
            };

            var facA = dict.Get(cmpA);
            Assert.AreEqual(1, facA[ExposurePathType.Oral]);
            Assert.AreEqual(3, facA[ExposurePathType.Dermal]);
            Assert.IsTrue(facA.ContainsKey(ExposurePathType.Oral));

            var facB = dict.Get(cmpB);
            Assert.AreEqual(2, facB[ExposurePathType.Oral]);
            Assert.AreEqual(3, facB[ExposurePathType.Dermal]);
            Assert.IsTrue(facB.ContainsKey(ExposurePathType.Oral));
        }
    }
}
