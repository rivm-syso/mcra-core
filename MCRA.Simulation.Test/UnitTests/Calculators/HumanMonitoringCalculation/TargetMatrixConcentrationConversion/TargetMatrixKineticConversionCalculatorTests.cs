using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmBiologicalMatrixConcentrationConversion;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.Calculators.HumanMonitoringCalculation.TargetMatrixConcentrationConversion {

    [TestClass]
    public class TargetMatrixKineticConversionCalculatorTests {

        [DataRow(0.8, ConcentrationUnit.ugPerL, ConcentrationUnit.ugPerL, ConcentrationUnit.ugPerL, ConcentrationUnit.ugPerL, 0.5, 0.4)]
        [DataRow(0.8, ConcentrationUnit.ugPerL, ConcentrationUnit.ugPerL, ConcentrationUnit.mgPerL, ConcentrationUnit.mgPerL, 0.5, 0.4)]
        [DataRow(0.8, ConcentrationUnit.ugPerL, ConcentrationUnit.ugPerL, ConcentrationUnit.mgPerL, ConcentrationUnit.ugPerL, 0.5, 400)]
        [DataRow(0.8, ConcentrationUnit.mgPerL, ConcentrationUnit.ugPerL, ConcentrationUnit.ugPerL, ConcentrationUnit.ugPerL, 0.5, 400)]
        [DataRow(0.8, ConcentrationUnit.ugPerL, ConcentrationUnit.ugPerL, ConcentrationUnit.ngPerg, ConcentrationUnit.ugPerg, 0.5, 0.0004)]
        [DataRow(0.8, ConcentrationUnit.ugPerL, ConcentrationUnit.ugPerL, ConcentrationUnit.ugPerg, ConcentrationUnit.ngPerg, 0.5, 400)]
        [TestMethod]
        public void TargetMatrixKineticConversionCalculator_TestGetTargetConcentration(
            double concentration,
            ConcentrationUnit concentrationUnitSource,
            ConcentrationUnit doseFrom,
            ConcentrationUnit doseTo,
            ConcentrationUnit target,
            double factor,
            double expected
        ) {
            var targetUnit = new TargetUnit(
                target.GetSubstanceAmountUnit(),
                target.GetConcentrationMassUnit(),
                TimeScaleUnit.SteadyState,
                BiologicalMatrix.Blood
            );

            var substance = MockSubstancesGenerator.Create(1).First();
            var doseUnitFrom = doseFrom;
            var biologicalMatrixSource = BiologicalMatrix.Urine;
            var expressionTypeSource = ExpressionType.Creatinine;
            var expressionTypeTo = ExpressionType.None;
            var fakeConversionFactors = new List<KineticConversionFactor>() {
                new KineticConversionFactor() {
                    SubstanceFrom = substance,
                    BiologicalMatrixFrom = biologicalMatrixSource,
                    ExpressionTypeFrom = expressionTypeSource,
                    DoseUnitFrom = doseUnitFrom,
                    SubstanceTo = substance,
                    DoseUnitTo = doseTo,
                    BiologicalMatrixTo = BiologicalMatrix.Blood,
                    ExpressionTypeTo = expressionTypeTo,
                    ConversionFactor = factor
                }
            };

            var converter = new TargetMatrixKineticConversionCalculator(
                fakeConversionFactors,
                targetUnit.BiologicalMatrix,
                targetUnit.ExpressionType
            );

            var result = converter
                .GetTargetConcentration(
                    concentration,
                    substance,
                    expressionTypeSource,
                    biologicalMatrixSource,
                    concentrationUnitSource,
                    targetUnit);

            Assert.AreEqual(expected, result);
        }
    }
}
