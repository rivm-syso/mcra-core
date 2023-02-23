using MCRA.Utils.Statistics;
using MCRA.Simulation.Calculators.IntakeModelling;
using MCRA.Simulation.OutputGeneration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.DietaryExposures {
    /// <summary>
    /// OutputGeneration, ActionSummaries, DietaryExposures, IntakeModels, ISUF
    /// </summary>
    [TestClass]
    public class ISUFTransformedChartTests : ChartCreatorTestBase {
        /// <summary>
        /// Create charts ISUF power transformed, spline
        /// </summary>
        [TestMethod]
        public void PowerSplineTransformedDistributionChart_Test1() {
            var number = 1000;
            var error = NormalDistribution.NormalSamples(number, 0, 1).ToList();

            var isufDiagnostics = new List<IsufModelDiagnostics>();
            foreach (var item in error) {
                isufDiagnostics.Add(new IsufModelDiagnostics() {
                    Zhat = item,
                    GZ = 0d,
                });
            }
            var section = new ISUFModelResultsSection() {
                ISUFDiagnostics = isufDiagnostics,
                Power = .5,
            };

            var chart = new ISUFDistributionChartCreator(section);
            RenderChart(chart, $"TestCreate1");
        }
        /// <summary>
        /// Create charts ISUF identical transformed, spline
        /// </summary>
        [TestMethod]
        public void IdenticalSplineTransformedDistributionChart_Test1() {
            var number = 1000;
            var error = NormalDistribution.NormalSamples(number, 0, 1).ToList();

            var isufDiagnostics = new List<IsufModelDiagnostics>();
            foreach (var item in error) {
                isufDiagnostics.Add(new IsufModelDiagnostics() {
                    Zhat = item,
                    GZ = 0d,
                });
            }
            var iSUFModelResultsSection = new ISUFModelResultsSection() {
                ISUFDiagnostics = isufDiagnostics,
                Power = 1,
            };

            var chart = new ISUFDistributionChartCreator(iSUFModelResultsSection);
            RenderChart(chart, $"TestCreate2");
        }
        /// <summary>
        /// Create charts ISUF log transformed, spline
        /// </summary>
        [TestMethod]
        public void LogSplineTransformedDistributionChart_Test1() {
            var number = 1000;
            var error = NormalDistribution.NormalSamples(number, 0, 1).ToList();

            var isufDiagnostics = new List<IsufModelDiagnostics>();
            foreach (var item in error) {
                isufDiagnostics.Add(new IsufModelDiagnostics() {
                    Zhat = item,
                    GZ = 0d,
                });
            }
            var iSUFModelResultsSection = new ISUFModelResultsSection() {
                ISUFDiagnostics = isufDiagnostics,
                Power = 0,
            };

            var chart = new ISUFDistributionChartCreator(iSUFModelResultsSection);
            RenderChart(chart, $"TestCreate3");
        }

        /// <summary>
        /// Create charts ISUF power transformed, no spline
        /// </summary>
        [TestMethod]
        public void PowerTransformedDistributionChart_Test1() {
            var number = 1000;
            var error = NormalDistribution.NormalSamples(number, 0, 1).ToList();

            var isufDiagnostics = new List<IsufModelDiagnostics>();
            foreach (var item in error) {
                isufDiagnostics.Add(new IsufModelDiagnostics() {
                    Zhat = item,
                    GZ = double.NaN,
                });
            }
            var iSUFModelResultsSection = new ISUFModelResultsSection() {
                ISUFDiagnostics = isufDiagnostics,
                Power = .5,
            };

            var chart = new ISUFDistributionChartCreator(iSUFModelResultsSection);
            RenderChart(chart, $"TestCreate4");
        }
        /// <summary>
        /// Create charts ISUF identical transformed, no spline
        /// </summary>
        [TestMethod]
        public void IdenticalTransformedDistributionChart_Test1() {
            var number = 1000;
            var error = NormalDistribution.NormalSamples(number, 0, 1).ToList();

            var isufDiagnostics = new List<IsufModelDiagnostics>();
            foreach (var item in error) {
                isufDiagnostics.Add(new IsufModelDiagnostics() {
                    Zhat = item,
                    GZ = double.NaN,
                });
            }
            var iSUFModelResultsSection = new ISUFModelResultsSection() {
                ISUFDiagnostics = isufDiagnostics,
                Power = 1,
            };

            var chart = new ISUFDistributionChartCreator(iSUFModelResultsSection);
            RenderChart(chart, $"TestCreate5");
        }
        /// <summary>
        /// Create charts ISUF log transformed, no spline
        /// </summary>
        [TestMethod]
        public void LogTransformedDistributionChart_Test1() {
            var number = 1000;
            var error = NormalDistribution.NormalSamples(number, 0, 1).ToList();

            var isufDiagnostics = new List<IsufModelDiagnostics>();
            foreach (var item in error) {
                isufDiagnostics.Add(new IsufModelDiagnostics() {
                    Zhat = item,
                    GZ = double.NaN,
                });
            }
            var iSUFModelResultsSection = new ISUFModelResultsSection() {
                ISUFDiagnostics = isufDiagnostics,
                Power = 0,
            };

            var chart = new ISUFDistributionChartCreator(iSUFModelResultsSection);
            RenderChart(chart, $"TestCreate6");
        }
    }
}


