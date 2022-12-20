using MCRA.Utils.Statistics;
using MCRA.General;
using MCRA.Simulation.Calculators.CompoundResidueCollectionCalculation;
using MCRA.Simulation.Calculators.ConcentrationModelCalculation.ConcentrationModels;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using ExcelDataReader.Log;
using System.Linq.Expressions;

namespace MCRA.Simulation.Test.UnitTests.Calculators.ConcentrationModelCalculation.ConcentrationModels {

    /// <summary>
    /// ResidueGeneration calculator
    /// </summary>
    [TestClass]
    public class CMCensoredLogNormalTests {

        /// <summary>
        /// Test censored log-normal model fit for no data; the model should give a ParameterFitException.
        /// </summary>
        [TestMethod]
        [TestCategory("Concentration Modeling Tests")]
        public void CMCensoredLogNormalTest1() {
            var residues = new CompoundResidueCollection() {
                Positives = new List<double>(),
                CensoredValuesCollection = new List<CensoredValueCollection>(),
            };

            var concentrationModel = new CMCensoredLogNormal() {
                DesiredModelType = ConcentrationModelType.NonDetectSpikeLogNormal,
                NonDetectsHandlingMethod = NonDetectsHandlingMethod.ReplaceByLOR,
                FractionOfLOR = 1,
                Residues = residues,
            };

            Assert.IsFalse(concentrationModel.CalculateParameters());
        }

        /// <summary>
        /// Test censored log-normal model fit for data with only nondetects (replaced by LOR);
        /// the model should give a ParameterFitException.
        /// </summary>
        [TestMethod]
        [TestCategory("Concentration Modeling Tests")]
        public void CMCensoredLogNormalTest2() {
            var lor = 0.1;

            var residues = new CompoundResidueCollection() {
                Positives = new List<double>(),
                CensoredValuesCollection = new List<CensoredValueCollection>() { new CensoredValueCollection() { LOD = lor, LOQ = lor } }
            };

            var concentrationModel = new CMCensoredLogNormal() {
                DesiredModelType = ConcentrationModelType.NonDetectSpikeLogNormal,
                NonDetectsHandlingMethod = NonDetectsHandlingMethod.ReplaceByLOR,
                FractionOfLOR = 1,
                Residues = residues,
            };

            Assert.IsFalse(concentrationModel.CalculateParameters());
        }

        /// <summary>
        /// Test censored log-normal model fit for data with only nondetects (replaced by LOR in 50% and by 0 in 50% based on agricultural use)
        /// the model should give a ParameterFitException.
        /// </summary>
        [TestMethod]
        [TestCategory("Concentration Modeling Tests")]
        public void CMCensoredLogNormalTest3() {
            var lor = 0.1;

            var residues = new CompoundResidueCollection() {
                Positives = new List<double>(),
                CensoredValuesCollection = new List<CensoredValueCollection>() { new CensoredValueCollection() { LOD = lor, LOQ = lor } }
            };

            var concentrationModel = new CMCensoredLogNormal() {
                DesiredModelType = ConcentrationModelType.NonDetectSpikeLogNormal,
                NonDetectsHandlingMethod = NonDetectsHandlingMethod.ReplaceByLOR,
                FractionOfLOR = 1,
                Residues = residues,
            };

            Assert.IsFalse(concentrationModel.CalculateParameters());
        }

        /// <summary>
        /// Test censored log-normal model fit with fraction positive residues higher than
        /// the specified agricultural use percentage. Should fail; leads to all censored values
        /// interpretted as true zeros.
        /// </summary>
        [TestMethod]
        [TestCategory("Concentration Modeling Tests")]
        public void CMCensoredLogNormalTest4() {
            var lor = 0.1;

            var residues = new CompoundResidueCollection() {
                Positives = new List<double>() { 0.1, 0.2, 0.3 },
                CensoredValuesCollection = new List<CensoredValueCollection>() { new CensoredValueCollection() { LOD = lor, LOQ = lor } }
            };

            var concentrationModel = new CMCensoredLogNormal() {
                DesiredModelType = ConcentrationModelType.NonDetectSpikeLogNormal,
                NonDetectsHandlingMethod = NonDetectsHandlingMethod.ReplaceByLOR,
                FractionOfLOR = 1,
                Residues = residues,
                CorrectedWeightedAgriculturalUseFraction = 0.5,
            };

            Assert.IsFalse(concentrationModel.CalculateParameters());
        }

        /// <summary>
        /// Test censored log-normal model fit for 100% agricultural use. The model should return
        /// 100% positives when drawing from the distribution.
        /// </summary>
        [TestMethod]
        [TestCategory("Concentration Modeling Tests")]
        public void CMCensoredLogNormalTest5() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);

            var mu = .25;
            var sigma = 1;
            var residues = MockCompoundResidueCollectionsGenerator
                .CreateSingle(
                    food: null,
                    substance: null,
                    mu: mu,
                    sigma: sigma,
                    fractionZero: 0.25,
                    markZerosAsNonDetects: true,
                    lors: new double[] { 0.5 },
                    numberOfSamples: 100
            );

            var concentrationModel = new CMCensoredLogNormal() {
                DesiredModelType = ConcentrationModelType.NonDetectSpikeLogNormal,
                NonDetectsHandlingMethod = NonDetectsHandlingMethod.ReplaceByLOR,
                FractionOfLOR = 1,
                WeightedAgriculturalUseFraction = 1,
                CorrectedWeightedAgriculturalUseFraction = 1,
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
            var generatedPositives = generatedResidues.Where(s => s > 0).ToList();
            var pObservedPositives = (double)generatedPositives.Count / repetitions;
            var pExpectedPositives = 1D;
            var sigmaPositives = Math.Sqrt((pExpectedPositives * (1 - pExpectedPositives)) / repetitions);
            Assert.IsTrue(pObservedPositives > pExpectedPositives - 1.96 * sigma);
            Assert.IsTrue(pObservedPositives < pExpectedPositives + 1.96 * sigma);

            // Test zeros
            var generatedZeros = generatedResidues.Where(s => s == 0).ToList();
            var pObservedZeros = (double)generatedZeros.Count / repetitions;
            var pExpectedZeros = 0D;
            var sigmaLors = Math.Sqrt((pExpectedZeros * (1 - pExpectedZeros)) / repetitions);
            Assert.IsTrue(pObservedZeros > pExpectedZeros - 1.96 * sigma);
            Assert.IsTrue(pObservedZeros < pExpectedZeros + 1.96 * sigma);

            //==========================================
            // DrawFromCensoredOrPositives
            //==========================================

            generatedResidues = new List<double>(repetitions);
            for (int i = 0; i < repetitions; i++) {
                generatedResidues.Add(concentrationModel.DrawFromDistributionExceptZeroes(random, concentrationModel.NonDetectsHandlingMethod));
            }

            // Test positives
            generatedPositives = generatedResidues.Where(s => s > 0).ToList();
            pObservedPositives = (double)generatedPositives.Count / repetitions;
            pExpectedPositives = 1D;
            sigmaPositives = Math.Sqrt((pExpectedPositives * (1 - pExpectedPositives)) / repetitions);
            Assert.IsTrue(pObservedPositives > pExpectedPositives - 1.96 * sigma);
            Assert.IsTrue(pObservedPositives < pExpectedPositives + 1.96 * sigma);

            //==========================================
            // Other
            //==========================================

            var correctedAgriculturalUseFraction = Math.Max(concentrationModel.FractionPositives, concentrationModel.WeightedAgriculturalUseFraction);
            var distributionMean = correctedAgriculturalUseFraction * Math.Exp(concentrationModel.Mu + 0.5 * Math.Pow(concentrationModel.Sigma, 2));

            Assert.AreEqual(distributionMean, concentrationModel.GetDistributionMean(concentrationModel.NonDetectsHandlingMethod));
            Assert.AreEqual(residues.FractionPositives, concentrationModel.FractionPositives, 1e-4);
            Assert.AreEqual(1 - concentrationModel.CorrectedWeightedAgriculturalUseFraction, concentrationModel.FractionTrueZeros, 1e-4);
            Assert.AreEqual(concentrationModel.CorrectedWeightedAgriculturalUseFraction - residues.FractionPositives, concentrationModel.FractionCensored, 1e-4);
        }

        /// <summary>
        /// Test censored log-normal model fit for 75% agricultural use.
        /// </summary>
        [TestMethod]
        [TestCategory("UnitTestsTODO")]
        [TestCategory("Concentration Modeling Tests")]
        public void CMCensoredLogNormalTest6() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);

            var mu = .25;
            var sigma = 1;
            var residues = MockCompoundResidueCollectionsGenerator
                .CreateSingle(
                    food: null,
                    substance: null,
                    mu: mu,
                    sigma: sigma,
                    fractionZero: 0.25,
                    markZerosAsNonDetects: true,
                    lors: new double[] { 0.5 },
                    numberOfSamples: 100
            );

            var concentrationModel = new CMCensoredLogNormal() {
                DesiredModelType = ConcentrationModelType.NonDetectSpikeLogNormal,
                NonDetectsHandlingMethod = NonDetectsHandlingMethod.ReplaceByLOR,
                WeightedAgriculturalUseFraction = 0.75,
                CorrectedWeightedAgriculturalUseFraction = 0.75,
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
            var generatedPositives = generatedResidues.Where(s => s > 0).ToList();
            var pObservedPositives = (double)generatedPositives.Count / repetitions;
            var pExpectedPositives = 0.75;
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
            generatedPositives = generatedResidues.Where(s => s > 0).ToList();
            pObservedPositives = (double)generatedPositives.Count / repetitions;
            pExpectedPositives = 1;
            sigmaPositives = Math.Sqrt((pExpectedPositives * (1 - pExpectedPositives)) / repetitions);
            Assert.IsTrue(pObservedPositives > pExpectedPositives - 1.96 * sigma);
            Assert.IsTrue(pObservedPositives < pExpectedPositives + 1.96 * sigma);

            //==========================================
            // Other
            //==========================================

            var correctedAgriculturalUseFraction = Math.Max(concentrationModel.FractionPositives, concentrationModel.WeightedAgriculturalUseFraction);
            var distributionMean = correctedAgriculturalUseFraction * Math.Exp(concentrationModel.Mu + 0.5 * Math.Pow(concentrationModel.Sigma, 2));

            Assert.AreEqual(distributionMean, concentrationModel.GetDistributionMean(concentrationModel.NonDetectsHandlingMethod));
            Assert.AreEqual(residues.FractionPositives, concentrationModel.FractionPositives, 1e-4);
            Assert.AreEqual(1 - concentrationModel.CorrectedWeightedAgriculturalUseFraction, concentrationModel.FractionTrueZeros, 1e-4);
            Assert.AreEqual(concentrationModel.CorrectedWeightedAgriculturalUseFraction - residues.FractionPositives, concentrationModel.FractionCensored, 1e-4);
        }

        /// <summary>
        /// Test censored log-normal model fit for 75% agricultural use.
        /// </summary>
        [TestMethod]
        [TestCategory("UnitTestsTODO")]
        [TestCategory("Concentration Modeling Tests")]
        public void CMCensoredLogNormalTest7() {
            var data = new List<double> {0.01, 0.01,
                0.01, 0.01, 0.01, 0.01, 0.01, 0.01, 0.01, 0.01, 0.01,
                0.01, 0.02, 0.02, 0.02, 0.02, 0.02, 0.02, 0.02, 0.02,
                0.02, 0.02, 0.02, 0.02, 0.02, 0.02, 0.03, 0.03, 0.03,
                0.03, 0.03, 0.03, 0.03, 0.04, 0.04, 0.04, 0.04, 0.04,
                0.04, 0.05, 0.05, 0.05, 0.06, 0.47};
            var censoredValues = new List<CensoredValueCollection>();
            for (int i = 0; i < 16; i++) {
                censoredValues.Add(new CensoredValueCollection() { LOD = 0.01, LOQ = 0.01 });
            }

            var residues = new CompoundResidueCollection() {
                Positives = data,
                CensoredValuesCollection = censoredValues
            };

            var concentrationModel = new CMCensoredLogNormal() {
                DesiredModelType = ConcentrationModelType.NonDetectSpikeLogNormal,
                NonDetectsHandlingMethod = NonDetectsHandlingMethod.ReplaceByLOR,
                FractionOfLOR = 1,
                Residues = residues,
            };

            concentrationModel.CalculateParameters();
        }
    }
}
