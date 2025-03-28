using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.KineticModelCalculation.PbpkModelCalculation;
using MCRA.Simulation.Test.Helpers;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.Calculators.KineticModelCalculation.PbkModelCalculation {

    /// <summary>
    /// KineticModelCalculation calculator
    /// </summary>
    [TestClass]
    public abstract class PbkModelCalculatorBaseTests {

        private static readonly string _baseOutputPath = Path.Combine(TestUtilities.TestOutputPath, "PbkModelCalculators");

        protected abstract KineticModelInstance getDefaultInstance(params Compound[] substances);
        protected abstract PbkSimulationSettings getDefaultSimulationSettings();

        protected abstract PbkModelCalculatorBase createCalculator(
            KineticModelInstance instance,
            PbkSimulationSettings simulationSettings
        );

        protected abstract TargetUnit getDefaultInternalTarget();

        protected abstract TargetUnit getDefaultExternalTarget();

        /// <summary>
        /// PBK model: calculates reverse dose based on PBK model.
        /// </summary>
        [TestMethod]
        [DataRow(ExposureType.Chronic)]
        [DataRow(ExposureType.Acute)]
        public void PbkModelCalculator_TestReverse(ExposureType exposureType) {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var substances = FakeSubstancesGenerator.Create(1);
            var substance = substances.First();
            var individual = FakeIndividualsGenerator.CreateSingle();

            var internalDose = 10d;
            var internalDoseUnit = getDefaultInternalTarget();
            var externalExposuresUnit = getDefaultExternalTarget();

            var instance = getDefaultInstance(substance);
            var simulationSettings = getDefaultSimulationSettings();
            simulationSettings.PrecisionReverseDoseCalculation = 0.05;
            var calculator = createCalculator(instance, simulationSettings);

            var externalDose = calculator
                .Reverse(
                    individual,
                    internalDose,
                    internalDoseUnit,
                    ExposureRoute.Oral,
                    externalExposuresUnit.ExposureUnit,
                    exposureType,
                    random
                );

            var resultForward = calculator
                .Forward(
                    individual,
                    externalDose,
                    ExposureRoute.Oral,
                    externalExposuresUnit.ExposureUnit,
                    internalDoseUnit,
                    exposureType,
                    random
                );

            Assert.AreEqual(internalDose, resultForward, 1e-1);
        }

        [TestMethod]
        public virtual void TestForwardAcute(ExposureRoute route) {
        }

        [TestMethod]
        public virtual void TestForwardChronic(ExposureRoute route) {
        }

        protected void testForwardAcute(
            ExposureRoute route
        ) {
            var random = new McraRandomGenerator(1);
            var substances = FakeSubstancesGenerator.Create(1);
            var substance = substances.First();
            var routes = new[] { route };
            var paths = FakeExposurePathGenerator.Create([.. routes]);
            var individuals = FakeIndividualsGenerator.Create(5, 2, random, useSamplingWeights: true);
            var individualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var individualDayExposures = FakeExternalExposureGenerator
                .CreateExternalIndividualDayExposures(individualDays, substances, paths, seed: 1);
            var targetUnit = getDefaultInternalTarget();

            var instance = getDefaultInstance(substance);
            var simulationSettings = getDefaultSimulationSettings();
            var calculator = createCalculator(instance, simulationSettings);

            var internalExposures = calculator.CalculateIndividualDayTargetExposures(
                individualDayExposures,
                routes,
                ExposureUnitTriple.FromExposureUnit(ExternalExposureUnit.mgPerKgBWPerDay),
                [targetUnit],
                new ProgressState(),
                random
            );

            var positiveExternalExposures = individualDayExposures
                .Where(r => r.ExposuresPerPath
                .Any(eprc => eprc.Value.Any(ipc => ipc.Amount > 0)))
                .ToList();
            var positiveInternalExposures = internalExposures
                .Where(r => r.IsPositiveTargetExposure(targetUnit.Target))
                .ToList();
            Assert.AreEqual(
                positiveExternalExposures.Count,
                positiveInternalExposures.Count
            );

            var targetExposurePattern = positiveInternalExposures.First()
                .GetSubstanceTargetExposure(targetUnit.Target, substance) as SubstanceTargetExposurePattern;
            var timePoints = simulationSettings.NumberOfSimulatedDays
                    * TimeUnit.Days.GetTimeUnitMultiplier(instance.KineticModelDefinition.TimeScale)
                    * instance.KineticModelDefinition.EvaluationFrequency
                    + 1;
            Assert.AreEqual(timePoints, targetExposurePattern.TargetExposuresPerTimeUnit.Count);
        }

        protected void testForwardChronic(
            ExposureRoute route
        ) {
            var random = new McraRandomGenerator(1);
            var substances = FakeSubstancesGenerator.Create(1);
            var substance = substances.First();
            var routes = new[] { route };
            var paths = FakeExposurePathGenerator.Create(routes);
            var individuals = FakeIndividualsGenerator.Create(5, 2, random, useSamplingWeights: true);
            var individualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var individualExposures = FakeExternalExposureGenerator
                .CreateExternalIndividualExposures(individualDays, substances, paths, seed: 1);
            var targetUnit = getDefaultInternalTarget();

            var instance = getDefaultInstance(substance);
            var simulationSettings = getDefaultSimulationSettings();
            var calculator = createCalculator(instance, simulationSettings);

            var internalExposures = calculator.CalculateIndividualTargetExposures(
                individualExposures,
                routes,
                ExposureUnitTriple.FromExposureUnit(ExternalExposureUnit.mgPerKgBWPerDay),
                [targetUnit],
                new ProgressState(),
                random
            );

            var positiveExternalExposures = individualExposures
                .Where(r => r.ExposuresPerPath
                .Any(eprc => eprc.Value.Any(ipc => ipc.Amount > 0)))
                .ToList();
            var positiveInternalExposures = internalExposures
                .Where(r => r.IsPositiveTargetExposure(targetUnit.Target))
                .ToList();
            Assert.AreEqual(
                positiveExternalExposures.Count,
                positiveInternalExposures.Count
            );

            var targetExposurePattern = positiveInternalExposures.First()
                .GetSubstanceTargetExposure(targetUnit.Target, substance) as SubstanceTargetExposurePattern;
            var timePoints = simulationSettings.NumberOfSimulatedDays
                * TimeUnit.Days.GetTimeUnitMultiplier(instance.KineticModelDefinition.TimeScale)
                * instance.KineticModelDefinition.EvaluationFrequency
                + 1;
            Assert.AreEqual(timePoints, targetExposurePattern.TargetExposuresPerTimeUnit.Count);
        }

        public string CreateTestOutputPath(string testName) {
            var outputPath = Path.Combine(_baseOutputPath, GetType().Name, testName);
            if (!Directory.Exists(outputPath)) {
                Directory.CreateDirectory(outputPath);
            }
            return outputPath;
        }
    }
}
