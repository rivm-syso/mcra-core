using MCRA.General;
using MCRA.Simulation.Calculators.IntakeModelling;
using MCRA.Simulation.Calculators.IntakeModelling.IntakeModels;
using MCRA.Simulation.Calculators.IntakeModelling.IntakeModels.OIMCalculation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.Calculators.IntakeModelling {
    /// <summary>
    /// IntakeModelling calculator
    /// </summary>
    [TestClass]
    public class IntakeModelFactoryTests {

        /// <summary>
        /// Create intake model for BBN
        /// </summary>
        [TestMethod]
        public void IntakeModelFactoryTestBBN() {
            IntakeModelFactory factory = getFactory();
            var intakeModel = factory.CreateIntakeModel(
                null,
                false,
                ExposureType.Chronic,
                IntakeModelType.BBN,
                TransformType.Logarithmic);
            Assert.IsTrue(intakeModel is BBNModel);
        }

        /// <summary>
        /// Create intake model for ISUF
        /// </summary>
        [TestMethod]
        public void IntakeModelFactoryTestISUF() {
            IntakeModelFactory factory = getFactory();
            var intakeModel = factory.CreateIntakeModel(
                null,
                false,
                ExposureType.Chronic,
                IntakeModelType.ISUF,
                TransformType.Logarithmic);
            Assert.IsTrue(intakeModel is ISUFModel);
        }

        /// <summary>
        /// Create intake model for LNN
        /// </summary>
        [TestMethod]
        public void IntakeModelFactoryTestLNN() {
            IntakeModelFactory factory = getFactory();
            var intakeModel = factory.CreateIntakeModel(
                null,
                false,
                ExposureType.Chronic,
                IntakeModelType.LNN,
                TransformType.Logarithmic);
            Assert.IsTrue(intakeModel is LNNModel);
        }

        /// <summary>
        /// Create intake model for LNN0
        /// </summary>
        [TestMethod]
        public void IntakeModelFactoryTestLNN0() {
            IntakeModelFactory factory = getFactory();
            var intakeModel = factory.CreateIntakeModel(
                null,
                false,
                ExposureType.Chronic,
                IntakeModelType.LNN0,
                TransformType.Logarithmic);
            Assert.IsTrue(intakeModel is LNN0Model);
        }

        /// <summary>
        /// Create intake model for OIM
        /// </summary>
        [TestMethod]
        public void IntakeModelFactoryTestOIM() {
            IntakeModelFactory factory = getFactory();
            var intakeModel = factory.CreateIntakeModel(
                null,
                false,
                ExposureType.Chronic,
                IntakeModelType.OIM,
                TransformType.Logarithmic);
            Assert.IsTrue(intakeModel is OIMModel);
        }

        private static IntakeModelFactory getFactory() {
            return new IntakeModelFactory(
                new FrequencyModelCalculationSettings(new() { CovariateModelType = CovariateModelType.Cofactor }),
                new AmountModelCalculationSettings(new() { CovariateModelType = CovariateModelType.Cofactor }),
                new ISUFModelCalculationSettings(new() { GridPrecision = 20, NumberOfIterations = 100, SplineFit = false }),
                100000,
                0,
                Array.Empty<double>(),
                0,
                0);
        }
    }
}
