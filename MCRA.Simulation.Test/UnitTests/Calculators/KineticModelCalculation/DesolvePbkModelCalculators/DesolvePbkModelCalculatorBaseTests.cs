using MCRA.General;
using MCRA.Simulation.Calculators.KineticModelCalculation.DesolvePbkModelCalculators;
using MCRA.Simulation.Calculators.KineticModelCalculation.DesolvePbkModelCalculators.CosmosKineticModelCalculation;
using MCRA.Simulation.Calculators.KineticModelCalculation.PbpkModelCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using MCRA.Utils.Logger;
using MCRA.Utils.R.REngines;
using MCRA.Utils.Statistics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.Calculators.KineticModelCalculation.DesolvePbkModelCalculators {

    /// <summary>
    /// KineticModelCalculation calculator
    /// </summary>
    [TestClass]
    public abstract class DesolvePbkModelCalculatorBaseTests : PbkModelCalculatorBaseTests {

        [TestMethod]
        [DataRow(ExposureType.Acute)]
        [DataRow(ExposureType.Chronic)]
        public void DesolvePbkModelCalculator_TestSingle(ExposureType exposureType) {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var substances = MockSubstancesGenerator.Create(1);
            var substance = substances.First();
            var individual = MockIndividualsGenerator.CreateSingle();

            var dose = 1;
            var exposureRoute = ExposurePathType.Oral;
            var exposureUnit = ExposureUnitTriple.FromExposureUnit(ExternalExposureUnit.mgPerKgBWPerDay);
            var targetUnit = getDefaultInternalTarget();
            var externalExposure = ExternalIndividualDayExposure
                .FromSingleDose(
                    exposureRoute,
                    substance,
                    dose,
                    exposureUnit,
                    individual
                );

            var instance = getDefaultInstance(substance);
            instance.NumberOfDays = 100;
            instance.NumberOfDosesPerDay = 1;

            var calculator = createCalculator(instance) as DesolvePbkModelCalculator;
            var outputPath = CreateTestOutputPath($"TestSingle_{exposureType}");
            using (var logger = new FileLogger(Path.Combine(outputPath, "AnalysisCode.R"))) {
                calculator.CreateREngine = () => new LoggingRDotNetEngine(logger);
                calculator.DebugMode = true;
                var internalExposures = calculator
                    .Forward(
                        externalExposure,
                        exposureRoute,
                        exposureUnit,
                        targetUnit,
                        exposureType,
                        random
                    );
                var targetExposurePattern = internalExposures as SubstanceTargetExposurePattern;
                var timePoints = instance.NumberOfDays
                    * TimeUnit.Days.GetTimeUnitMultiplier(instance.KineticModelDefinition.TimeScale)
                    * instance.KineticModelDefinition.EvaluationFrequency
                    + 1;
                Assert.AreEqual(timePoints, targetExposurePattern.TargetExposuresPerTimeUnit.Count);
            }
        }
    }
}
