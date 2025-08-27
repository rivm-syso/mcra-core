using MCRA.General;
using MCRA.Simulation.Calculators.IntakeModelling;
using MCRA.Simulation.Calculators.IntakeModelling.IntakeTransformers;

namespace MCRA.Simulation.Test.UnitTests.IntakeModelling {
    /// <summary>
    /// IntakeModelling calculator
    /// </summary>
    [TestClass]
    public class IntakeTransformerFactoryTests {

        /// <summary>
        /// IntakeTransformerFactory: create identity transformer.
        /// </summary>
        [TestMethod]
        public void IntakeTransformerFactory_TestCreateIdentityTransformer() {
            var transformer = IntakeTransformerFactory.Create(TransformType.NoTransform, () => double.NaN);
            Assert.AreEqual(typeof(IdentityTransformer), transformer.GetType());
            Assert.AreEqual(TransformType.NoTransform, transformer.TransformType);
        }

        /// <summary>
        /// IntakeTransformerFactory: create logarithmic transformer.
        /// </summary>
        [TestMethod]
        public void IntakeTransformerFactory_TestCreateLogarithmicTransformer() {
            var transformer = IntakeTransformerFactory.Create(TransformType.Logarithmic, () => double.NaN);
            Assert.AreEqual(typeof(LogTransformer), transformer.GetType());
            Assert.AreEqual(TransformType.Logarithmic, transformer.TransformType);
        }

        /// <summary>
        /// IntakeTransformerFactory: create power transformer.
        /// </summary>
        [TestMethod]
        public void IntakeTransformerFactory_TestCreatePowerTransformer() {
            var transformer = IntakeTransformerFactory.Create(TransformType.Power, () => 2.5);
            Assert.AreEqual(typeof(PowerTransformer), transformer.GetType());
            Assert.AreEqual(2.5, (transformer as PowerTransformer).Power);
            Assert.AreEqual(TransformType.Power, transformer.TransformType);
        }
    }
}
