using MCRA.General;
using MCRA.Simulation.Calculators.HazardCharacterisationCalculation.KineticConversionFactorCalculation;
using MCRA.Simulation.Calculators.KineticConversionFactorModels;
using MCRA.Simulation.Calculators.KineticConversionCalculation;
using MCRA.Simulation.Calculators.PbkModelCalculation;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using MCRA.Utils.Statistics;

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
            var substances = FakeSubstancesGenerator.Create(1);
            var substance = substances.First();
            var routes = new List<ExposureRoute>() { ExposureRoute.Oral };
            var targetUnit = TargetUnit.FromInternalDoseUnit(DoseUnit.ugPerL, BiologicalMatrix.Liver);
            var kineticConversionFactors = FakeKineticConversionFactorModelsGenerator.CreateKineticConversionFactors(substances, routes, targetUnit);

            var kineticConversionFactorModels = kineticConversionFactors?
                .Select(c => KineticConversionFactorCalculatorFactory
                    .Create(c, false)
                ).ToList();

            var kineticModelFactory = new KineticConversionCalculatorFactory(
                null,
                kineticConversionFactorModels,
                null,
                TargetLevelType.Internal,
                InternalModelType.ConversionFactorModel
            );

            var calculator = new KineticConversionFactorCalculator(
                kineticModelFactory,
                new PbkSimulationSettings() {
                    NumberOfSimulatedDays = 10,
                    UseRepeatedDailyEvents = true,
                },
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
