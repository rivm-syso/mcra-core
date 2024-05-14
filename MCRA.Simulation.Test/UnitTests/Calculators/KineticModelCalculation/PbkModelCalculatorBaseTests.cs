using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.KineticModelCalculation.PbpkModelCalculation;
using MCRA.Simulation.Calculators.KineticModelCalculation.SbmlModelCalculation;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;
using MCRA.Utils.Test;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.Calculators.KineticModelCalculation {

    /// <summary>
    /// KineticModelCalculation calculator
    /// </summary>
    [TestClass]
    public abstract class PbkModelCalculatorBaseTests {

        private static readonly string _baseOutputPath = Path.Combine(TestUtilities.TestOutputPath, "PbkModelCalculators");

        protected abstract KineticModelInstance getDefaultInstance(params Compound[] substances);

        protected abstract PbkModelCalculatorBase createCalculator(KineticModelInstance instance);

        protected abstract TargetUnit getDefaultInternalTarget();

        protected abstract TargetUnit getDefaultExternalTarget();

        /// <summary>
        /// PBK model: calculates reverse dose based on PBK model.
        /// </summary>
        [TestMethod]
        [DataRow(ExposureType.Chronic)]
        [DataRow(ExposureType.Acute)]
        public void PbkModelCalculator_TestReverse(ExposureType exposureType) {
            int seed = 1;
            var random = new McraRandomGenerator(seed);
            var substances = MockSubstancesGenerator.Create(1);
            var substance = substances.First();
            var individual = MockIndividualsGenerator.CreateSingle();

            var internalDose = 10d;
            var internalDoseUnit = getDefaultInternalTarget();
            var externalExposuresUnit = getDefaultExternalTarget();

            var instance = getDefaultInstance(substance);
            var calculator = createCalculator(instance);

            var externalDose = calculator
                .Reverse(
                    individual,
                    internalDose,
                    internalDoseUnit,
                    ExposurePathType.Oral,
                    externalExposuresUnit.ExposureUnit,
                    exposureType,
                    random
                );

            var resultForward = calculator
                .Forward(
                    individual,
                    externalDose,
                    ExposurePathType.Oral,
                    externalExposuresUnit.ExposureUnit,
                    internalDoseUnit,
                    exposureType,
                    random
                );

            Assert.AreEqual(internalDose, resultForward, 1e-1);
        }

        [TestMethod]
        public abstract void TestForwardAcute(ExposureRoute exposureRoute);

        [TestMethod]
        public abstract void TestForwardChronic(ExposureRoute exposureRoute);

        protected void testForwardAcute(
            ExposureRoute exposureRoute
        ) {
            var random = new McraRandomGenerator(1);
            var substances = MockSubstancesGenerator.Create(1);
            var substance = substances.First();
            var routes = new List<ExposurePathType>() { exposureRoute.GetExposurePath() };
            var individuals = MockIndividualsGenerator.Create(5, 2, random, useSamplingWeights: true);
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var individualDayExposures = MockExternalExposureGenerator
                .CreateExternalIndividualDayExposures(individualDays, substances, routes, seed: 1);
            var targetUnit = getDefaultInternalTarget();

            var instance = getDefaultInstance(substance);
            var calculator = createCalculator(instance);

            var internalExposures = calculator.CalculateIndividualDayTargetExposures(
                individualDayExposures,
                routes,
                ExposureUnitTriple.FromExposureUnit(ExternalExposureUnit.mgPerKgBWPerDay),
                new List<TargetUnit> { targetUnit },
                new ProgressState(),
                random
            );

            var positiveExternalExposures = individualDayExposures
                .Where(r => r.ExposuresPerRouteSubstance
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
            Assert.AreEqual(instance.NumberOfDays * 24 + 1, targetExposurePattern.TargetExposuresPerTimeUnit.Count);
        }

        protected void testForwardChronic(
            ExposureRoute exposureRoute
        ) {
            var random = new McraRandomGenerator(1);
            var substances = MockSubstancesGenerator.Create(1);
            var substance = substances.First();
            var routes = new List<ExposurePathType>() { exposureRoute.GetExposurePath() };
            var individuals = MockIndividualsGenerator.Create(5, 2, random, useSamplingWeights: true);
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var individualExposures = MockExternalExposureGenerator
                .CreateExternalIndividualExposures(individualDays, substances, routes, seed: 1);
            var targetUnit = getDefaultInternalTarget();

            var instance = getDefaultInstance(substance);
            var calculator = createCalculator(instance);

            var internalExposures = calculator.CalculateIndividualTargetExposures(
                individualExposures,
                routes,
                ExposureUnitTriple.FromExposureUnit(ExternalExposureUnit.mgPerKgBWPerDay),
                new List<TargetUnit> { targetUnit },
                new ProgressState(),
                random
            );

            var positiveExternalExposures = individualExposures
                .Where(r => r.ExposuresPerRouteSubstance
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
            Assert.AreEqual(instance.NumberOfDays * 24 + 1, targetExposurePattern.TargetExposuresPerTimeUnit.Count);
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
