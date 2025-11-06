using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.ComponentCalculation.DriverSubstanceCalculation;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.Statistics;
using MCRA.Utils.Statistics.Histograms;

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
            var substA = new Compound() { Code = "aa" };
            var substB = new Compound() { Code = "bb" };
            var substC = new Compound() { Code = "cc" };
            var substD = new Compound() { Code = "dd" };
            var substE = new Compound() { Code = "ee" };
            var substF = new Compound() { Code = "ff" };
            var drivers = new List<DriverSubstance>();
            for (int i = 0; i < 100; i++) {
                var mcrA = rnd.NextDouble() * nSubst + 1;
                var mcrB = rnd.NextDouble() * nSubst + 1;
                var mcrC = rnd.NextDouble() * nSubst + 1;
                var mcrD = rnd.NextDouble() * nSubst + 1;
                var mcrE = rnd.NextDouble() * nSubst + 1;
                var mcrF = rnd.NextDouble() * nSubst + 1;
                drivers.Add(new DriverSubstance() { Substance = substA, MaximumCumulativeRatio = mcrA, CumulativeExposure = 1 / mcrA * rnd.NextDouble() });
                drivers.Add(new DriverSubstance() { Substance = substB, MaximumCumulativeRatio = mcrB, CumulativeExposure = 1 / mcrB * rnd.NextDouble() });
                drivers.Add(new DriverSubstance() { Substance = substC, MaximumCumulativeRatio = mcrC, CumulativeExposure = 1 / mcrC * rnd.NextDouble() });
                drivers.Add(new DriverSubstance() { Substance = substD, MaximumCumulativeRatio = mcrD, CumulativeExposure = 1 / mcrD * rnd.NextDouble() });
                drivers.Add(new DriverSubstance() { Substance = substE, MaximumCumulativeRatio = mcrE, CumulativeExposure = 1 / mcrE * rnd.NextDouble() });
                drivers.Add(new DriverSubstance() { Substance = substF, MaximumCumulativeRatio = mcrF, CumulativeExposure = 1 / mcrF * rnd.NextDouble() });
            }
            var driverCompoundStatisticsRecords = drivers.GroupBy(gr => gr.Substance)
                .Select(g => {
                    var logTotalExposure = g.Select(c => Math.Log(c.CumulativeExposure)).ToList();
                    var logRatio = g.Select(c => Math.Log(c.MaximumCumulativeRatio)).ToList();
                    var bivariate = getBivariateParameters(logTotalExposure, logRatio);
                    return new DriverSubstanceStatisticsRecord {
                        SubstanceName = g.Key.Name,
                        SubstanceCode = g.Key.Code,
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

            var driverCompounds = new List<DriverSubstanceRecord>();
            foreach (var item in drivers) {
                driverCompounds.Add(new DriverSubstanceRecord() {
                    SubstanceCode = item.Substance.Code,
                    SubstanceName = item.Substance.Name,
                    Ratio = item.MaximumCumulativeRatio,
                    CumulativeExposure = item.CumulativeExposure,
                    Target = ""
                });
            }

            var section = new MaximumCumulativeRatioSection {
                DriverSubstanceTargets = driverCompounds,
                RatioCutOff = 0,
                CumulativeExposureCutOffPercentage = 0,
                Percentiles = [80, 90, 99],
                DriverSubstanceTargetStatisticsRecords = driverCompoundStatisticsRecords,
                TargetUnit = TargetUnit.FromInternalDoseUnit(DoseUnit.mgPerL, BiologicalMatrix.Blood)
            };

            var chart = new DriverSubstancesChartCreator(section);
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
            var substA = new Compound() { Code = "aa" };
            var substB = new Compound() { Code = "bb" };
            var substC = new Compound() { Code = "cc" };
            var substD = new Compound() { Code = "dd" };
            var substE = new Compound() { Code = "ee" };
            var substF = new Compound() { Code = "ff" };
            var drivers = new List<DriverSubstance>();
            for (int i = 0; i < 100; i++) {
                var mcrA = rnd.NextDouble() * nSubst + 1;
                var mcrB = rnd.NextDouble() * nSubst + 1;
                var mcrC = rnd.NextDouble() * nSubst + 1;
                var mcrD = rnd.NextDouble() * nSubst + 1;
                var mcrE = rnd.NextDouble() * nSubst + 1;
                var mcrF = rnd.NextDouble() * nSubst + 1;
                drivers.Add(new DriverSubstance() { Substance = substA, MaximumCumulativeRatio = mcrA, CumulativeExposure = 1 / mcrA * rnd.NextDouble() });
                drivers.Add(new DriverSubstance() { Substance = substB, MaximumCumulativeRatio = mcrB, CumulativeExposure = 1 / mcrB * rnd.NextDouble() });
                drivers.Add(new DriverSubstance() { Substance = substC, MaximumCumulativeRatio = mcrC, CumulativeExposure = 1 / mcrC * rnd.NextDouble() });
                drivers.Add(new DriverSubstance() { Substance = substD, MaximumCumulativeRatio = mcrD, CumulativeExposure = 1 / mcrD * rnd.NextDouble() });
                drivers.Add(new DriverSubstance() { Substance = substE, MaximumCumulativeRatio = mcrE, CumulativeExposure = 1 / mcrE * rnd.NextDouble() });
                drivers.Add(new DriverSubstance() { Substance = substF, MaximumCumulativeRatio = mcrF, CumulativeExposure = 1 / mcrF * rnd.NextDouble() });
            }

            var driverCompoundStatisticsRecords = drivers.GroupBy(gr => gr.Substance)
                .Select(g => {
                    var logTotalExposure = g.Select(c => Math.Log(c.CumulativeExposure)).ToList();
                    var logRatio = g.Select(c => Math.Log(c.MaximumCumulativeRatio)).ToList();
                    var bivariate = getBivariateParameters(logTotalExposure, logRatio);
                    return new DriverSubstanceStatisticsRecord {
                        SubstanceName = g.Key.Name,
                        SubstanceCode = g.Key.Code,
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

            var driverCompounds = new List<DriverSubstanceRecord>();
            foreach (var item in drivers) {
                driverCompounds.Add(new DriverSubstanceRecord() {
                    SubstanceCode = item.Substance.Code,
                    SubstanceName = item.Substance.Name,
                    Ratio = item.MaximumCumulativeRatio,
                    CumulativeExposure = item.CumulativeExposure,
                });
            }

            var section = new MaximumCumulativeRatioSection {
                DriverSubstanceTargets = driverCompounds,
                RatioCutOff = 2,
                CumulativeExposureCutOffPercentage = 0,
                Percentiles = [50, 90, 99],
                MinimumPercentage = 17,
                DriverSubstanceTargetStatisticsRecords = driverCompoundStatisticsRecords,
                TargetUnit = TargetUnit.FromInternalDoseUnit(DoseUnit.mgPerL, BiologicalMatrix.Blood)
            };

            var chart = new DriverSubstancesChartCreator(section);
            RenderChart(chart, $"TestCreateNewMCRPlot");
            AssertIsValidView(section); ;
        }


        /// <summary>
        /// Create charts: HI
        /// </summary>
        [TestMethod]
        public void MCR_HI_Test1() {
            var seed = 1;
            var rnd = new McraRandomGenerator(seed);
            var substances = new List<Compound> {
                new() { Code = "A" },
                new() { Code = "B" },
                new() { Code = "C" },
                new() { Code = "D" },
                new() { Code = "E" },
                new() { Code = "F" },
            };
            var drivers = new List<DriverSubstance>();
            var rfD = 50d;
            var lognormal = new LogNormalDistribution(-.2, 2);
            for (int i = 0; i < 100; i++) {
                var exposure = new List<double>();
                for (int ii = 0; ii < substances.Count - 2; ii++) {
                    exposure.Add(lognormal.Draw(rnd) * 1 * (i + 1) / rfD);
                }
                //drivers
                for (int ii = 0; ii < 2; ii++) {
                    exposure.Add(lognormal.Draw(rnd) * 3 * (i + 1) / rfD);
                }
                var cumulative = exposure.Sum();
                var maximum = exposure.Max();
                var index = exposure.FindIndex(a => a == maximum);
                drivers.Add(new DriverSubstance() { Substance = substances[index], MaximumCumulativeRatio = cumulative / maximum, CumulativeExposure = cumulative });
            }

            var driverSubstances = new List<DriverSubstanceRecord>();
            foreach (var item in drivers) {
                driverSubstances.Add(new DriverSubstanceRecord() {
                    SubstanceCode = item.Substance.Code,
                    SubstanceName = item.Substance.Name,
                    Ratio = item.MaximumCumulativeRatio,
                    CumulativeExposure = item.CumulativeExposure,
                });
            }
            var section = new RiskMaximumCumulativeRatioSection {
                DriverSubstanceTargets = driverSubstances,
                RiskMetricType = RiskMetricType.ExposureHazardRatio,
                Threshold = 1,
            };

            var chart = new MCRChartCreator(section);
            RenderChart(chart, $"TestCreateMCR_HI");
        }

        /// <summary>
        /// Create charts: MOE
        /// </summary>
        [TestMethod]
        public void MCR_MOE_Test1() {
            var seed = 1;
            var rnd = new McraRandomGenerator(seed);
            var substances = new List<Compound> {
                new() { Code = "A" },
                new() { Code = "B" },
                new() { Code = "C" },
                new() { Code = "D" },
                new() { Code = "E" },
                new() { Code = "F" },
            };
            var drivers = new List<DriverSubstance>();
            var rfD = 5d;
            var lognormal = new LogNormalDistribution(-.2, 2);
            for (int i = 0; i < 100; i++) {
                var exposure = new List<double>();
                for (int ii = 0; ii < substances.Count - 2; ii++) {
                    exposure.Add(rfD / (lognormal.Draw(rnd) * 1 * (i + 1)));
                }
                //drivers
                for (int ii = 0; ii < 2; ii++) {
                    exposure.Add(rfD / (lognormal.Draw(rnd) * 3 * (i + 1)));
                }
                var cumulative = exposure.Sum();
                var maximum = exposure.Max();
                var index = exposure.FindIndex(a => a == maximum);
                drivers.Add(new DriverSubstance() { Substance = substances[index], MaximumCumulativeRatio = cumulative / maximum, CumulativeExposure = cumulative });
            }
            var driverSubstances = new List<DriverSubstanceRecord>();
            foreach (var item in drivers) {
                driverSubstances.Add(new DriverSubstanceRecord() {
                    SubstanceCode = item.Substance.Code,
                    SubstanceName = item.Substance.Name,
                    Ratio = item.MaximumCumulativeRatio,
                    CumulativeExposure = item.CumulativeExposure,
                });
            }
            var section = new RiskMaximumCumulativeRatioSection {
                DriverSubstanceTargets = driverSubstances,
                RiskMetricType = RiskMetricType.HazardExposureRatio,
                Threshold = 1,
            };

            var chart = new MCRChartCreator(section);
            RenderChart(chart, $"TestCreateMCR_MOE");
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
