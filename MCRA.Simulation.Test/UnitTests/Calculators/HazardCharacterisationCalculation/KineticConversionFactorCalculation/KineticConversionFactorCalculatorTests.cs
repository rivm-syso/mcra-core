using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.HazardCharacterisationCalculation.KineticConversionFactorCalculation;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmKineticConversionFactor;
using MCRA.Simulation.Calculators.KineticModelCalculation;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using MCRA.Utils.Statistics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.Calculators.HazardCharacterisationCalculation.KineticConversionFactorCalculation {

    [TestClass]
    public class KineticConversionFactorCalculatorTests {

        [TestMethod]
        [DataRow(DoseUnit.ugPerKgBWPerDay, DoseUnit.ugPerL)]
        public void KineticConversionCalculator_TestForward(
            DoseUnit hazardDoseUnit,
            DoseUnit targetDoseUnit
        ) {
            var random = new McraRandomGenerator(1);
            var dose = 1D;
            var doseUnit = TargetUnit.FromExternalDoseUnit(hazardDoseUnit, ExposureRoute.Oral);
            var substances = MockSubstancesGenerator.Create(1);
            var substance = substances.First();
            var exposureRoutes = new List<ExposurePathType>() { ExposurePathType.Oral };
            var targetUnit = TargetUnit.FromInternalDoseUnit(DoseUnit.ugPerL, BiologicalMatrix.Liver);
            var kineticConversionFactors = MockKineticModelsGenerator.CreateKineticConversionFactors(substances, exposureRoutes, targetUnit);

            var kineticConversionFactorModels = kineticConversionFactors?
                .Select(c => KineticConversionFactorCalculatorFactory
                    .Create(c, false)
                ).ToList();

            var kineticModelFactory = new KineticModelCalculatorFactory(
                null,
                kineticConversionFactorModels
            );

            var calculator = new KineticConversionFactorCalculator(
                kineticModelFactory,
                70
            );
            var result = calculator.ComputeKineticConversionFactor(
                dose,
                doseUnit,
                substance,
                ExposureType.Acute,
                targetUnit,
                random
            );

            Assert.AreEqual(.5 * dose, result);
        }
    }
}
