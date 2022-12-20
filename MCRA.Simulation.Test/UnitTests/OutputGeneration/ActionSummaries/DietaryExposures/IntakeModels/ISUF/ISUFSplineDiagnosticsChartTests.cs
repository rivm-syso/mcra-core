using MCRA.Utils.Statistics;
using MCRA.Simulation.Calculators.IntakeModelling;
using MCRA.Simulation.OutputGeneration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.DietaryExposures {
    /// <summary>
    /// OutputGeneration, ActionSummaries, DietaryExposures, IntakeModels, ISUF
    /// </summary>
    [TestClass]
    public class ISUFSplineDiagnosticsChartTests : ChartCreatorTestBase {
        /// <summary>
        /// Create chart ISUF power
        /// </summary>
        [TestMethod]
        public void ISUFPowerSplineDiagnosticsChart_Test1() {
            var number = 1000;
            var error = NormalDistribution.NormalSamples(number, 0, 1).ToList();
            var e = NormalDistribution.NormalSamples(number, 0, .1).ToList();

            var isufDiagnostics = new List<IsufModelDiagnostics>();
            var counter = 0;
            foreach (var item in error) {
                isufDiagnostics.Add(new IsufModelDiagnostics() {
                    GZ = item,
                    Z = item + e[counter],
                    TransformedDailyIntakes = item + item,
                });
                counter++;
            }
            var section = new ISUFModelResultsSection() {
                ISUFDiagnostics = isufDiagnostics,
                Power = .5,
            };

            var chart = new ISUFSplineDiagnosticsChartCreator(section);
            RenderChart(chart, $"TestCreate1");
        }
        /// <summary>
        ///  Creates chart ISUF identical
        /// </summary>
        [TestMethod]
        public void ISUFIdenticalSplineDiagnosticsChart_Test1() {
            var number = 1000;
            var error = NormalDistribution.NormalSamples(number, 0, 1).ToList();
            var e = NormalDistribution.NormalSamples(number, 0, .1).ToList();

            var isufDiagnostics = new List<IsufModelDiagnostics>();
            var counter = 0;
            foreach (var item in error) {
                isufDiagnostics.Add(new IsufModelDiagnostics() {
                    GZ = item,
                    Z = item + e[counter],
                    TransformedDailyIntakes = item + item,
                });
                counter++;
            }
            var section = new ISUFModelResultsSection() {
                ISUFDiagnostics = isufDiagnostics,
                Power = 1,
            };

            var chart = new ISUFSplineDiagnosticsChartCreator(section);
            RenderChart(chart, $"TestCreate2");
        }
        /// <summary>
        ///  Creates chart ISUF log
        /// </summary>
        [TestMethod]
        public void ISUFLogSplineDiagnosticsChart_Test1() {
            var number = 1000;
            var error = NormalDistribution.NormalSamples(number, 0, 1).ToList();
            var e = NormalDistribution.NormalSamples(number, 0, .1).ToList();

            var isufDiagnostics = new List<IsufModelDiagnostics>();
            var counter = 0;
            foreach (var item in error) {
                isufDiagnostics.Add(new IsufModelDiagnostics() {
                    GZ = item,
                    Z = item + e[counter],
                    TransformedDailyIntakes = item + item,
                });
                counter++;
            }
            var section = new ISUFModelResultsSection() {
                ISUFDiagnostics = isufDiagnostics,
                Power = 0,
            };

            var chart = new ISUFSplineDiagnosticsChartCreator(section);
            RenderChart(chart, $"TestCreate3");
        }
    }
}


