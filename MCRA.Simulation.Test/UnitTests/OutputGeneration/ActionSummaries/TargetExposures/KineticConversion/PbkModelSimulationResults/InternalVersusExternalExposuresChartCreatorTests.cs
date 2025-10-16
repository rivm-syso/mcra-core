using MCRA.General;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.Collections;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.TargetExposures {

    [TestClass]
    public class InternalVersusExternalExposuresChartCreatorTests : ChartCreatorTestBase {

        [TestMethod]
        public void InternalVersusExternalExposuresScatterChartCreator_TestCreate() {
            var seed = 1;
            var rnd = new McraRandomGenerator(seed);
            var configs = new int[] { 1, 1000 };
            var exposureUnit = ExposureUnitTriple.FromExposureUnit(ExternalExposureUnit.mgPerKgBWPerDay);
            var targetUnit = TargetUnit.FromInternalDoseUnit(DoseUnit.ugPerL, BiologicalMatrix.Liver);
            foreach (var n in configs) {
                (var externalExposures, var targetExposures) = createFake(rnd, n, targetUnit);
                var section = new InternalVersusExternalExposuresSection() {
                    TotalExternalIndividualExposures = externalExposures,
                    TargetIndividualExposures = targetExposures,
                    ExternalExposureUnit = exposureUnit
                };
                var chart = new InternalVersusExternalExposuresScatterChartCreator(
                    section,
                    targetUnit
                );
                RenderChart(chart, $"TestCreate_{n}");
            }
        }

        [TestMethod]
        public void InternalVersusExternalExposuresScatterChartCreator_TestCreateNaNs() {
            var seed = 1;
            var rnd = new McraRandomGenerator(seed);
            var n = 10000;
            var exposureUnit = ExposureUnitTriple.FromExposureUnit(ExternalExposureUnit.mgPerKgBWPerDay);
            var targetUnit = TargetUnit.FromInternalDoseUnit(DoseUnit.ugPerL, BiologicalMatrix.Liver);
            var records = createFake(rnd, n, targetUnit);
            (var externalExposures, var targetExposures) = createFake(rnd, n, targetUnit);
            externalExposures = externalExposures.Select(r => double.NaN).ToArray();
            var section = new InternalVersusExternalExposuresSection() {
                TotalExternalIndividualExposures = externalExposures,
                TargetIndividualExposures = targetExposures,
                ExternalExposureUnit = exposureUnit
            };
            var chart = new InternalVersusExternalExposuresScatterChartCreator(
                section,
                targetUnit
            );
            RenderChart(chart, $"TestCreate_NaNs");
        }

        [TestMethod]
        public void InternalVersusExternalExposuresScatterChartCreator_TestCreateEmptyList() {
            var seed = 1;
            var rnd = new McraRandomGenerator(seed);
            var n = 0;
            var exposureUnit = ExposureUnitTriple.FromExposureUnit(ExternalExposureUnit.mgPerKgBWPerDay);
            var targetUnit = TargetUnit.FromInternalDoseUnit(DoseUnit.ugPerL, BiologicalMatrix.Liver);
            (var externalExposures, var targetExposures) = createFake(rnd, n, targetUnit);
            var section = new InternalVersusExternalExposuresSection() {
                TotalExternalIndividualExposures = externalExposures,
                TargetIndividualExposures = targetExposures,
                ExternalExposureUnit = exposureUnit
            };
            var chart = new InternalVersusExternalExposuresScatterChartCreator(
                section,
                targetUnit
            );
            RenderChart(chart, $"TestCreate_EmptyList");
        }

        private (double[], SerializableDictionary<ExposureTarget, double[]>) createFake(
            IRandom random,
            int n,
            TargetUnit targetUnit
        ) {
            var externalExposures = LogNormalDistribution.Samples(random, 5, 1, n);
            var internalExposures = new SerializableDictionary<ExposureTarget, double[]>();
            internalExposures[targetUnit.Target] = externalExposures
                .Select(r => r / 10 + NormalDistribution.Draw(random, 0, r / 100))
                .ToArray();
            return (externalExposures.ToArray(), internalExposures);
        }
    }
}
