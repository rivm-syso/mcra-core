using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.KineticConversionCalculation;
using MCRA.Simulation.Calculators.PbkModelCalculation;
using MCRA.Simulation.Test.Helpers;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Test.UnitTests.Calculators.PbkModelCalculation {

    /// <summary>
    /// KineticModelCalculation calculator
    /// </summary>
    [TestClass]
    public abstract class PbkModelCalculatorTestsBase {

        private static readonly string _baseOutputPath = Path.Combine(TestUtilities.TestOutputPath, "PbkModelCalculators");

        protected abstract KineticModelInstance getDefaultInstance(params Compound[] substances);
        protected abstract PbkSimulationSettings getDefaultSimulationSettings();

        protected abstract PbkModelCalculatorBase createCalculator(
            KineticModelInstance instance,
            PbkSimulationSettings simulationSettings
        );

        protected abstract TargetUnit getDefaultInternalTarget();

        protected abstract TargetUnit getDefaultExternalTarget();

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
            var forwardCalculator = new PbkKineticConversionCalculator(calculator);

            var internalExposures = forwardCalculator.CalculateIndividualDayTargetExposures(
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
            Assert.HasCount(
                positiveExternalExposures.Count,
                positiveInternalExposures);

            var targetExposurePattern = positiveInternalExposures.First()
                .GetSubstanceTargetExposure(targetUnit.Target, substance) as SubstanceTargetExposurePattern;
            var timePoints = simulationSettings.NumberOfSimulatedDays
                    * TimeUnit.Days.GetTimeUnitMultiplier(instance.KineticModelDefinition.Resolution)
                    * instance.KineticModelDefinition.EvaluationFrequency
                    + 1;
            Assert.HasCount((int)timePoints, targetExposurePattern.TargetExposuresPerTimeUnit);
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
            var forwardCalculator = new PbkKineticConversionCalculator(calculator);

            var internalExposures = forwardCalculator.CalculateIndividualTargetExposures(
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
            Assert.HasCount(
                positiveExternalExposures.Count,
                positiveInternalExposures);

            var targetExposurePattern = positiveInternalExposures.First()
                .GetSubstanceTargetExposure(targetUnit.Target, substance) as SubstanceTargetExposurePattern;
            var timePoints = simulationSettings.NumberOfSimulatedDays
                * TimeUnit.Days.GetTimeUnitMultiplier(instance.KineticModelDefinition.Resolution)
                * instance.KineticModelDefinition.EvaluationFrequency
                + 1;
            Assert.HasCount((int)timePoints, targetExposurePattern.TargetExposuresPerTimeUnit);
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
