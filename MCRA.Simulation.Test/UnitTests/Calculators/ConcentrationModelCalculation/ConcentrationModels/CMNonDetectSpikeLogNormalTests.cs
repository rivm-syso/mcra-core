using MCRA.Utils;
using MCRA.Utils.Statistics;
using MCRA.General;
using MCRA.Simulation.Calculators.CompoundResidueCollectionCalculation;
using MCRA.Simulation.Calculators.ConcentrationModelCalculation.ConcentrationModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Simulation.Test.UnitTests.Calculators.ConcentrationModelCalculation.ConcentrationModels {

    /// <summary>
    /// Censored value spike lognormal concentration model tests.
    /// </summary>
    [TestClass]
    public class CMNonDetectSpikeLogNormalTests {

        /// <summary>
        /// Without data the model should give a ParameterFitException
        /// </summary>
        [TestMethod]
        public void CMNonDetectSpikeLogNormalTest1() {
            var positives = new List<double>();

            var residues = new CompoundResidueCollection() {
                Positives = positives,
                CensoredValuesCollection = new List<CensoredValueCollection>(),
            };

            var concentrationModel = new CMNonDetectSpikeLogNormal() {
                DesiredModelType = ConcentrationModelType.NonDetectSpikeLogNormal,
                NonDetectsHandlingMethod = NonDetectsHandlingMethod.ReplaceByLOR,
                CorrectedWeightedAgriculturalUseFraction = residues.FractionPositives,
                FractionOfLOR = 1,
                Residues = residues,
            };

            Assert.IsFalse(concentrationModel.CalculateParameters());
        }

        /// <summary>
        ///  With only nondetects (replaced by LOR) the model should give a ParameterFitException
        /// </summary>
        [TestMethod]
        public void CMNonDetectSpikeLogNormalTest2() {
            var lor = 0.1;
            var positives = new List<double>();

            var residues = new CompoundResidueCollection() {
                Positives = positives,
                CensoredValuesCollection = new List<CensoredValueCollection>() { new CensoredValueCollection() { LOD = lor, LOQ = lor } }
            };

            var concentrationModel = new CMNonDetectSpikeLogNormal() {
                DesiredModelType = ConcentrationModelType.NonDetectSpikeLogNormal,
                NonDetectsHandlingMethod = NonDetectsHandlingMethod.ReplaceByLOR,
                FractionOfLOR = 1,
                Residues = residues,
            };

            Assert.IsFalse(concentrationModel.CalculateParameters());
        }

        /// <summary>
        /// With only nondetects (replaced by LOR in 50% and by 0 in 50% based on agricultural use) the model should give a ParameterFitException
        /// </summary>
        [TestMethod]
        public void CMNonDetectSpikeLogNormalTest3() {
            var lor = 0.1;
            var positives = new List<double>();

            var residues = new CompoundResidueCollection() {
                Positives = positives,
                CensoredValuesCollection = new List<CensoredValueCollection>() { new CensoredValueCollection() { LOD = lor, LOQ = lor } }
            };

            var concentrationModel = new CMNonDetectSpikeLogNormal() {
                DesiredModelType = ConcentrationModelType.NonDetectSpikeLogNormal,
                NonDetectsHandlingMethod = NonDetectsHandlingMethod.ReplaceByLOR,
                FractionOfLOR = 1,
                Residues = residues,
            };

            Assert.IsFalse(concentrationModel.CalculateParameters());
        }

        /// <summary>
        /// With 100 censored values (replaced by LOR) and 100 positives, the fractions of LORs and positives should be ca. 50%.
        /// The method DrawAccordingToNonDetectsHandlingMethod should give LOR.
        /// The mean should be the average of LOR and exp(Mu+0.5*Sigma^2)
        /// The fractions positives/censored/truezeroes should be 0.5 / 0.5 / 0
        /// </summary>
        [TestMethod]
        public void CMNonDetectSpikeLogNormalTest4a() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var lor = 0.1;
            var mu = Math.Log(10);
            var sigma = .1;
            var residues = new CompoundResidueCollection() {
                Positives = LogNormalDistribution.Samples(random, mu, sigma, 100),
                CensoredValuesCollection = Enumerable.Repeat(lor, 100).Select(c => new CensoredValueCollection() { LOD = c, LOQ = c }).ToList(),
            };

            var concentrationModel = new CMNonDetectSpikeLogNormal() {
                DesiredModelType = ConcentrationModelType.NonDetectSpikeLogNormal,
                NonDetectsHandlingMethod = NonDetectsHandlingMethod.ReplaceByLOR,
                FractionOfLOR = 1,
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
            Assert.IsTrue(pObservedPositives > pExpectedPositives - 1.96 * sigmaPositives);
            Assert.IsTrue(pObservedPositives < pExpectedPositives + 1.96 * sigmaPositives);

            // Test lors
            var generatedLors = generatedResidues.Where(s => s == lor).ToList();
            var pObservedLors = (double)generatedLors.Count / repetitions;
            var pExpectedLors = 0.5;
            var sigmaLors = Math.Sqrt((pExpectedLors * (1 - pExpectedLors)) / repetitions);
            Assert.IsTrue(pObservedLors > pExpectedLors - 1.96 * sigmaLors);
            Assert.IsTrue(pObservedLors < pExpectedLors + 1.96 * sigmaLors);

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
            Assert.IsTrue(pObservedPositives > pExpectedPositives - 1.96 * sigmaPositives);
            Assert.IsTrue(pObservedPositives < pExpectedPositives + 1.96 * sigmaPositives);

            // Test lors
            generatedLors = generatedResidues.Where(s => s == lor).ToList();
            pObservedLors = (double)generatedLors.Count / repetitions;
            pExpectedLors = 0.5;
            sigmaLors = Math.Sqrt((pExpectedLors * (1 - pExpectedLors)) / repetitions);
            Assert.IsTrue(pObservedLors > pExpectedLors - 1.96 * sigmaLors);
            Assert.IsTrue(pObservedLors < pExpectedLors + 1.96 * sigmaLors);

            //==========================================
            // Other
            //==========================================

            Assert.AreEqual(lor, concentrationModel.DrawAccordingToNonDetectsHandlingMethod(random, concentrationModel.NonDetectsHandlingMethod, 1D));
            Assert.AreEqual(0.5 * Math.Exp(concentrationModel.Mu + 0.5 * Math.Pow(concentrationModel.Sigma, 2)) + 0.5 * lor, concentrationModel.GetDistributionMean(concentrationModel.NonDetectsHandlingMethod));
            Assert.AreEqual(0.5, concentrationModel.FractionPositives);
            Assert.AreEqual(0D, concentrationModel.FractionTrueZeros);
            Assert.AreEqual(0.5, concentrationModel.FractionCensored);

        }

        /// <summary>
        /// With 100 censored values (replaced by LOR) and 100 positives, the fractions of LORs and positives should be ca. 50%.
        /// The method DrawAccordingToNonDetectsHandlingMethod should give LOR.
        /// The mean should be the average of LOR and exp(Mu+0.5*Sigma^2)
        /// The fractions positives/censored/truezeroes should be 0.5 / 0.5 / 0
        /// </summary>
        [TestMethod]
        public void CMNonDetectSpikeLogNormalTest4b() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var lod = 0.1;
            var mu = Math.Log(10);
            var sigma = .1;
            var residues = new CompoundResidueCollection() {
                Positives = LogNormalDistribution.Samples(random, mu, sigma, 100),
                CensoredValuesCollection = Enumerable.Repeat(lod, 100).Select(c => new CensoredValueCollection() { LOD = c, LOQ = c, ResType = ResType.LOD }).ToList(),
            };

            var concentrationModel = new CMNonDetectSpikeLogNormal() {
                DesiredModelType = ConcentrationModelType.NonDetectSpikeLogNormal,
                NonDetectsHandlingMethod = NonDetectsHandlingMethod.ReplaceByLODLOQSystem,
                FractionOfLOR = 1,
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
            var generatedPositives = generatedResidues.Where(s => s > 0 && s != lod).ToList();
            var pObservedPositives = (double)generatedPositives.Count / repetitions;
            var pExpectedPositives = 0.5;
            var sigmaPositives = Math.Sqrt((pExpectedPositives * (1 - pExpectedPositives)) / repetitions);
            Assert.IsTrue(pObservedPositives > pExpectedPositives - 1.96 * sigmaPositives);
            Assert.IsTrue(pObservedPositives < pExpectedPositives + 1.96 * sigmaPositives);

            // Test lors
            var generatedLors = generatedResidues.Where(s => s == lod).ToList();
            var pObservedLors = (double)generatedLors.Count / repetitions;
            var pExpectedLors = 0.5;
            var sigmaLors = Math.Sqrt((pExpectedLors * (1 - pExpectedLors)) / repetitions);
            Assert.IsTrue(pObservedLors > pExpectedLors - 1.96 * sigmaLors);
            Assert.IsTrue(pObservedLors < pExpectedLors + 1.96 * sigmaLors);

            //==========================================
            // DrawFromCensoredOrPositives
            //==========================================

            generatedResidues = new List<double>(repetitions);
            for (int i = 0; i < repetitions; i++) {
                generatedResidues.Add(concentrationModel.DrawFromDistributionExceptZeroes(random, concentrationModel.NonDetectsHandlingMethod));
            }

            // Test positives
            generatedPositives = generatedResidues.Where(s => s > 0 && s != lod).ToList();
            pObservedPositives = (double)generatedPositives.Count / repetitions;
            pExpectedPositives = 0.5;
            sigmaPositives = Math.Sqrt((pExpectedPositives * (1 - pExpectedPositives)) / repetitions);
            Assert.IsTrue(pObservedPositives > pExpectedPositives - 1.96 * sigmaPositives);
            Assert.IsTrue(pObservedPositives < pExpectedPositives + 1.96 * sigmaPositives);

            // Test lors
            generatedLors = generatedResidues.Where(s => s == lod).ToList();
            pObservedLors = (double)generatedLors.Count / repetitions;
            pExpectedLors = 0.5;
            sigmaLors = Math.Sqrt((pExpectedLors * (1 - pExpectedLors)) / repetitions);
            Assert.IsTrue(pObservedLors > pExpectedLors - 1.96 * sigmaLors);
            Assert.IsTrue(pObservedLors < pExpectedLors + 1.96 * sigmaLors);

            //==========================================
            // Other
            //==========================================

            Assert.AreEqual(lod, concentrationModel.DrawAccordingToNonDetectsHandlingMethod(random, concentrationModel.NonDetectsHandlingMethod, 1D));
            Assert.AreEqual(0.5 * Math.Exp(concentrationModel.Mu + 0.5 * Math.Pow(concentrationModel.Sigma, 2)) + 0.5 * lod, concentrationModel.GetDistributionMean(concentrationModel.NonDetectsHandlingMethod));
            Assert.AreEqual(0.5, concentrationModel.FractionPositives);
            Assert.AreEqual(0D, concentrationModel.FractionTrueZeros);
            Assert.AreEqual(0.5, concentrationModel.FractionCensored);

            var generatedMeanResidues = new List<double>(1000);
            for (int i = 0; i < 1000; i++) {
                concentrationModel.DrawParametricUncertainty(random);
                generatedMeanResidues.Add(concentrationModel.GetDistributionMean(concentrationModel.NonDetectsHandlingMethod));
            }
            var mean = generatedMeanResidues.Average();
            Assert.AreEqual(0.5 * Math.Exp(concentrationModel.Mu + 0.5 * Math.Pow(concentrationModel.Sigma, 2)) + 0.5 * lod, mean,1e-1);
        }

        /// <summary>
        /// With 100 nondetects (replaced by LOR) and 100 positives and 50% agricultural use, the fractions of zeroes and positives should be ca. 50%.
        /// The method DrawAccordingToNonDetectsHandlingMethod should give LOR.
        /// The mean should be 0.5*exp(Mu+0.5*Sigma^2)
        /// The fractions positives/censored/truezeroes should be 0.5 / 0 / 0.5
        /// </summary>
        [TestMethod]
        public void CMNonDetectSpikeLogNormalTest5() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var lor = 0.1;
            var mu = Math.Log(10);
            var sigma = .1;
            var positives = new List<double>();
            for (int i = 0; i < 100; i++) {
                positives.Add(UtilityFunctions.ExpBound(mu + NormalDistribution.Draw(random, 0, 1) * sigma));
            }

            var residues = new CompoundResidueCollection() {
                Positives = positives,
                CensoredValuesCollection = Enumerable.Repeat(lor, 100).Select(c => new CensoredValueCollection() { LOD = c, LOQ = c }).ToList(),
            };

            var concentrationModel = new CMNonDetectSpikeLogNormal() {
                DesiredModelType = ConcentrationModelType.NonDetectSpikeLogNormal,
                NonDetectsHandlingMethod = NonDetectsHandlingMethod.ReplaceByLOR,
                FractionOfLOR = 1,
                Residues = residues,
                CorrectedWeightedAgriculturalUseFraction = .5
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
            var generatedZeroes = generatedResidues.Where(s => s == 0).ToList();
            var pObservedZeroes = (double)generatedZeroes.Count / repetitions;
            var pExpectedZeroes = 0.5;
            var sigmaLors = Math.Sqrt((pExpectedZeroes * (1 - pExpectedZeroes)) / repetitions);
            Assert.IsTrue(pObservedZeroes > pExpectedZeroes - 1.96 * sigma);
            Assert.IsTrue(pObservedZeroes < pExpectedZeroes + 1.96 * sigma);

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
            Assert.AreEqual(0.5 * Math.Exp(concentrationModel.Mu + 0.5 * Math.Pow(concentrationModel.Sigma, 2)), concentrationModel.GetDistributionMean(concentrationModel.NonDetectsHandlingMethod));
            Assert.AreEqual(0.5, concentrationModel.FractionPositives);
            Assert.AreEqual(0.5, concentrationModel.FractionTrueZeros);
            Assert.AreEqual(0D, concentrationModel.FractionCensored);

        }

        /// <summary>
        /// With 100 nondetects (replaced by LOR) and 100 positives and 75% agricultural use, the fractions of LORs and positives should be ca. 25% and 50%.
        /// The method DrawAccordingToNonDetectsHandlingMethod should give LOR.
        /// The mean should be 0.5*exp(Mu+0.5*Sigma^2)
        /// The fractions positives/censored/truezeroes should be 0.5 / 0 / 0.5
        /// </summary>
        [TestMethod]
        public void CMNonDetectSpikeLogNormalTest6() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var lor = 0.1;
            var mu = Math.Log(10);
            var sigma = .1;
            var positives = new List<double>();
            for (int i = 0; i < 100; i++) {
                positives.Add(UtilityFunctions.ExpBound(mu + NormalDistribution.InvCDF(0, 1, random.NextDouble()) * sigma));
            }

            var residues = new CompoundResidueCollection() {
                Positives = positives,
                CensoredValuesCollection = Enumerable.Repeat(lor, 100).Select(c => new CensoredValueCollection() { LOD = c, LOQ = c }).ToList()
            };

            var concentrationModel = new CMNonDetectSpikeLogNormal() {
                DesiredModelType = ConcentrationModelType.NonDetectSpikeLogNormal,
                NonDetectsHandlingMethod = NonDetectsHandlingMethod.ReplaceByLOR,
                FractionOfLOR = 1,
                CorrectedWeightedAgriculturalUseFraction = .75,
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
            Assert.AreEqual(0.5 * Math.Exp(concentrationModel.Mu + 0.5 * Math.Pow(concentrationModel.Sigma, 2)) + 0.25 * lor, concentrationModel.GetDistributionMean(concentrationModel.NonDetectsHandlingMethod));
            Assert.AreEqual(0.5, concentrationModel.FractionPositives);
            Assert.AreEqual(0.25, concentrationModel.FractionTrueZeros);
            Assert.AreEqual(0.25, concentrationModel.FractionCensored);

        }
    }
}
