using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmBiologicalMatrixConcentrationConversion;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.Calculators.HumanMonitoringCalculation.TargetMatrixConcentrationConversion {

    [TestClass]
    public class TargetMatrixKineticConversionCalculatorTests {


        [DataRow(ConcentrationUnit.ugPerL, ConcentrationUnit.ugPerL, ConcentrationUnit.ugPerL, ConcentrationUnit.ugPerL, 0.5, 0.4)]
        [DataRow(ConcentrationUnit.ugPerL, ConcentrationUnit.ugPerL, ConcentrationUnit.mgPerL, ConcentrationUnit.mgPerL, 0.5, 400)]
        [DataRow(ConcentrationUnit.ugPerL, ConcentrationUnit.ugPerL, ConcentrationUnit.mgPerL, ConcentrationUnit.ugPerL, 0.5, 0.4)]
        [DataRow(ConcentrationUnit.ugPerL, ConcentrationUnit.ugPerg, ConcentrationUnit.ugPerL, ConcentrationUnit.ugPerg, 0.5, 0.4)]
        [DataRow(ConcentrationUnit.ugPerL, ConcentrationUnit.ugPerg, ConcentrationUnit.mgPerL, ConcentrationUnit.ngPerg, 0.5, 0.0004)]
        [DataRow(ConcentrationUnit.ugPerL, ConcentrationUnit.ugPerg, ConcentrationUnit.mgPerL, ConcentrationUnit.ugPerL, 0.5, 0.4)]
        [TestMethod]
        public void MonitoringMissingValueImputationCalculatorFactory_TestCreate(
            ConcentrationUnit unitData,
            ConcentrationUnit target,
            ConcentrationUnit doseFrom,
            ConcentrationUnit doseTo,
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
            var expressionTypeFrom = ExpressionType.Creatinine;
            var expressionTypeTo = ExpressionType.None;
            var fakeConversionFactors = new List<KineticConversionFactor>() {
                new KineticConversionFactor() {
                    SubstanceFrom = substance,
                    BiologicalMatrixFrom = biologicalMatrixSource,
                    ExpressionTypeFrom = expressionTypeFrom,
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
                targetUnit.BiologicalMatrix
            );

            var result = converter
                .GetTargetConcentration(
                    .8,
                    substance,
                    expressionTypeFrom,
                    targetUnit,
                    biologicalMatrixSource
                );

            Assert.AreEqual(expected, result);
        }
    }
}
