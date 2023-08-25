using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmBiologicalMatrixConcentrationConversion;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.Calculators.HumanMonitoringCalculation.TargetMatrixConcentrationConversion {

    [TestClass]
    public class TargetMatrixKineticConversionCalculatorTests {


        [DataRow(ConcentrationUnit.ugPerL, ConcentrationUnit.ugPerL, ConcentrationUnit.ugPerL, 0.4)]
        [DataRow(ConcentrationUnit.ugPerL, ConcentrationUnit.mgPerL, ConcentrationUnit.mgPerL, 400)]
        [DataRow(ConcentrationUnit.ugPerL, ConcentrationUnit.mgPerL, ConcentrationUnit.ugPerL, 0.4)]
        [DataRow(ConcentrationUnit.ugPerL, ConcentrationUnit.ugPermL, ConcentrationUnit.ugPermL, 400)]
        [DataRow(ConcentrationUnit.ugPerL, ConcentrationUnit.ugPermL, ConcentrationUnit.ugPerL, 0.4)]
        [TestMethod]
        public void MonitoringMissingValueImputationCalculatorFactory_TestCreate(
            ConcentrationUnit target,
            ConcentrationUnit doseFrom,
            ConcentrationUnit doseTo,
            double value
        ) {
            var targetUnit = new TargetUnit(
                target.GetSubstanceAmountUnit(),
                target.GetConcentrationMassUnit(),
                TimeScaleUnit.SteadyState,
                BiologicalMatrix.Blood
            );

            var substance = MockSubstancesGenerator.Create(1).First();
            var doseUnitFrom = doseFrom;

            var sourceUnit = new TargetUnit(
                doseUnitFrom.GetSubstanceAmountUnit(),
                doseUnitFrom.GetConcentrationMassUnit(),
                TimeScaleUnit.SteadyState,
                BiologicalMatrix.Urine,
                ExpressionType.Creatinine
            );

            var fakeConversionFactors = new List<KineticConversionFactor>() {
                new KineticConversionFactor() {
                    SubstanceFrom = substance,
                    BiologicalMatrixFrom = BiologicalMatrix.Urine,
                    ExpressionTypeFrom = ExpressionType.Creatinine,
                    DoseUnitFrom = doseUnitFrom,
                    SubstanceTo = substance,
                    DoseUnitTo = doseTo,
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
                    ConcentrationUnit.ugPermL,
                    ExpressionType.Creatinine,
                    BiologicalMatrix.Blood
                );

            Assert.AreEqual(value, result);
        }
    }
}
