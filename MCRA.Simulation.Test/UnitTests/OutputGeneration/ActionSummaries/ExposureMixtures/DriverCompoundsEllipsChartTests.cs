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
    public class DriverCompoundsEllipsChartTests : ChartCreatorTestBase {
        /// <summary>
        /// Create charts and test MaximumCumulativeRatioSection view
        /// </summary>
        [TestMethod]
        public void DriverCompoundsChart_Test1() {
            var drivers = new List<DriverSubstance>();
            var compounds = new List<Compound> {
                new Compound() {
                    Code = "aa",
                    Name = "AA"
                },
                new Compound() {
                    Code = "bb",
                    Name = "BB"
                },
                new Compound() {
                    Code = "cc",
                    Name = "CC"
                },
                new Compound() {
                    Code = "dd",
                    Name = "DD"
                }
            };
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var ix = 0;
            var mu = 1.1;
            for (int i = 0; i < 1000; i++) {
                if (i < 250) {
                    ix = 0;
                    mu = 2;
                } else if (i < 500) {
                    ix = 1;
                    mu = 3;
                } else if (i < 750) {
                    ix = 2;
                    mu = 4;
                } else {
                    ix = 3;
                    mu = 5;
                }

                var u = Math.Abs(NormalDistribution.InvCDF(3, 1, random.NextDouble()));
                var mcr = Math.Abs(NormalDistribution.InvCDF(u + mu, 1, random.NextDouble()));
                var ce = Math.Abs(Math.Exp(NormalDistribution.InvCDF(u + mu, 1, random.NextDouble())));
                drivers.Add(new DriverSubstance() { Compound = compounds[ix], MaximumCumulativeRatio = mcr, CumulativeExposure = ce });
            }

            var driverCompoundStatisticsRecords = drivers.GroupBy(gr => gr.Compound)
                .Select(g => {
                    var logTotalExposure = g.Select(c => Math.Log(c.CumulativeExposure)).ToList();
                    var logRatio = g.Select(c => (c.MaximumCumulativeRatio)).ToList();
                    var bivariate = getBivariateParameters(logTotalExposure, logRatio);
                    return new DriverCompoundStatisticsRecord {
                        CompoundName = g.Key.Name,
                        CompoundCode = g.Key.Code,
                        CumulativeExposureMedian = Math.Exp(bivariate[0]),
                        CVCumulativeExposure = bivariate[2],
                        RatioMedian = (bivariate[1]),
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
                TargetUnit = new TargetUnit(ExposureUnit.mgPerKgBWPerDay),
                RatioCutOff = 0,
                CumulativeExposureCutOffPercentage = 0,
                DriverCompoundStatisticsRecords = driverCompoundStatisticsRecords,
            };

            var chart = new DriverCompoundsEllipsChartCreator(section);
            RenderChart(chart, $"TestCreate");
            var chart1 = new DriverCompoundsChartCreator(section);
            RenderChart(chart1, $"TestCreate");
            var chart2 = new DriverCompoundsEllipsChartCreator(section);
            RenderChart(chart2, $"TestCreate");
            var chart3 = new DriverCompoundsChartCreator(section);
            RenderChart(chart3, $"TestCreate");
            AssertIsValidView(section);
        }

        /// <summary>
        /// x, y is normal after log transformation (logscale)
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
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
