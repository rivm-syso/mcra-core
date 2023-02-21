using MCRA.Utils.Collections;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.KineticModelCalculation.AbsorptionFactorsGeneration;
using MCRA.Simulation.Constants;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

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
            var dict = new Dictionary<(ExposureRouteType, Compound), double> {
                { (ExposureRouteType.Dietary, cmpA), 1 },
                { (ExposureRouteType.Dietary, SimulationConstants.NullSubstance), 2 },
                { (ExposureRouteType.Dermal, SimulationConstants.NullSubstance), 3 }
            };

            var facA = dict.Get(cmpA);
            Assert.AreEqual(1, facA[ExposureRouteType.Dietary]);
            Assert.AreEqual(3, facA[ExposureRouteType.Dermal]);
            Assert.IsFalse(facA.ContainsKey(ExposureRouteType.Oral));

            var facB = dict.Get(cmpB);
            Assert.AreEqual(2, facB[ExposureRouteType.Dietary]);
            Assert.AreEqual(3, facB[ExposureRouteType.Dermal]);
            Assert.IsFalse(facB.ContainsKey(ExposureRouteType.Oral));
        }
    }
}
