using MCRA.Utils.Statistics;
using MCRA.Utils.Statistics.Histograms;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.ComponentCalculation.DriverSubstanceCalculation;
using MCRA.Simulation.OutputGeneration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.ExposureMixtures {
    /// <summary>
    /// OutputGeneration, ActionSummaries, ExposureMixtures
    /// </summary>
    [TestClass]
    public class DriverCompoundsChartTests : ChartCreatorTestBase {
        /// <summary>
        /// Create charts and test MaximumCumulativeRatioSection view
        /// </summary>
        [TestMethod]
        public void DriverCompoundsChart_Test1() {
            var seed = 1;
            var rnd = new McraRandomGenerator(seed);
            var nSubst = 5;
            var substA = new Compound() { Code = "aa", Name = "AA" };
            var substB = new Compound() { Code = "bb", Name = "BB" };
            var substC = new Compound() { Code = "cc", Name = "CC" };
            var substD = new Compound() { Code = "dd", Name = "DD" };
            var substE = new Compound() { Code = "ee", Name = "EE" };
            var substF = new Compound() { Code = "ff", Name = "FF" };
            var drivers = new List<DriverSubstance>();
            for (int i = 0; i < 100; i++) {
                var mcrA = rnd.NextDouble() * nSubst + 1;
                var mcrB = rnd.NextDouble() * nSubst + 1;
                var mcrC = rnd.NextDouble() * nSubst + 1;
                var mcrD = rnd.NextDouble() * nSubst + 1;
                var mcrE = rnd.NextDouble() * nSubst + 1;
                var mcrF = rnd.NextDouble() * nSubst + 1;
                drivers.Add(new DriverSubstance() { Compound = substA, MaximumCumulativeRatio = mcrA, CumulativeExposure = 1 / mcrA * rnd.NextDouble() });
                drivers.Add(new DriverSubstance() { Compound = substB, MaximumCumulativeRatio = mcrB, CumulativeExposure = 1 / mcrB * rnd.NextDouble() });
                drivers.Add(new DriverSubstance() { Compound = substC, MaximumCumulativeRatio = mcrC, CumulativeExposure = 1 / mcrC * rnd.NextDouble() });
                drivers.Add(new DriverSubstance() { Compound = substD, MaximumCumulativeRatio = mcrD, CumulativeExposure = 1 / mcrD * rnd.NextDouble() });
                drivers.Add(new DriverSubstance() { Compound = substE, MaximumCumulativeRatio = mcrE, CumulativeExposure = 1 / mcrE * rnd.NextDouble() });
                drivers.Add(new DriverSubstance() { Compound = substF, MaximumCumulativeRatio = mcrF, CumulativeExposure = 1 / mcrF * rnd.NextDouble() });
            }
            var driverCompoundStatisticsRecords = drivers.GroupBy(gr => gr.Compound)
                .Select(g => {
                    var logTotalExposure = g.Select(c => Math.Log(c.CumulativeExposure)).ToList();
                    var logRatio = g.Select(c => Math.Log(c.MaximumCumulativeRatio)).ToList();
                    var bivariate = getBivariateParameters(logTotalExposure, logRatio);
                    return new DriverCompoundStatisticsRecord {
                        CompoundName = g.Key.Name,
                        CompoundCode = g.Key.Code,
                        CumulativeExposureMedian = Math.Exp(bivariate[0]),
                        CVCumulativeExposure = bivariate[2],
                        RatioMedian = Math.Exp(bivariate[1]),
                        CVRatio = bivariate[3],
                        R = bivariate[4],
                        Number = logTotalExposure.Count,
                    };
                })
                .OrderBy(c => c.CumulativeExposureMedian)
                .ToList();

            var driverCompounds = new List<DriverCompoundRecord>();
            foreach (var item in drivers) {
                driverCompounds.Add(new DriverCompoundRecord() {
                    CompoundCode = item.Compound.Code,
                    CompoundName = item.Compound.Name,
                    Ratio = item.MaximumCumulativeRatio,
                    CumulativeExposure = item.CumulativeExposure,
                });
            }

            var section = new MaximumCumulativeRatioSection {
                DriverCompounds = driverCompounds,
                TargetUnit = TargetUnit.FromExternalExposureUnit(ExposureUnit.mgPerKgBWPerDay),
                RatioCutOff = 0,
                CumulativeExposureCutOffPercentage = 0,
                Percentiles = new double[] { 80, 90, 99 },
                DriverCompoundStatisticsRecords = driverCompoundStatisticsRecords,
            };

            var chart = new DriverCompoundsChartCreator(section);
            RenderChart(chart, $"TestCreate");
            AssertIsValidView(section); ;
        }


        /// <summary>
        /// Create charts and test MaximumCumulativeRatioSection view
        /// </summary>
        [TestMethod]
        public void DriverSubstancesChart_Test1() {
            var seed = 1;
            var rnd = new McraRandomGenerator(seed);
            var nSubst = 5;
            var substA = new Compound() { Code = "aa", Name = "AA" };
            var substB = new Compound() { Code = "bb", Name = "BB" };
            var substC = new Compound() { Code = "cc", Name = "CC" };
            var substD = new Compound() { Code = "dd", Name = "DD" };
            var substE = new Compound() { Code = "ee", Name = "EE" };
            var substF = new Compound() { Code = "ff", Name = "FF" };
            var drivers = new List<DriverSubstance>();
            for (int i = 0; i < 100; i++) {
                var mcrA = rnd.NextDouble() * nSubst + 1;
                var mcrB = rnd.NextDouble() * nSubst + 1;
                var mcrC = rnd.NextDouble() * nSubst + 1;
                var mcrD = rnd.NextDouble() * nSubst + 1;
                var mcrE = rnd.NextDouble() * nSubst + 1;
                var mcrF = rnd.NextDouble() * nSubst + 1;
                drivers.Add(new DriverSubstance() { Compound = substA, MaximumCumulativeRatio = mcrA, CumulativeExposure = 1 / mcrA * rnd.NextDouble() });
                drivers.Add(new DriverSubstance() { Compound = substB, MaximumCumulativeRatio = mcrB, CumulativeExposure = 1 / mcrB * rnd.NextDouble() });
                drivers.Add(new DriverSubstance() { Compound = substC, MaximumCumulativeRatio = mcrC, CumulativeExposure = 1 / mcrC * rnd.NextDouble() });
                drivers.Add(new DriverSubstance() { Compound = substD, MaximumCumulativeRatio = mcrD, CumulativeExposure = 1 / mcrD * rnd.NextDouble() });
                drivers.Add(new DriverSubstance() { Compound = substE, MaximumCumulativeRatio = mcrE, CumulativeExposure = 1 / mcrE * rnd.NextDouble() });
                drivers.Add(new DriverSubstance() { Compound = substF, MaximumCumulativeRatio = mcrF, CumulativeExposure = 1 / mcrF * rnd.NextDouble() });
            }

            var driverCompoundStatisticsRecords = drivers.GroupBy(gr => gr.Compound)
                .Select(g => {
                    var logTotalExposure = g.Select(c => Math.Log(c.CumulativeExposure)).ToList();
                    var logRatio = g.Select(c => Math.Log(c.MaximumCumulativeRatio)).ToList();
                    var bivariate = getBivariateParameters(logTotalExposure, logRatio);
                    return new DriverCompoundStatisticsRecord {
                        CompoundName = g.Key.Name,
                        CompoundCode = g.Key.Code,
                        CumulativeExposureMedian = Math.Exp(bivariate[0]),
                        CVCumulativeExposure = bivariate[2],
                        RatioMedian = Math.Exp(bivariate[1]),
                        CVRatio = bivariate[3],
                        R = bivariate[4],
                        Number = logTotalExposure.Count,
                    };
                })
                .OrderBy(c => c.CumulativeExposureMedian)
                .ToList();

            var driverCompounds = new List<DriverCompoundRecord>();
            foreach (var item in drivers) {
                driverCompounds.Add(new DriverCompoundRecord() {
                    CompoundCode = item.Compound.Code,
                    CompoundName = item.Compound.Name,
                    Ratio = item.MaximumCumulativeRatio,
                    CumulativeExposure = item.CumulativeExposure,
                });
            }

            var section = new MaximumCumulativeRatioSection {
                DriverCompounds = driverCompounds,
                TargetUnit = TargetUnit.FromExternalExposureUnit(ExposureUnit.mgPerKgBWPerDay),
                RatioCutOff = 2,
                CumulativeExposureCutOffPercentage = 0,
                Percentiles = new double[] { 50, 90, 99 },
                MinimumPercentage = 17,
                DriverCompoundStatisticsRecords = driverCompoundStatisticsRecords,
            };

            var chart = new DriverSubstancesChartCreator(section);
            RenderChart(chart, $"TestCreateNewMCRPlot");
            AssertIsValidView(section); ;
        }

        private List<double> getBivariateParameters(List<double> x, List<double> y) {
            var x_muX = new List<double>();
            var y_muY = new List<double>();

            var bivariate = new List<double>(5);
            var muX = x.Average();
            var muY = y.Average(); ;
            var sdX = Math.Sqrt(x.Variance());
            var sdY = Math.Sqrt(y.Variance());

            for (int i = 0; i < x.Count; i++) {
                x_muX.Add(x[i] - muX);
                y_muY.Add(y[i] - muY);
            }

            var sumXY = 0d;
            var sumX = 0d;
            var sumY = 0d;
            for (int i = 0; i < x.Count; i++) {
                sumXY += x_muX[i] * y_muY[i];
                sumX += x_muX[i] * x_muX[i];
                sumY += y_muY[i] * y_muY[i];
            }
            var correlation = sumXY / Math.Sqrt(sumX * sumY);
            bivariate.Add(muX);
            bivariate.Add(muY);
            bivariate.Add(sdX);
            bivariate.Add(sdY);
            bivariate.Add(correlation);
            return bivariate;
        }
    }
}
