using MCRA.General;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;
using MCRA.Simulation.Calculators.KineticModelCalculation.PbpkModelCalculation;
using MCRA.Simulation.Calculators.KineticModelCalculation.PbpkModelCalculation.DesolvePbkModelCalculators;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using MCRA.Utils.Logger;
using MCRA.Utils.R.REngines;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Test.UnitTests.Calculators.KineticModelCalculation.PbkModelCalculation.DesolvePbkModelCalculators {

    /// <summary>
    /// KineticModelCalculation calculator
    /// </summary>
    [TestClass]
    public abstract class DesolvePbkModelCalculatorBaseTests : PbkModelCalculatorTestsBase {

        protected override PbkSimulationSettings getDefaultSimulationSettings() {
            return new PbkSimulationSettings() {
                NumberOfSimulatedDays = 10,
                UseRepeatedDailyEvents = true,
                NumberOfOralDosesPerDay = 1,
                NonStationaryPeriod = 5
            };
        }

        [TestMethod]
        [DataRow(ExposureType.Acute)]
        [DataRow(ExposureType.Chronic)]
        public void DesolvePbkModelCalculator_TestSingle(ExposureType exposureType) {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var substances = FakeSubstancesGenerator.Create(1);
            var substance = substances.First();
            var individual = FakeIndividualsGenerator.CreateSingle();

            var dose = 1;
            var route = ExposureRoute.Oral;
            var exposureUnit = ExposureUnitTriple.FromExposureUnit(ExternalExposureUnit.mgPerKgBWPerDay);
            var targetUnit = getDefaultInternalTarget();
            var externalExposure = ExternalIndividualDayExposure
                .FromSingleDose(
                    route,
                    substance,
                    dose,
                    exposureUnit,
                    individual
                );

            var instance = getDefaultInstance(substance);
            var simulationSettings = getDefaultSimulationSettings();
            simulationSettings.NumberOfSimulatedDays = 100;
            simulationSettings.NumberOfOralDosesPerDay = 1;

            var calculator = createCalculator(instance, simulationSettings) as DesolvePbkModelCalculator;
            var outputPath = CreateTestOutputPath($"TestSingle_{exposureType}");
            using (var logger = new FileLogger(Path.Combine(outputPath, "AnalysisCode.R"))) {
                calculator.CreateREngine = () => new LoggingRDotNetEngine(logger);
                calculator.DebugMode = true;
                var internalExposures = calculator
                    .Forward(
                        externalExposure,
                        route,
                        exposureUnit,
                        targetUnit,
                        exposureType,
                        random
                    );
                var targetExposurePattern = internalExposures as SubstanceTargetExposurePattern;
                var timePoints = simulationSettings.NumberOfSimulatedDays
                    * TimeUnit.Days.GetTimeUnitMultiplier(instance.KineticModelDefinition.Resolution)
                    * instance.KineticModelDefinition.EvaluationFrequency
                    + 1;
                Assert.AreEqual(timePoints, targetExposurePattern.TargetExposuresPerTimeUnit.Count);
            }
        }
    }
}
