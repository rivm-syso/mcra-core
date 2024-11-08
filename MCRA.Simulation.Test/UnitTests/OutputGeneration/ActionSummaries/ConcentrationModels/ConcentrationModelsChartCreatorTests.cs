using MCRA.General;
using MCRA.Simulation.Calculators.CompoundResidueCollectionCalculation;
using MCRA.Simulation.Calculators.ConcentrationModelCalculation.ConcentrationModels;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.OutputGeneration.Helpers;
using MCRA.Utils;
using MCRA.Utils.Statistics;
using MCRA.Utils.Statistics.Histograms;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.ConcentrationModels {
    /// <summary>
    /// OutputGeneration, ActionSummaries, ConcentrationModels
    /// </summary>
    [TestClass]
    public class ConcentrationModelsChartCreatorTests : ChartCreatorTestBase {

        /// <summary>
        /// Create chart
        /// </summary>
        [TestMethod]
        public void ConcentrationModelsChartCreator_TestEmpirical() {
            var record = new ConcentrationModelRecord() {
                LogPositiveResiduesBins = fillBin(),
                FractionCensored = 0.98,
                FractionPositives = 0.02,
                FractionTrueZeros = 0.0,
                Model = ConcentrationModelType.Empirical,
            };
            var chart = new ConcentrationModelChartCreator(record, 120, 160, true);
            TestRender(chart, "Empirical1", ChartFileType.Png);
        }

        /// <summary>
        /// Create chart empirical
        /// </summary>
        [TestMethod]
        public void ConcentrationModelsChartCreator_TestEmpirical2() {
            var lors = new List<double>() { 0.05 };
            var bins = new List<HistogramBin> {
                new() { Frequency = 4, XMinValue = -4.7, XMaxValue = -4.29, },
                new() { Frequency = 7, XMinValue = -4.29, XMaxValue = -3.87, },
                new() { Frequency = 12, XMinValue = -3.87, XMaxValue = -3.46, },
                new() { Frequency = 7, XMinValue = -3.46, XMaxValue = -3.05, },
                new() { Frequency = 31, XMinValue = -3.05, XMaxValue = -2.63, },
                new() { Frequency = 27, XMinValue = -2.63, XMaxValue = -2.22, },
                new() { Frequency = 25, XMinValue = -2.22, XMaxValue = -1.8, },
                new() { Frequency = 20, XMinValue = -1.8, XMaxValue = -1.4, },
                new() { Frequency = 23, XMinValue = -1.4, XMaxValue = -0.98, },
                new() { Frequency = 20, XMinValue = -0.98, XMaxValue = -0.57, },
                new() { Frequency = 12, XMinValue = -0.57, XMaxValue = -0.15, },
                new() { Frequency = 10, XMinValue = -0.15, XMaxValue = 0.26, },
                new() { Frequency = 5, XMinValue = 0.26, XMaxValue = 0.67, },
                new() { Frequency = 5, XMinValue = 0.67, XMaxValue = 1.09, },
                new() { Frequency = 6, XMinValue = 1.09, XMaxValue = 1.5, }
            };

            var record = new ConcentrationModelRecord() {
                LogPositiveResiduesBins = bins,
                FractionCensored = .17,
                FractionPositives = .83,
                FractionTrueZeros = 0.0,
                Model = ConcentrationModelType.Empirical,
                Sigma = .61778,
                Mu = -4.0109,
                MaximumResidueLimit = 2,
                LORs = lors,
                CensoredValuesCount = 46,
                TotalMeasurementsCount = 240,
            };
            var chart = new ConcentrationModelDetectsMatrixViewChartCreator(record, 120, 160);
            TestRender(chart, "Empirical2", ChartFileType.Png);
        }

        /// <summary>
        /// Create chart empirical
        /// </summary>
        [TestMethod]
        public void ConcentrationModelsChartCreator_TestEmpirical3() {
            var bins = new List<HistogramBin> {
                new() { Frequency = 0, XMinValue = -2.99, XMaxValue = -1.99, },
                new() { Frequency = 0, XMinValue = -1.99, XMaxValue = -0.99, },
                new() { Frequency = 5, XMinValue = -0.99, XMaxValue = -0.49, }
            };
            var lors = new List<double>() { 0.05 };
            var record = new ConcentrationModelRecord() {
                LogPositiveResiduesBins = bins,
                FractionCensored = 0,
                FractionPositives = 1,
                FractionTrueZeros = 0,
                Model = ConcentrationModelType.Empirical,
                CensoredValuesCount = 0,
                LORs = lors,
            };
            var chart = new ConcentrationModelDetectsMatrixViewChartCreator(record, 120, 160);
            TestRender(chart, "TestEmpirical3", ChartFileType.Png);
        }

        /// <summary>
        /// Create chart empirical, test with no positives
        /// </summary>
        [TestMethod]
        public void ConcentrationModelsChartCreator_TestNoPositives() {
            var record = new ConcentrationModelRecord() {
                FractionCensored = 0.2,
                FractionTrueZeros = 0.8,
                Model = ConcentrationModelType.Empirical,
            };
            var chart = new ConcentrationModelChartCreator(record, 120, 160, true);
            TestRender(chart, "TestNoPositives", ChartFileType.Png);
        }

        /// <summary>
        /// Create chart lognormal, nondetects
        /// </summary>
        [TestMethod]
        public void ConcentrationModelsChartCreator_TestLogNormal() {
            for (int i = 0; i < 10; i++) {
                var mu = -1;
                var sigma = .6;
                var lors = new List<double>() { Math.Exp(-1.6) };
                var n = (i + 1) * 10;
                var bins = drawBins(i, mu, sigma, lors, n, out var nonDetectsCount);
                var fractionPositives = 1 - (nonDetectsCount / (double)n);
                var record = new ConcentrationModelRecord() {
                    LogPositiveResiduesBins = bins,
                    CensoredValuesCount = nonDetectsCount,
                    FractionCensored = .8 * (1 - fractionPositives),
                    FractionPositives = fractionPositives,
                    FractionTrueZeros = .2 * (1 - fractionPositives),
                    Model = ConcentrationModelType.LogNormal,
                    Sigma = .4,
                    Mu = mu,
                    LORs = lors,
                };

                var chart = new ConcentrationModelChartCreator(record, 120, 160, true);
                TestRender(chart, $"LogNormal_{i}", ChartFileType.Png);

                var detectsChartCreator = new ConcentrationModelDetectsMatrixViewChartCreator(record, 120, 160);
                TestRender(detectsChartCreator, $"LogNormal_Matrix_{i}", ChartFileType.Png);
            }
        }

        /// <summary>
        /// Create chart lognormal, all detects
        /// </summary>
        [TestMethod]
        public void ConcentrationModelsChartCreator_TestLogNormal1() {
            var lors = new List<double>() { };
            var record = new ConcentrationModelRecord() {
                LogPositiveResiduesBins = fillBin(),
                FractionCensored = .985,
                FractionPositives = .0141,
                FractionTrueZeros = 0.0,
                Model = ConcentrationModelType.LogNormal,
                Sigma = .61778,
                Mu = -4.0109,
                LORs = lors,
            };

            var chart = new ConcentrationModelChartCreator(record, 120, 160, true);
            TestRender(chart, $"TestLogNormal1", ChartFileType.Png);

            var detectsChartCreator = new ConcentrationModelDetectsMatrixViewChartCreator(record, 120, 160);
            TestRender(detectsChartCreator, $"LogNormal1_Detects", ChartFileType.Png);
        }

        /// <summary>
        /// Create chart lognormal, all detects
        /// </summary>
        [TestMethod]
        public void ConcentrationModelsChartCreator_TestLogNormal2() {
            var lors = new List<double>() { 0.05 };
            var bins = new List<HistogramBin> {
                new() { Frequency = 4, XMinValue = -4.7, XMaxValue = -4.29, },
                new() { Frequency = 7, XMinValue = -4.29, XMaxValue = -3.87, },
                new() { Frequency = 12, XMinValue = -3.87, XMaxValue = -3.46, },
                new() { Frequency = 7, XMinValue = -3.46, XMaxValue = -3.05, },
                new() { Frequency = 31, XMinValue = -3.05, XMaxValue = -2.63, },
                new() { Frequency = 27, XMinValue = -2.63, XMaxValue = -2.22, },
                new() { Frequency = 25, XMinValue = -2.22, XMaxValue = -1.8, },
                new() { Frequency = 20, XMinValue = -1.8, XMaxValue = -1.4, },
                new() { Frequency = 23, XMinValue = -1.4, XMaxValue = -0.98, },
                new() { Frequency = 20, XMinValue = -0.98, XMaxValue = -0.57, },
                new() { Frequency = 12, XMinValue = -0.57, XMaxValue = -0.15, },
                new() { Frequency = 10, XMinValue = -0.15, XMaxValue = 0.26, },
                new() { Frequency = 5, XMinValue = 0.26, XMaxValue = 0.67, },
                new() { Frequency = 5, XMinValue = 0.67, XMaxValue = 1.09, },
                new() { Frequency = 6, XMinValue = 1.09, XMaxValue = 1.5, }
            };

            var record = new ConcentrationModelRecord() {
                LogPositiveResiduesBins = bins,
                FractionCensored = .17,
                FractionPositives = .83,
                FractionTrueZeros = 0.0,
                Model = ConcentrationModelType.LogNormal,
                Sigma = 1.3,
                Mu = -1.77,
                MaximumResidueLimit = 2,
                LORs = lors,
                CensoredValuesCount = 46,
                TotalMeasurementsCount = 240,
            };

            var chart = new ConcentrationModelChartCreator(record, 120, 160, true);
            TestRender(chart, $"LogNormal2", ChartFileType.Png);

            var detectsChartCreator = new ConcentrationModelDetectsMatrixViewChartCreator(record, 120, 160);
            TestRender(detectsChartCreator, $"LogNormal2_Detects", ChartFileType.Png);
        }

        /// <summary>
        /// Create chart lognormal, all detects
        /// </summary>
        [TestMethod]
        public void ConcentrationModelsChartCreator_TestLogNormal3() {
            var lors = new List<double>() { 0.05 };
            var bins = new List<HistogramBin> {
                new() { Frequency = 4, XMinValue = -4.7, XMaxValue = -4.29, },
                new() { Frequency = 7, XMinValue = -4.29, XMaxValue = -3.87, },
                new() { Frequency = 12, XMinValue = -3.87, XMaxValue = -3.46, },
                new() { Frequency = 7, XMinValue = -3.46, XMaxValue = -3.05, },
                new() { Frequency = 31, XMinValue = -3.05, XMaxValue = -2.63, },
                new() { Frequency = 27, XMinValue = -2.63, XMaxValue = -2.22, },
                new() { Frequency = 25, XMinValue = -2.22, XMaxValue = -1.8, },
                new() { Frequency = 20, XMinValue = -1.8, XMaxValue = -1.4, },
                new() { Frequency = 23, XMinValue = -1.4, XMaxValue = -0.98, },
                new() { Frequency = 20, XMinValue = -0.98, XMaxValue = -0.57, },
                new() { Frequency = 12, XMinValue = -0.57, XMaxValue = -0.15, },
                new() { Frequency = 10, XMinValue = -0.15, XMaxValue = 0.26, },
                new() { Frequency = 5, XMinValue = 0.26, XMaxValue = 0.67, },
                new() { Frequency = 5, XMinValue = 0.67, XMaxValue = 1.09, },
                new() { Frequency = 6, XMinValue = 1.09, XMaxValue = 1.5, }
            };

            var record = new ConcentrationModelRecord() {
                LogPositiveResiduesBins = bins,
                FractionCensored = .17,
                FractionPositives = .83,
                FractionTrueZeros = 0.0,
                Model = ConcentrationModelType.LogNormal,
                Sigma = 1.3,
                Mu = -1.77,
                MaximumResidueLimit = 2,
                LORs = lors,
                CensoredValuesCount = 46,
                TotalMeasurementsCount = 240,
            };

            var chartCreator = new ConcentrationModelChartCreator(record, 120, 160, true);
            TestRender(chartCreator, $"LogNormal3", ChartFileType.Png);

            var detectsChartCreator = new ConcentrationModelDetectsMatrixViewChartCreator(record, 120, 160);
            TestRender(detectsChartCreator, $"LogNormal3_Detects", ChartFileType.Png);
        }

        /// <summary>
        /// Create chart, mrl
        /// </summary>
        [TestMethod]
        public void ConcentrationModelsChartCreator_TestMrl() {
            for (int i = 0; i < 10; i++) {
                var mu = -1;
                var sigma = .6;
                var lors = new List<double>() { Math.Exp(-1.6) };
                var n = (i + 1) * 10;
                var bins = drawBins(i, mu, sigma, lors, n, out var nonDetectsCount);
                var fractionPositives = 1 - (nonDetectsCount / (double)n);
                var record = new ConcentrationModelRecord() {
                    Model = ConcentrationModelType.MaximumResidueLimit,
                    LogPositiveResiduesBins = bins,
                    CensoredValuesCount = nonDetectsCount,
                    FractionCensored = .8 * (1 - fractionPositives),
                    FractionPositives = fractionPositives,
                    FractionTrueZeros = .2 * (1 - fractionPositives),
                    MaximumResidueLimit = 1,
                    FractionOfMrl = .8,
                    Sigma = .4,
                    Mu = mu,
                    LORs = lors,
                };

                var chart = new ConcentrationModelChartCreator(record, 120, 160, true);
                TestRender(chart, $"Mrl_{i}", ChartFileType.Png);

                var detectsChartCreator = new ConcentrationModelDetectsMatrixViewChartCreator(record, 120, 160);
                TestRender(detectsChartCreator, $"Mrl_Matrix_{i}", ChartFileType.Png);
            }
        }

        /// <summary>
        /// Create chart mrl
        /// </summary>
        [TestMethod]
        public void ConcentrationModelsChartCreator_TestMrl1() {
            var record = new ConcentrationModelRecord() {
                Model = ConcentrationModelType.MaximumResidueLimit,
                LogPositiveResiduesBins = fillBin(),
                FractionCensored = .985,
                FractionPositives = .0141,
                FractionTrueZeros = 0.0,
                FractionOfMrl = .5,
                Sigma = .61778,
                Mu = -4.0109,
                LORs = [],
                MaximumResidueLimit = 2,
            };

            var chart = new ConcentrationModelChartCreator(record, 120, 160, true);
            TestRender(chart, $"TestMrl1", ChartFileType.Png);

            var detectsChartCreator = new ConcentrationModelDetectsMatrixViewChartCreator(record, 120, 160);
            TestRender(detectsChartCreator, $"Mrl1_Detects", ChartFileType.Png);
        }

        /// <summary>
        /// Create chart mrl
        /// </summary>
        [TestMethod]
        public void ConcentrationModelsChartCreator_TestMrl2() {
            var record = new ConcentrationModelRecord() {
                Model = ConcentrationModelType.MaximumResidueLimit,
                FractionCensored = 1,
                FractionPositives = 0,
                FractionOfMrl = .5,
                MaximumResidueLimit = 2,
            };
            var chart = new ConcentrationModelChartCreator(record, 120, 160, true);
            TestRender(chart, $"Mrl2", ChartFileType.Png);
        }

        /// <summary>
        /// Create chart mrl
        /// </summary>
        [TestMethod]
        public void ConcentrationModelsChartCreator_TestMrl3() {
            var lors = new List<double>() { 0.05 };
            var record = new ConcentrationModelRecord() {
                Model = ConcentrationModelType.MaximumResidueLimit,
                FractionCensored = 1,
                FractionPositives = 0,
                FractionOfMrl = .5,
                MaximumResidueLimit = 2,
                CensoredValuesCount = 46,
                LORs = lors,
            };
            var chart = new ConcentrationModelChartCreator(record, 120, 160, false);
            TestRender(chart, $"Mrl3", ChartFileType.Png);
        }

        /// <summary>
        /// Test MRL concentration model chart creation.
        /// </summary>
        [TestMethod]
        public void ConcentrationModelsChartCreator_TestMrl4() {
            var bins = new List<HistogramBin> {
                new() { Frequency = 0, XMinValue = -2.99, XMaxValue = -1.99, },
                new() { Frequency = 0, XMinValue = -1.99, XMaxValue = -0.99, },
                new() { Frequency = 5, XMinValue = -0.99, XMaxValue = -0.49, }
            };
            var lors = new List<double>() { 0.05 };
            var record = new ConcentrationModelRecord() {
                Model = ConcentrationModelType.MaximumResidueLimit,
                LogPositiveResiduesBins = bins,
                FractionCensored = 0,
                FractionPositives = 1,
                FractionTrueZeros = 0,
                MaximumResidueLimit = 2,
                FractionOfMrl = .5,
                LORs = lors,
            };

            var chart = new ConcentrationModelChartCreator(record, 120, 160, false);
            TestRender(chart, $"Mrl4", ChartFileType.Png);

            var nonDetectsMatrix = new ConcentrationModelDetectsMatrixViewChartCreator(record, 120, 160);
            TestRender(nonDetectsMatrix, $"Mrl4_Matrix", ChartFileType.Png);
        }

        /// <summary>
        /// Test summary statistic concentration model chart creation.
        /// </summary>
        [TestMethod]
        public void ConcentrationModelsChartCreator_TestSummaryStatistic() {
            var record = new ConcentrationModelRecord() {
                FractionCensored = 1,
                FractionPositives = 0,
                Model = ConcentrationModelType.SummaryStatistics,
                Sigma = 1.3,
                Mu = -1.77
            };

            var chart = new ConcentrationModelChartCreator(record, 120, 160, true);
            TestRender(chart, $"SummaryStatistic", ChartFileType.Png);
        }

        /// <summary>
        /// Creates concentration models
        /// </summary>
        [TestMethod]
        public void ConcentrationModelsChartCreator_ConcentrationModels() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var n = 100;
            var lor = 0.2;
            var mu = Math.Log(.5);
            var sigma = 1;
            var positives = new List<double>();
            var nonDetects = new List<double>();
            for (int i = 0; i < n; i++) {
                var concentration = UtilityFunctions.ExpBound(mu + NormalDistribution.Draw(random, 0, 1) * sigma);
                if (concentration > lor) {
                    positives.Add(Math.Log(concentration));
                } else {
                    nonDetects.Add(Math.Log(lor));
                }
            }
            var fractionCensored = 1D * nonDetects.Count / n;
            var fractionPositives = 1D * positives.Count / n;
            var compoundResidueCollection = new CompoundResidueCollection() {
                CensoredValuesCollection = nonDetects.Select(c => new CensoredValue() { LOD = UtilityFunctions.ExpBound(c), LOQ = UtilityFunctions.ExpBound(c) }).ToList(),
                Positives = positives.Select(c => UtilityFunctions.ExpBound(c)).ToList(),
            };

            var cmE = new CMEmpirical() {
                Residues = compoundResidueCollection,
            };

            cmE.CalculateParameters();

            var lors = new List<double>() { lor };
            var bins = positives.MakeHistogramBins(10, positives.Min(), positives.Max());

            var record = new ConcentrationModelRecord() {
                LogPositiveResiduesBins = bins,
                FractionCensored = fractionCensored,
                FractionPositives = fractionPositives,
                FractionTrueZeros = 0.0,
                Model = ConcentrationModelType.Empirical,
                LORs = lors,
                CensoredValuesCount = nonDetects.Count,
                TotalMeasurementsCount = n,
            };
            var chart = new ConcentrationModelChartCreator(record, 150, 200, true);
            TestRender(chart, $"Empirical", ChartFileType.Png);

            var nonDetectSpikeLogNormal = new CMNonDetectSpikeLogNormal() {
                Residues = compoundResidueCollection,
            };
            nonDetectSpikeLogNormal.CalculateParameters();
            record.Model = ConcentrationModelType.NonDetectSpikeLogNormal;
            record.Mu = nonDetectSpikeLogNormal.Mu;
            record.Sigma = nonDetectSpikeLogNormal.Sigma;
            chart = new ConcentrationModelChartCreator(record, 150, 200, true);
            TestRender(chart, $"NDSpikeLogNormal", ChartFileType.Png);

            var censoredLogNormal = new CMCensoredLogNormal() {
                Residues = compoundResidueCollection,
            };
            censoredLogNormal.CalculateParameters();
            record.Model = ConcentrationModelType.CensoredLogNormal;
            record.Mu = censoredLogNormal.Mu;
            record.Sigma = censoredLogNormal.Sigma;
            chart = new ConcentrationModelChartCreator(record, 150, 200, true);
            TestRender(chart, $"CensoredLogNormal", ChartFileType.Png);

            var nonDetectSpikeTruncatedLogNormal = new CMNonDetectSpikeTruncatedLogNormal() {
                Residues = compoundResidueCollection,
            };
            nonDetectSpikeTruncatedLogNormal.CalculateParameters();
            record.Model = ConcentrationModelType.NonDetectSpikeTruncatedLogNormal;
            record.Mu = nonDetectSpikeTruncatedLogNormal.Mu;
            record.Sigma = nonDetectSpikeTruncatedLogNormal.Sigma;
            chart = new ConcentrationModelChartCreator(record, 150, 200, true);
            TestRender(chart, $"NDSpikeTruncatedLogNormal", ChartFileType.Png);

            var zeroSpikeCensoredLogNormal = new CMZeroSpikeCensoredLogNormal() {
                Residues = compoundResidueCollection,
            };
            zeroSpikeCensoredLogNormal.CalculateParameters();
            record.Model = ConcentrationModelType.ZeroSpikeCensoredLogNormal;
            record.Mu = zeroSpikeCensoredLogNormal.Mu;
            record.Sigma = zeroSpikeCensoredLogNormal.Sigma;
            record.FractionCensored = zeroSpikeCensoredLogNormal.FractionCensored;
            record.FractionPositives = zeroSpikeCensoredLogNormal.FractionPositives;
            record.FractionTrueZeros = zeroSpikeCensoredLogNormal.FractionTrueZeros;
            chart = new ConcentrationModelChartCreator(record, 150, 200, true);
            TestRender(chart, $"NDZeroSpikeCensoredLogNormal", ChartFileType.Png);
        }

        private List<HistogramBin> drawBins(int seed, double mu, double sigma, List<double> lors, int n, out int nonDetects) {
            var rnd = new McraRandomGenerator(seed);
            var logNormal = new LogNormalDistribution(mu, sigma);
            var samples = logNormal.Draws(rnd, n)
                .Select(r => Math.Log(r))
                .ToList();
            var detects = samples.Where(r => r > Math.Log(lors[rnd.Next(0, lors.Count)])).ToList();
            nonDetects = n - detects.Count;
            if (detects.Any()) {
                return detects.MakeHistogramBins(10, detects.Min(), detects.Max());
            }
            return [];
        }

        private List<HistogramBin> fillBin() {
            var bins = new List<HistogramBin> {
                new() { Frequency = 4, XMinValue = -4.605, XMaxValue = -4.07, },
                new() { Frequency = 1, XMinValue = -4.07, XMaxValue = -3.535, },
                new() { Frequency = 3, XMinValue = -3.535, XMaxValue = -3, }
            };
            return bins;
        }
    }
}
