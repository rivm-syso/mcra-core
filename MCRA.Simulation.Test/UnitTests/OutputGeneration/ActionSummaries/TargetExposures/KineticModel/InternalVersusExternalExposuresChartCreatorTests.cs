using MCRA.General;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.Collections;
using MCRA.Utils.Statistics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.TargetExposures {

    /// <summary>
    /// OutputGeneration, ActionSummaries, TargetExposures, KineticModel
    /// </summary>
    [TestClass]
    public class InternalVersusExternalExposuresChartCreatorTests : ChartCreatorTestBase {

        [TestMethod]
        public void InternalVersusExternalExposuresChartCreator_TestCreate() {
            var seed = 1;
            var rnd = new McraRandomGenerator(seed);
            var configs = new int[] { 1, 1000 };
            var exposureUnit = ExposureUnitTriple.FromExposureUnit(ExternalExposureUnit.mgPerKgBWPerDay);
            var targetUnit = TargetUnit.FromInternalDoseUnit(DoseUnit.ugPerL, BiologicalMatrix.Liver);
            foreach (var n in configs) {
                var records = createFake(rnd, n, targetUnit);
                var section = new InternalVersusExternalExposuresSection() {
                    Records = records,
                    ExternalExposureUnit = exposureUnit
                };
                var chart = new InternalVersusExternalExposuresScatterChartCreator(
                    section,
                    targetUnit
                );
                RenderChart(chart, $"TestCreate");
            }
        }

        [TestMethod]
        public void InternalVersusExternalExposuresChartCreator_TestCreateNaNs() {
            var seed = 1;
            var rnd = new McraRandomGenerator(seed);
            var n = 10000;
            var exposureUnit = ExposureUnitTriple.FromExposureUnit(ExternalExposureUnit.mgPerKgBWPerDay);
            var targetUnit = TargetUnit.FromInternalDoseUnit(DoseUnit.ugPerL, BiologicalMatrix.Liver);
            var records = createFake(rnd, n, targetUnit);
            records.ForEach(r => r.ExternalExposure = double.NaN);
            var section = new InternalVersusExternalExposuresSection() {
                Records = records,
                ExternalExposureUnit = exposureUnit
            };
            var chart = new InternalVersusExternalExposuresScatterChartCreator(
                section,
                targetUnit
            );
            RenderChart(chart, $"TestCreateNaNs");
        }

        [TestMethod]
        public void InternalVersusExternalExposuresSection_TestCreateEmptyList() {
            var seed = 1;
            var rnd = new McraRandomGenerator(seed);
            var n = 0;
            var exposureUnit = ExposureUnitTriple.FromExposureUnit(ExternalExposureUnit.mgPerKgBWPerDay);
            var targetUnit = TargetUnit.FromInternalDoseUnit(DoseUnit.ugPerL, BiologicalMatrix.Liver);
            var records = createFake(rnd, n, targetUnit);
            var section = new InternalVersusExternalExposuresSection() {
                Records = records,
                ExternalExposureUnit = exposureUnit
            };
            var chart = new InternalVersusExternalExposuresScatterChartCreator(
                section,
                targetUnit
            );
            RenderChart(chart, $"TestCreateEmptyList");
        }

        private List<InternalVersusExternalExposureRecord> createFake(
            IRandom random,
            int n,
            TargetUnit targetUnit
        ) {
            var externalExposures = LogNormalDistribution.Samples(random, 5, 1, n);
            var internalExposures = externalExposures
                .Select(r => r / 10 + NormalDistribution.Draw(random, 0, r / 100))
                .ToList();
            var result = externalExposures
                .Zip(internalExposures)
                .Select(r => {
                    var internalExposures = new SerializableDictionary<ExposureTarget, double>();
                    internalExposures.Add(targetUnit.Target, r.Second);
                    var record = new InternalVersusExternalExposureRecord() {
                        ExternalExposure = r.First,
                        TargetExposure = internalExposures
                    };
                    return record;
                })
                .ToList();
            return result;
        }
    }
}
