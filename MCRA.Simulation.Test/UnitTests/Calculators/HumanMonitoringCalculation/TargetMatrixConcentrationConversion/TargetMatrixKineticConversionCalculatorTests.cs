using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmBiologicalMatrixConcentrationConversion;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.Calculators.HumanMonitoringCalculation.TargetMatrixConcentrationConversion {

    [TestClass]
    public class TargetMatrixKineticConversionCalculatorTests {

        [TestMethod]
        public void MonitoringMissingValueImputationCalculatorFactory_TestCreate() {
            var targetUnit = new TargetUnit(
                SubstanceAmountUnit.Micrograms,
                ConcentrationMassUnit.Liter,
                TimeScaleUnit.SteadyState,
                BiologicalMatrix.Blood
            );

            var substance = MockSubstancesGenerator.Create(1).First();
            var fakeConversionFactors = new List<KineticConversionFactor>() {
                new KineticConversionFactor() {
                    SubstanceFrom = substance,
                    BiologicalMatrixFrom = BiologicalMatrix.Urine,
                    ExpressionTypeFrom = ExpressionType.Creatinine,
                    DoseUnitFrom = ConcentrationUnit.mgPerL,
                    SubstanceTo = substance,
                    DoseUnitTo = ConcentrationUnit.ugPerL,
                    BiologicalMatrixTo = BiologicalMatrix.Blood,
                    ConversionFactor = 0.5
                } 
            };
            var converter = new TargetMatrixKineticConversionCalculator(
                fakeConversionFactors,
                targetUnit
            );
            var result = converter
                .GetTargetConcentration(
                    .8,
                    substance,
                    TargetUnit.FromDoseUnit(DoseUnit.mgPerL, BiologicalMatrix.Urine)
                );
            Assert.AreEqual(0.0004, result);
        }
    }
}
