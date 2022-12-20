using MCRA.Utils.Statistics;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.TargetExposures {
    /// <summary>
    /// OutputGeneration, ActionSummaries, TargetExposures, KineticModel
    /// </summary>
    [TestClass]
    public class KineticModelChartCreatorTests : ChartCreatorTestBase {

        private static string outputPath;

        [ClassInitialize]
        public static void Init(TestContext testContext) {
            outputPath = TestResourceUtilities.CreateTestOutputPath("KineticModelChartCreatorTests");
        }
        /// <summary>
        /// Create chart acute
        /// </summary>
        [TestMethod]
        public void KineticModelChartCreator_TestCreateAcute() {
            var seed = 1;
            var rnd = new McraRandomGenerator(seed);
            var configs = new int[] { 0, 1000 };
            foreach (var n in configs) {
                var externalExposures = LogNormalDistribution.Samples(rnd, 5, 1, n);
                var internalExposures = externalExposures.Select(r => r / 10 + NormalDistribution.Draw(rnd, 0, r / 100)).ToList();
                var ratio = n > 0 ? internalExposures.Average() / externalExposures.Average() : double.NaN;
                var section = new KineticModelSection() {
                    ExternalExposures = externalExposures,
                    PeakTargetExposures = internalExposures,
                    ConcentrationRatioPeak = ratio,
                    IsAcute = true,
                };
                var chart = new KineticModelChartCreator(section, "mg/kg", "mg/kg bw/day");
                RenderChart(chart, $"TestCreate1");
            }
        }
        /// <summary>
        /// Create chart acute only NaNs
        /// </summary>
        [TestMethod]
        public void KineticModelChartCreator_TestCreateAcuteNaNs() {
            var seed = 1;
            var rnd = new McraRandomGenerator(seed);
            var n = 10000;
            var externalExposures = LogNormalDistribution.Samples(rnd, 5, 1, n);
            var internalExposures = externalExposures.Select(r => double.NaN).ToList();
            var section = new KineticModelSection() {
                ExternalExposures = externalExposures,
                PeakTargetExposures = internalExposures,
                IsAcute = true,
            };
            var chart = new KineticModelChartCreator(section, "mg/kg", "mg/kg bw/day");
            RenderChart(chart, $"TestCreate2");
        }
        /// <summary>
        /// Create chart acute empty list
        /// </summary>
        [TestMethod]
        public void KineticModelChartCreator_TestCreateAcuteEmptyList() {
            var seed = 1;
            var rnd = new McraRandomGenerator(seed);
            var n = 10000;
            var externalExposures = LogNormalDistribution.Samples(rnd, 5, 1, n);
            var section = new KineticModelSection() {
                ExternalExposures = externalExposures,
                PeakTargetExposures = new List<double>(),
                IsAcute = true,
            };
            var chart = new KineticModelChartCreator(section, "mg/kg", "mg/kg bw/day");
            RenderChart(chart, $"TestCreate3");
        }
        /// <summary>
        /// Create chart chronic
        /// </summary>
        [TestMethod]
        public void KineticModelChartCreator_TestCreateChronic() {
            var seed = 1;
            var rnd = new McraRandomGenerator(seed);
            var configs = new int[] { 0, 1000 };
            foreach (var n in configs) {
                var externalExposures = LogNormalDistribution.Samples(rnd, 5, 1, n);
                var internalExposures = externalExposures.Select(r => r / 10 + NormalDistribution.Draw(rnd, 0, r / 100)).ToList();
                var ratio = n > 0 ? internalExposures.Average() / externalExposures.Average() : double.NaN;
                var section = new KineticModelSection() {
                    ExternalExposures = externalExposures,
                    SteadyStateTargetExposures = internalExposures,
                    ConcentrationRatioAverage = ratio,
                    IsAcute = false,
                };
                var chart = new KineticModelChartCreator(section, "mg/kg", "mg/kg bw/day");
                RenderChart(chart, $"TestCreate4");
            }
        }
        /// <summary>
        /// Create chart chronic only NaNs
        /// </summary>
        [TestMethod]
        public void KineticModelChartCreator_TestCreateChronicNaNs() {
            var seed = 1;
            var rnd = new McraRandomGenerator(seed);
            var n = 10000;
            var externalExposures = LogNormalDistribution.Samples(rnd, 5, 1, n);
            var internalExposures = externalExposures.Select(r => double.NaN).ToList();
            var section = new KineticModelSection() {
                ExternalExposures = externalExposures,
                SteadyStateTargetExposures = internalExposures,
                IsAcute = false,
            };
            var chart = new KineticModelChartCreator(section, "mg/kg", "mg/kg bw/day");
            RenderChart(chart, $"TestCreate5");
        }
        /// <summary>
        /// Create chart chronic empty lists
        /// </summary>
        [TestMethod]
        public void KineticModelChartCreator_TestCreateChronicEmptyList() {
            var seed = 1;
            var rnd = new McraRandomGenerator(seed);
            var n = 10000;
            var externalExposures = LogNormalDistribution.Samples(rnd, 5, 1, n);
            var section = new KineticModelSection() {
                ExternalExposures = externalExposures,
                SteadyStateTargetExposures = new List<double>(),
                IsAcute = false,
            };
            var chart = new KineticModelChartCreator(section, "mg/kg", "mg/kg bw/day");
            RenderChart(chart, $"TestCreate6");
        }
    }
}
