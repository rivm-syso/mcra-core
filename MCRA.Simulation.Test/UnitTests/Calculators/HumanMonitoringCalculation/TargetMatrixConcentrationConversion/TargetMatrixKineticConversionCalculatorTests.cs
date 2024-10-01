using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers;
using MCRA.General;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.KineticConversions;
using MCRA.Simulation.Calculators.KineticConversionFactorModels;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.Calculators.HumanMonitoringCalculation.TargetMatrixConcentrationConversion {

    [TestClass]
    public class TargetMatrixKineticConversionCalculatorTests {

        [DataRow(0.8, DoseUnit.ugPerL, DoseUnit.ugPerL, DoseUnit.ugPerL, DoseUnit.ugPerL, 0.5, 0.4)]
        [DataRow(0.8, DoseUnit.ugPerL, DoseUnit.ugPerL, DoseUnit.mgPerL, DoseUnit.mgPerL, 0.5, 0.4)]
        [DataRow(0.8, DoseUnit.ugPerL, DoseUnit.ugPerL, DoseUnit.mgPerL, DoseUnit.ugPerL, 0.5, 400)]
        [DataRow(0.8, DoseUnit.mgPerL, DoseUnit.ugPerL, DoseUnit.ugPerL, DoseUnit.ugPerL, 0.5, 400)]
        [DataRow(0.8, DoseUnit.ugPerL, DoseUnit.ugPerL, DoseUnit.ngPerg, DoseUnit.ugPerg, 0.5, 0.0004)]
        [DataRow(0.8, DoseUnit.ugPerL, DoseUnit.ugPerL, DoseUnit.ugPerg, DoseUnit.ngPerg, 0.5, 400)]
        [TestMethod]
        public void TargetMatrixKineticConversionCalculator_TestGetTargetConcentration(
            double concentration,
            DoseUnit unitSource,
            DoseUnit doseFrom,
            DoseUnit doseTo,
            DoseUnit target,
            double factor,
            double expected
        ) {
            var targetUnit = new TargetUnit(
                new ExposureTarget(BiologicalMatrix.Blood),
                new ExposureUnitTriple(
                    target.GetSubstanceAmountUnit(),
                    target.GetConcentrationMassUnit(),
                    TimeScaleUnit.SteadyState
                )
            );
            var biologicalMatrixSource = BiologicalMatrix.Urine;
            var expressionTypeSource = ExpressionType.Creatinine;
            var sourceTargetUnit = new TargetUnit(
                new ExposureTarget(biologicalMatrixSource, expressionTypeSource),
                ExposureUnitTriple.FromDoseUnit(unitSource)
            );

            var substance = MockSubstancesGenerator.Create(1).First();
            var expressionTypeTo = ExpressionType.None;
            var fakeConversionFactor = new KineticConversionFactor() {
                SubstanceFrom = substance,
                BiologicalMatrixFrom = biologicalMatrixSource,
                ExpressionTypeFrom = expressionTypeSource,
                DoseUnitFrom = ExposureUnitTriple.FromDoseUnit(doseFrom),
                SubstanceTo = substance,
                DoseUnitTo = ExposureUnitTriple.FromDoseUnit(doseTo),
                BiologicalMatrixTo = BiologicalMatrix.Blood,
                ExpressionTypeTo = expressionTypeTo,
                ConversionFactor = factor
            };

            var conversionFactorModel = KineticConversionFactorCalculatorFactory.Create(fakeConversionFactor, false);
            var converter = new TargetMatrixKineticConversionCalculator(
                [conversionFactorModel],
                targetUnit
            );

            var rec = new HbmSubstanceTargetExposure() {
                Exposure = concentration,
                IsAggregateOfMultipleSamplingMethods = false,
                SourceSamplingMethods = new List<HumanMonitoringSamplingMethod>(),
                Substance = substance
            };
            var individualDay = new SimulatedIndividualDay();
            var result = converter
                .GetSubstanceTargetExposures(
                    rec,
                    individualDay,
                    sourceTargetUnit,
                    double.NaN
                ); ;

            Assert.AreEqual(expected, result.First().Exposure);
        }
    }
}
