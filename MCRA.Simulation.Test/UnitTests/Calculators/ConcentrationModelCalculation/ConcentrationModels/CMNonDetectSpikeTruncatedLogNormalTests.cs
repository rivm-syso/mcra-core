using MCRA.General;
using MCRA.Simulation.Calculators.CompoundResidueCollectionCalculation;
using MCRA.Simulation.Calculators.ConcentrationModelCalculation.ConcentrationModels;
using MCRA.Utils;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Test.UnitTests.Calculators.ConcentrationModelCalculation.ConcentrationModels {
    /// <summary>
    /// ResidueGeneration calculator
    /// </summary>
    [TestClass]
    public class CMNonDetectSpikeTruncatedLogNormalTests {
        /// <summary>
        /// NonDetect spike truncated logNormal
        /// </summary>
        [TestMethod]
        [TestCategory("Concentration Modeling Tests")]
        public void CMNonDetectSpikeTruncatedLogNormalTest1() {
            var residues = new CompoundResidueCollection() {
                Positives = [],
                CensoredValuesCollection= [],
            };

            var concentrationModel = new CMNonDetectSpikeTruncatedLogNormal() {
                DesiredModelType = ConcentrationModelType.NonDetectSpikeTruncatedLogNormal,
                NonDetectsHandlingMethod = NonDetectsHandlingMethod.ReplaceByLOR,
                FractionOfLor = 1,
                Residues = residues,
            };

            Assert.IsFalse(concentrationModel.CalculateParameters());
        }
        /// <summary>
        /// NonDetect spike truncated logNormal
        /// </summary>
        [TestMethod]
        [TestCategory("Concentration Modeling Tests")]
        public void CMNonDetectSpikeTruncatedLogNormalTest2() {
            var lor = 0.1;

            var residues = new CompoundResidueCollection() {
                Positives = [],
                CensoredValuesCollection = [new CensoredValue() { LOD = lor, LOQ = lor }]
            };

            var concentrationModel = new CMNonDetectSpikeTruncatedLogNormal() {
                DesiredModelType = ConcentrationModelType.NonDetectSpikeLogNormal,
                NonDetectsHandlingMethod = NonDetectsHandlingMethod.ReplaceByLOR,
                FractionOfLor = 1,
                Residues = residues,
            };

            Assert.IsFalse(concentrationModel.CalculateParameters());
        }

        /// <summary>
        /// NonDetect spike truncated logNormal
        /// </summary>
        [TestMethod]
        [TestCategory("Concentration Modeling Tests")]
        public void CMNonDetectSpikeTruncatedLogNormalTest3() {
            var lor = 0.1;
            var positives = new List<double>();
            var residues = new CompoundResidueCollection() {
                Positives = positives,
                CensoredValuesCollection = [new CensoredValue() { LOD = lor, LOQ = lor }]
            };

            var concentrationModel = new CMNonDetectSpikeTruncatedLogNormal() {
                DesiredModelType = ConcentrationModelType.NonDetectSpikeLogNormal,
                NonDetectsHandlingMethod = NonDetectsHandlingMethod.ReplaceByLOR,
                FractionOfLor = 1,
                Residues = residues,
            };

            Assert.IsFalse(concentrationModel.CalculateParameters());
        }

        /// <summary>
        /// Censored value spike truncated lognormal.
        /// </summary>
        [TestMethod]
        [TestCategory("Concentration Modeling Tests")]
        public void CMNonDetectSpikeTruncatedLogNormalTest4() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var lor = 0.1;
            var mu = Math.Log(10);
            var sigma = .1;
            var rnd = new McraRandomGenerator(seed);
            var residues = new CompoundResidueCollection() {
                Positives = LogNormalDistribution.Samples(rnd, mu, sigma, 100),
                CensoredValuesCollection = Enumerable.Repeat(lor, 100).Select(c => new CensoredValue() { LOD = lor, LOQ = lor }).ToList(),
            };

            var concentrationModel = new CMNonDetectSpikeTruncatedLogNormal() {
                DesiredModelType = ConcentrationModelType.NonDetectSpikeLogNormal,
                NonDetectsHandlingMethod = NonDetectsHandlingMethod.ReplaceByLOR,
                FractionOfLor = 1,
                Residues = residues,
            };

            concentrationModel.CalculateParameters();
            var repetitions = 1000;

            //==========================================
            // DrawFromDistribution
            //==========================================

            var generatedResidues = new List<double>(repetitions);
            for (int i = 0; i < repetitions; i++) {
                generatedResidues.Add(concentrationModel.DrawFromDistribution(random, concentrationModel.NonDetectsHandlingMethod));
            }

            // Test positives
            var generatedPositives = generatedResidues.Where(s => s > 0 && s != lor).ToList();
            var pObservedPositives = (double)generatedPositives.Count / repetitions;
            var pExpectedPositives = 0.5;
            var sigmaPositives = Math.Sqrt((pExpectedPositives * (1 - pExpectedPositives)) / repetitions);
            Assert.IsTrue(pObservedPositives > pExpectedPositives - 1.96 * sigma);
            Assert.IsTrue(pObservedPositives < pExpectedPositives + 1.96 * sigma);

            // Test lors
            var generatedLors = generatedResidues.Where(s => s == lor).ToList();
            var pObservedLors = (double)generatedLors.Count / repetitions;
            var pExpectedLors = 0.5;
            var sigmaLors = Math.Sqrt((pExpectedLors * (1 - pExpectedLors)) / repetitions);
            Assert.IsTrue(pObservedLors > pExpectedLors - 1.96 * sigma);
            Assert.IsTrue(pObservedLors < pExpectedLors + 1.96 * sigma);

            //==========================================
            // DrawFromCensoredOrPositives
            //==========================================

            generatedResidues = new List<double>(repetitions);
            for (int i = 0; i < repetitions; i++) {
                generatedResidues.Add(concentrationModel.DrawFromDistributionExceptZeroes(random, concentrationModel.NonDetectsHandlingMethod));
            }

            // Test positives
            generatedPositives = generatedResidues.Where(s => s > 0 && s != lor).ToList();
            pObservedPositives = (double)generatedPositives.Count / repetitions;
            pExpectedPositives = 0.5;
            sigmaPositives = Math.Sqrt((pExpectedPositives * (1 - pExpectedPositives)) / repetitions);
            Assert.IsTrue(pObservedPositives > pExpectedPositives - 1.96 * sigma);
            Assert.IsTrue(pObservedPositives < pExpectedPositives + 1.96 * sigma);

            // Test lors
            generatedLors = generatedResidues.Where(s => s == lor).ToList();
            pObservedLors = (double)generatedLors.Count / repetitions;
            pExpectedLors = 0.5;
            sigmaLors = Math.Sqrt((pExpectedLors * (1 - pExpectedLors)) / repetitions);
            Assert.IsTrue(pObservedLors > pExpectedLors - 1.96 * sigma);
            Assert.IsTrue(pObservedLors < pExpectedLors + 1.96 * sigma);

            //==========================================
            // Other
            //==========================================
            Assert.AreEqual(lor, concentrationModel.DrawAccordingToNonDetectsHandlingMethod(random, concentrationModel.NonDetectsHandlingMethod, 1D));
            Assert.AreEqual(0.5 * UtilityFunctions.ExpBound(SpecialFunctions.MeanLeftTruncatedNormal(UtilityFunctions.LogBound(lor), concentrationModel.Mu, concentrationModel.Sigma)) + 0.5 * lor, concentrationModel.GetDistributionMean(concentrationModel.NonDetectsHandlingMethod));

            Assert.AreEqual(0.5, concentrationModel.FractionPositives);
            Assert.AreEqual(0D, concentrationModel.FractionTrueZeros);
            Assert.AreEqual(0.5, concentrationModel.FractionCensored);
        }

        /// <summary>
        /// NonDetect spike truncated logNormal
        /// </summary>
        [TestMethod]
        [TestCategory("Concentration Modeling Tests")]
        public void CMNonDetectSpikeTruncatedLogNormalTest5() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var lor = 0.1;
            var mu = Math.Log(10);
            var sigma = .1;

            var rnd = new McraRandomGenerator(seed);
            var residues = new CompoundResidueCollection() {
                Positives = LogNormalDistribution.Samples(rnd, mu, sigma, 100),
                CensoredValuesCollection = Enumerable.Repeat(lor, 100).Select(c => new CensoredValue() { LOD = c, LOQ = c }).ToList(),
            };

            var concentrationModel = new CMNonDetectSpikeTruncatedLogNormal() {
                DesiredModelType = ConcentrationModelType.NonDetectSpikeLogNormal,
                NonDetectsHandlingMethod = NonDetectsHandlingMethod.ReplaceByLOR,
                FractionOfLor = 1,
                CorrectedWeightedAgriculturalUseFraction = 0.5,
                Residues = residues,
            };

            concentrationModel.CalculateParameters();
            var repetitions = 1000;

            //==========================================
            // DrawFromDistribution
            //==========================================

            var generatedResidues = new List<double>(repetitions);
            for (int i = 0; i < repetitions; i++) {
                generatedResidues.Add(concentrationModel.DrawFromDistribution(random, concentrationModel.NonDetectsHandlingMethod));
            }

            // Test positives
            var generatedPositives = generatedResidues.Where(s => s > 0 && s != lor).ToList();
            var pObservedPositives = (double)generatedPositives.Count / repetitions;
            var pExpectedPositives = 0.5;
            var sigmaPositives = Math.Sqrt((pExpectedPositives * (1 - pExpectedPositives)) / repetitions);
            Assert.IsTrue(pObservedPositives > pExpectedPositives - 1.96 * sigma);
            Assert.IsTrue(pObservedPositives < pExpectedPositives + 1.96 * sigma);

            // Test lors
            var generatedLors = generatedResidues.Where(s => s == 0).ToList();
            var pObservedLors = (double)generatedLors.Count / repetitions;
            var pExpectedLors = 0.5;
            var sigmaLors = Math.Sqrt((pExpectedLors * (1 - pExpectedLors)) / repetitions);
            Assert.IsTrue(pObservedLors > pExpectedLors - 1.96 * sigma);
            Assert.IsTrue(pObservedLors < pExpectedLors + 1.96 * sigma);

            //==========================================
            // DrawFromCensoredOrPositives
            //==========================================

            generatedResidues = new List<double>(repetitions);
            for (int i = 0; i < repetitions; i++) {
                generatedResidues.Add(concentrationModel.DrawFromDistributionExceptZeroes(random, concentrationModel.NonDetectsHandlingMethod));
            }

            // Test positives
            generatedPositives = generatedResidues.Where(s => s > 0 && s != lor).ToList();
            pObservedPositives = (double)generatedPositives.Count / repetitions;
            pExpectedPositives = 1D;
            sigmaPositives = Math.Sqrt((pExpectedPositives * (1 - pExpectedPositives)) / repetitions);
            Assert.IsTrue(pObservedPositives > pExpectedPositives - 1.96 * sigma);
            Assert.IsTrue(pObservedPositives < pExpectedPositives + 1.96 * sigma);

            //==========================================
            // Other
            //==========================================

            Assert.AreEqual(lor, concentrationModel.DrawAccordingToNonDetectsHandlingMethod(random, concentrationModel.NonDetectsHandlingMethod, 1D));
            Assert.AreEqual(0.5 * UtilityFunctions.ExpBound(SpecialFunctions.MeanLeftTruncatedNormal(UtilityFunctions.LogBound(lor), concentrationModel.Mu, concentrationModel.Sigma)), concentrationModel.GetDistributionMean(concentrationModel.NonDetectsHandlingMethod));
            Assert.AreEqual(0.5, concentrationModel.FractionPositives);
            Assert.AreEqual(0.5, concentrationModel.FractionTrueZeros);
            Assert.AreEqual(0D, concentrationModel.FractionCensored);
        }

        /// <summary>
        /// NonDetect spike truncated logNormal
        /// </summary>
        [TestMethod]
        [TestCategory("Concentration Modeling Tests")]
        public void CMNonDetectSpikeTruncatedLogNormalTest6() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var lor = 0.1;
            var mu = Math.Log(10);
            var sigma = .1;

            var rnd = new McraRandomGenerator(seed);
            var residues = new CompoundResidueCollection() {
                Positives = LogNormalDistribution.Samples(rnd, mu, sigma, 100),
                CensoredValuesCollection = Enumerable.Repeat(lor, 100).Select(c => new CensoredValue() { LOD = c, LOQ = c }).ToList(),
            };

            var concentrationModel = new CMNonDetectSpikeTruncatedLogNormal() {
                DesiredModelType = ConcentrationModelType.NonDetectSpikeLogNormal,
                NonDetectsHandlingMethod = NonDetectsHandlingMethod.ReplaceByLOR,
                FractionOfLor = 1,
                CorrectedWeightedAgriculturalUseFraction = 0.75,
                Residues = residues,
            };

            concentrationModel.CalculateParameters();
            var repetitions = 1000;

            //==========================================
            // DrawFromDistribution
            //==========================================

            var generatedResidues = new List<double>(repetitions);
            for (int i = 0; i < repetitions; i++) {
                generatedResidues.Add(concentrationModel.DrawFromDistribution(random, concentrationModel.NonDetectsHandlingMethod));
            }

            // Test positives
            var generatedPositives = generatedResidues.Where(s => s > 0 && s != lor).ToList();
            var pObservedPositives = (double)generatedPositives.Count / repetitions;
            var pExpectedPositives = 0.5;
            var sigmaPositives = Math.Sqrt((pExpectedPositives * (1 - pExpectedPositives)) / repetitions);
            Assert.IsTrue(pObservedPositives > pExpectedPositives - 1.96 * sigma);
            Assert.IsTrue(pObservedPositives < pExpectedPositives + 1.96 * sigma);

            // Test lors
            var generatedLors = generatedResidues.Where(s => s == 0).ToList();
            var pObservedLors = (double)generatedLors.Count / repetitions;
            var pExpectedLors = 0.25;
            var sigmaLors = Math.Sqrt((pExpectedLors * (1 - pExpectedLors)) / repetitions);
            Assert.IsTrue(pObservedLors > pExpectedLors - 1.96 * sigma);
            Assert.IsTrue(pObservedLors < pExpectedLors + 1.96 * sigma);

            //==========================================
            // DrawFromCensoredOrPositives
            //==========================================

            generatedResidues = new List<double>(repetitions);
            for (int i = 0; i < repetitions; i++) {
                generatedResidues.Add(concentrationModel.DrawFromDistributionExceptZeroes(random, concentrationModel.NonDetectsHandlingMethod));
            }

            // Test positives
            generatedPositives = generatedResidues.Where(s => s > 0 && s != lor).ToList();
            pObservedPositives = (double)generatedPositives.Count / repetitions;
            pExpectedPositives = 2D / 3;
            sigmaPositives = Math.Sqrt((pExpectedPositives * (1 - pExpectedPositives)) / repetitions);
            Assert.IsTrue(pObservedPositives > pExpectedPositives - 1.96 * sigma);
            Assert.IsTrue(pObservedPositives < pExpectedPositives + 1.96 * sigma);

            // Test lors
            generatedLors = generatedResidues.Where(s => s == lor).ToList();
            pObservedLors = (double)generatedLors.Count / repetitions;
            pExpectedLors = 1D / 3;
            sigmaLors = Math.Sqrt((pExpectedLors * (1 - pExpectedLors)) / repetitions);
            Assert.IsTrue(pObservedLors > pExpectedLors - 1.96 * sigma);
            Assert.IsTrue(pObservedLors < pExpectedLors + 1.96 * sigma);

            //==========================================
            // Other
            //==========================================

            Assert.AreEqual(lor, concentrationModel.DrawAccordingToNonDetectsHandlingMethod(random, concentrationModel.NonDetectsHandlingMethod, 1D));
            Assert.AreEqual(0.5 * UtilityFunctions.ExpBound(SpecialFunctions.MeanLeftTruncatedNormal(UtilityFunctions.LogBound(lor), concentrationModel.Mu, concentrationModel.Sigma)) + 0.25 * lor, concentrationModel.GetDistributionMean(concentrationModel.NonDetectsHandlingMethod));
            Assert.AreEqual(0.5, concentrationModel.FractionPositives);
            Assert.AreEqual(0.25, concentrationModel.FractionTrueZeros);
            Assert.AreEqual(0.25, concentrationModel.FractionCensored);
        }

        /// <summary>
        /// NonDetect spike truncated logNormal
        /// </summary>
        [TestMethod]
        [TestCategory("Concentration Modeling Tests")]
        public void CMNonDetectSpikeTruncatedLogNormal_Test7() {
            // Model 3: Truncated LogNormal; single LOR; muliple LORS for positives are not allowed
            // Compare with D:\Data\TechnicalDocumentation\UnitTests\ConcentrationModel3.gen

            var logPositives = new List<double>() {
                5.80, 5.42, 6.24, 5.93, 5.33, 5.93, 6.20, 5.99, 5.91, 5.29,
                6.59, 5.20, 5.08, 5.18, 6.62, 5.86, 5.16, 5.15, 5.25, 5.30,
                5.86, 5.21, 6.22, 6.07, 5.55, 5.30, 5.42, 6.23, 5.58, 5.27,
                7.06, 6.13, 5.07, 5.79, 5.63, 5.77, 5.28, 5.52, 5.79, 5.38,
                5.06, 5.11, 5.86, 5.80, 5.53, 6.40, 5.52, 6.40, 5.28, 5.30,
                5.40, 5.47, 5.76, 5.18, 5.19, 5.66, 5.10, 5.45, 6.07, 7.17,
                5.81, 6.31, 5.18, 5.49, 6.38, 5.54, 6.14, 7.54, 5.41, 5.33,
                5.52, 5.68, 5.05, 6.00, 5.98, 5.42, 6.12, 5.23, 5.08, 5.75,
                5.02, 5.60, 6.72, 6.67, 5.17, 5.74, 6.15, 5.09, 5.47, 5.61,
                5.38, 5.42, 5.98, 5.10, 6.74, 6.01, 5.92, 5.02, 5.02, 6.00
            };
            var positives = logPositives.Select(p => UtilityFunctions.ExpBound(p)).ToList();
            //var nonDetects = Enumerable.Repeat(UtilityFunctions.ExpBound(5D), 40).ToList();
            //var lors = new List<double>() { UtilityFunctions.ExpBound(5D) };
            var censoredValuesCollection = Enumerable.Repeat(UtilityFunctions.ExpBound(5D), 40).Select(c => new CensoredValue() { LOD = c, LOQ = c }).ToList();

            var residues = new CompoundResidueCollection() {
                Positives = positives,
                CensoredValuesCollection = censoredValuesCollection,
            };

            var concentrationModel = new CMNonDetectSpikeTruncatedLogNormal() {
                DesiredModelType = ConcentrationModelType.NonDetectSpikeTruncatedLogNormal,
                NonDetectsHandlingMethod = NonDetectsHandlingMethod.ReplaceByLOR,
                FractionOfLor = 1,
                Residues = residues,
            };

            // Fit model
            concentrationModel.CalculateParameters();

            // Check model type
            Assert.AreEqual(concentrationModel.ModelType, ConcentrationModelType.NonDetectSpikeTruncatedLogNormal);

            // GenStat estimates
            var muGen = 5.007748592877e+00;
            var sigmaGen = 8.619917648365e-01;
            var vcovGen00 = 1.619669986495e-01;
            var vcovGen01 = -1.504660918280e-01;
            var vcovGen11 = 1.598543347315e-01;

            // Test parameters
            var mu = concentrationModel.Mu;
            var sigma = concentrationModel.Sigma;
            var tol3 = 1.0e-2;
            var diffMu = Math.Abs((mu - muGen) / mu);
            var diffSigma = Math.Abs((sigma - sigmaGen) / sigma);
            Assert.IsTrue(diffMu < tol3);
            Assert.IsTrue(diffSigma < tol3);

            // Check variance-covariance matrix; use the same estimates as produced by GenStat
            var tol10 = 1.0e-10;
            mu = muGen;
            sigma = sigmaGen;
            var tau = Math.Log(sigma * sigma);
            concentrationModel.PrepareParametricUncertainty(logPositives.ToList(), UtilityFunctions.LogBound(censoredValuesCollection[0].LOD), mu, sigma);

            // Check Parametric drawing
            var diff11 = Math.Abs((concentrationModel.Vcov.GetElement(0, 0) - vcovGen00) / concentrationModel.Vcov.GetElement(0, 0));
            var diff12 = Math.Abs((concentrationModel.Vcov.GetElement(0, 1) - vcovGen01) / concentrationModel.Vcov.GetElement(0, 1));
            var diff22 = Math.Abs((concentrationModel.Vcov.GetElement(1, 1) - vcovGen11) / concentrationModel.Vcov.GetElement(1, 1));
            Assert.IsTrue(diff11 < tol10);
            Assert.IsTrue(diff12 < tol10);
            Assert.IsTrue(diff22 < tol10);

            // Check Parametric drawing
            var ndraws = 10000;
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            List<double> saveSpike = new List<double>(ndraws);
            List<double> saveMu = new List<double>(ndraws);
            List<double> saveLogSigma = new List<double>(ndraws);
            for (int i = 0; i < ndraws; i++) {
                concentrationModel.DrawParametricUncertainty(random);
                saveSpike.Add(1 - concentrationModel.FractionPositives);
                saveMu.Add(concentrationModel.Mu);
                saveLogSigma.Add(Math.Log(concentrationModel.Sigma * concentrationModel.Sigma));
            }

            // Means and variances of random draws;
            var meanSpike = saveSpike.Average();
            var varSpike = saveSpike.Variance();
            var meanMu = saveMu.Average();
            var varMu = saveMu.Variance();
            var meanLogSigma = saveLogSigma.Average();
            var varLogSigma = saveLogSigma.Variance();
            var covar = 0d;
            for (int i = 0; i < ndraws; i++) {
                covar += (saveMu[i] - meanMu) * (saveLogSigma[i] - meanLogSigma);
            }
            covar /= (Convert.ToDouble(ndraws) - 1d);

            // Compare means and variances with true values
            var alfa = censoredValuesCollection.Count + 1d;
            var beta = residues.NumberOfResidues - censoredValuesCollection.Count + 1d;
            var Espike1 = alfa / (alfa + beta);
            var Espike2 = Espike1 * (1d - Espike1) / (alfa + beta + 1d);
            var diffSpike1 = Math.Abs((Espike1 - meanSpike) / Espike1);
            var diffSpike2 = Math.Abs((Espike2 - varSpike) / Espike2);
            var diffMu1 = Math.Abs((mu - meanMu) / mu);
            var diffMu2 = Math.Abs((concentrationModel.Vcov.GetElement(0, 0) - varMu) / concentrationModel.Vcov.GetElement(0, 0));
            var diffSigma1 = Math.Abs((tau - meanLogSigma) / tau);
            var diffSigma2 = Math.Abs((concentrationModel.Vcov.GetElement(1, 1) - varLogSigma) / concentrationModel.Vcov.GetElement(1, 1));
            var diffCovar = Math.Abs((concentrationModel.Vcov.GetElement(1, 0) - covar) / concentrationModel.Vcov.GetElement(1, 0));

            // Assert
            var tol1 = 1.0e-1;
            Assert.IsTrue(diffSpike1 < tol1);
            Assert.IsTrue(diffSpike2 < tol1);
            Assert.IsTrue(diffMu1 < tol1);
            Assert.IsTrue(diffMu2 < tol1);
            Assert.IsTrue(diffSigma1 < tol1);
            Assert.IsTrue(diffSigma2 < tol1);
            Assert.IsTrue(diffCovar < tol1);
        }
    }
}
