using MCRA.General;
using MCRA.General.ModuleDefinitions.Interfaces;
using MCRA.Simulation.Calculators.FocalCommodityMeasurementReplacementCalculation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.Calculators.FocalCommodityMeasurementReplacementCalculation {

    /// <summary>
    /// Tests for the focal commodity measurement replacement calculator factory.
    /// </summary>
    [TestClass]
    public class FocalCommodityMeasurementReplacementCalculatorFactoryTests {

        internal class MockFocalCommodityMeasurementReplacementCalculatorFactorySettings : IFocalCommodityMeasurementReplacementCalculatorFactorySettings {
            public FocalCommodityReplacementMethod FocalCommodityReplacementMethod { get; set; }
            public double FocalCommodityScenarioOccurrencePercentage { get; set; }
            public double FocalCommodityConcentrationAdjustmentFactor { get; set; }
            public bool FocalCommodityIncludeProcessedDerivatives { get; set; }
        }

        /// <summary>
        /// Test create measurement removal calculator.
        /// </summary>
        [TestMethod]
        public void FocalCommodityMeasurementReplacementCalculatorFactory_TestCreate1() {
            var settings = new MockFocalCommodityMeasurementReplacementCalculatorFactorySettings() {
                FocalCommodityReplacementMethod = FocalCommodityReplacementMethod.MeasurementRemoval,
                FocalCommodityScenarioOccurrencePercentage = .5,
                FocalCommodityConcentrationAdjustmentFactor = 1
            };
            var calculator = new FocalCommodityMeasurementReplacementCalculatorFactory(settings);
            var model = calculator.Create(
                null,
                null,
                null,
                null,
                ConcentrationUnit.mgPerKg
            );
            Assert.IsTrue(model is FocalCommodityMeasurementRemovalCalculator);
        }

        /// <summary>
        /// Test create replace measurements by measurements from samples calculator.
        /// </summary>
        [TestMethod]
        public void FocalCommodityMeasurementReplacementCalculatorFactory_TestCreate2() {
            var settings = new MockFocalCommodityMeasurementReplacementCalculatorFactorySettings() {
                FocalCommodityReplacementMethod = FocalCommodityReplacementMethod.ReplaceSubstances,
                FocalCommodityScenarioOccurrencePercentage = .5,
                FocalCommodityConcentrationAdjustmentFactor = 1
            };
            var calculator = new FocalCommodityMeasurementReplacementCalculatorFactory(settings);
            var model = calculator.Create(
                null,
                null,
                null,
                null,
                ConcentrationUnit.mgPerKg
            );
            Assert.IsTrue(model is FocalCommodityMeasurementBySamplesReplacementCalculator);
        }

        /// <summary>
        /// Test create replace measurements by MRL calculator.
        /// </summary>
        [TestMethod]
        public void FocalCommodityMeasurementReplacementCalculatorFactory_TestCreate3() {
            var settings = new MockFocalCommodityMeasurementReplacementCalculatorFactorySettings() {
                FocalCommodityReplacementMethod = FocalCommodityReplacementMethod.ReplaceSubstanceConcentrationsByLimitValue,
                FocalCommodityScenarioOccurrencePercentage = .5,
                FocalCommodityConcentrationAdjustmentFactor = 1
            };
            var calculator = new FocalCommodityMeasurementReplacementCalculatorFactory(settings);
            var model = calculator.Create(
                null,
                null,
                null,
                null,
                ConcentrationUnit.mgPerKg
            );
            Assert.IsTrue(model is FocalCommodityMeasurementMrlReplacementCalculator);
        }

        /// <summary>
        /// Assert fail for sample-based replacement methods.
        /// </summary>
        [TestMethod]
        public void FocalCommodityMeasurementReplacementCalculatorFactory_TestCreate4() {
            var settings = new MockFocalCommodityMeasurementReplacementCalculatorFactorySettings() {
                FocalCommodityReplacementMethod = FocalCommodityReplacementMethod.AppendSamples,
                FocalCommodityScenarioOccurrencePercentage = .5,
                FocalCommodityConcentrationAdjustmentFactor = 1
            };
            var calculator = new FocalCommodityMeasurementReplacementCalculatorFactory(settings);
            Assert.ThrowsException<NotImplementedException>(() => calculator
                .Create(
                    null,
                    null,
                    null,
                    null,
                    ConcentrationUnit.mgPerKg
                )
            );
        }

        /// <summary>
        /// Assert fail for sample-based replacement methods.
        /// Strange test Waldo
        /// </summary>
        [TestMethod]
        public void FocalCommodityMeasurementReplacementCalculatorFactory_TestCreate5() {
            var settings = new MockFocalCommodityMeasurementReplacementCalculatorFactorySettings() {
                FocalCommodityReplacementMethod = FocalCommodityReplacementMethod.AppendSamples,
                FocalCommodityScenarioOccurrencePercentage = .5,
                FocalCommodityConcentrationAdjustmentFactor = 1
            };
            var calculator = new FocalCommodityMeasurementReplacementCalculatorFactory(settings);
            Assert.ThrowsException<NotImplementedException>(() => calculator
                .Create(
                    null,
                    null,
                    null,
                    null,
                    ConcentrationUnit.mgPerKg
                )
            );
        }
    }
}
