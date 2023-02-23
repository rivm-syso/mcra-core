using MCRA.Utils.Statistics;
using MCRA.General;
using MCRA.Simulation.Calculators.CompoundResidueCollectionCalculation;
using MCRA.Simulation.Calculators.ConcentrationModelCalculation.ConcentrationModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.Calculators.ConcentrationModelCalculation.ConcentrationModels {
    /// <summary>
    /// ResidueGeneration calculator
    /// </summary>
    [TestClass]
    public class CMMaximumResidueLimitTests {
        /// <summary>
        /// Maximum residue limits
        /// </summary>
        [TestMethod]
        [TestCategory("Concentration Modeling Tests")]
        public void CMMaximumResidueLimitTest1() {
            var positives = new List<double>();
            var residues = new CompoundResidueCollection() {
                Positives = positives,
                CensoredValuesCollection = new List<CensoredValueCollection>(),
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
        /// Maximum residue limits
        /// </summary>
        [TestMethod]
        [TestCategory("Concentration Modeling Tests")]
        public void CMMaximumResidueLimitTest2() {
            var positives = new List<double>();
            var mrl = 0.5;

            var residues = new CompoundResidueCollection() {
                Positives = positives,
                CensoredValuesCollection = new List<CensoredValueCollection>(),
            };

            var concentrationModel = new CMMaximumResidueLimit() {
                DesiredModelType = ConcentrationModelType.Empirical,
                NonDetectsHandlingMethod = NonDetectsHandlingMethod.ReplaceByLOR,
                FractionOfLOR = 1,
                Residues = residues,
                MaximumResidueLimit = mrl,
                WeightedAgriculturalUseFraction = 0D,
                CorrectedWeightedAgriculturalUseFraction = 0D,
            };

            concentrationModel.CalculateParameters();

            var seed = 1;
            var random = new McraRandomGenerator(seed);
            Assert.AreEqual(0D, concentrationModel.DrawFromDistribution(random, concentrationModel.NonDetectsHandlingMethod));
            Assert.AreEqual(0.5, concentrationModel.DrawFromDistributionExceptZeroes(random, concentrationModel.NonDetectsHandlingMethod));
            Assert.AreEqual(0D, concentrationModel.GetDistributionMean(concentrationModel.NonDetectsHandlingMethod));
            Assert.AreEqual(0D, concentrationModel.FractionPositives);
            Assert.AreEqual(1D, concentrationModel.FractionTrueZeros);
            Assert.AreEqual(0D, concentrationModel.FractionCensored);
        }

        /// <summary>
        /// Maximum residue limits
        /// </summary>
        [TestMethod]
        [TestCategory("Concentration Modeling Tests")]
        public void CMMaximumResidueLimitTest3() {
            var positives = new List<double>();
            var mrl = 0.5;

            var residues = new CompoundResidueCollection() {
                Positives = positives,
                CensoredValuesCollection = new List<CensoredValueCollection>(),
            };

            var concentrationModel = new CMMaximumResidueLimit() {
                DesiredModelType = ConcentrationModelType.Empirical,
                NonDetectsHandlingMethod = NonDetectsHandlingMethod.ReplaceByLOR,
                FractionOfLOR = 1,
                Residues = residues,
                MaximumResidueLimit = mrl,
            };

            concentrationModel.CalculateParameters();

            var seed = 1;
            var random = new McraRandomGenerator(seed);
            Assert.AreEqual(mrl, concentrationModel.DrawFromDistribution(random, concentrationModel.NonDetectsHandlingMethod));
            Assert.AreEqual(mrl, concentrationModel.DrawFromDistributionExceptZeroes(random, concentrationModel.NonDetectsHandlingMethod));
            Assert.AreEqual(mrl, concentrationModel.GetDistributionMean(concentrationModel.NonDetectsHandlingMethod));
            Assert.AreEqual(1D, concentrationModel.FractionPositives);
            Assert.AreEqual(0D, concentrationModel.FractionTrueZeros);
            Assert.AreEqual(0D, concentrationModel.FractionCensored);

        }
        /// <summary>
        /// Maximum residue limits
        /// </summary>
        [TestMethod]
        [TestCategory("Concentration Modeling Tests")]
        public void CMMaximumResidueLimitTest4() {
            var positives = new List<double>();
            var mrl = 0.5;

            var residues = new CompoundResidueCollection() {
                Positives = positives,
                CensoredValuesCollection = new List<CensoredValueCollection>(),
            };

            var concentrationModel = new CMMaximumResidueLimit() {
                DesiredModelType = ConcentrationModelType.Empirical,
                NonDetectsHandlingMethod = NonDetectsHandlingMethod.ReplaceByLOR,
                FractionOfLOR = 1,
                Residues = residues,
                MaximumResidueLimit = mrl,
                WeightedAgriculturalUseFraction = 0.5,
                CorrectedWeightedAgriculturalUseFraction = 0.5,
            };

            concentrationModel.CalculateParameters();

            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var repetitions = 10000;
            var generatedResidues = new List<double>(repetitions);
            for (int i = 0; i < repetitions; i++) {
                generatedResidues.Add(concentrationModel.DrawFromDistribution(random, concentrationModel.NonDetectsHandlingMethod));
            }
            var observed = generatedResidues.Count(r => r == mrl);
            var pObserved = (double)observed / repetitions;
            var pExpected = concentrationModel.CorrectedWeightedAgriculturalUseFraction;
            var sigma = Math.Sqrt((pExpected * (1 - pExpected)) / repetitions);
            Assert.IsTrue(pObserved > pExpected - 1.96 * sigma);
            Assert.IsTrue(pObserved < pExpected + 1.96 * sigma);

            Assert.AreEqual(mrl, concentrationModel.DrawFromDistributionExceptZeroes(random, concentrationModel.NonDetectsHandlingMethod));
            Assert.AreEqual(0.5 * mrl, concentrationModel.GetDistributionMean(concentrationModel.NonDetectsHandlingMethod));
            Assert.AreEqual(.5, concentrationModel.FractionPositives);
            Assert.AreEqual(.5, concentrationModel.FractionTrueZeros);
            Assert.AreEqual(0d, concentrationModel.FractionCensored);
        }

        /// <summary>
        /// Maximum residue limits
        /// </summary>
        [TestMethod]
        [TestCategory("Concentration Modeling Tests")]
        public void CMMaximumResidueLimitTest5() {
            var lor = 0.1;
            var positives = new List<double>();
            var mrl = 0.5;

            var residues = new CompoundResidueCollection() {
                Positives = positives,
                CensoredValuesCollection = new List<CensoredValueCollection>() { new CensoredValueCollection() { LOD = lor, LOQ = lor } }
            };

            var concentrationModel = new CMMaximumResidueLimit() {
                DesiredModelType = ConcentrationModelType.Empirical,
                NonDetectsHandlingMethod = NonDetectsHandlingMethod.ReplaceByLOR,
                FractionOfLOR = 1,
                Residues = residues,
                MaximumResidueLimit = mrl,
                WeightedAgriculturalUseFraction = 0,
                CorrectedWeightedAgriculturalUseFraction = 0,
            };

            concentrationModel.CalculateParameters();

            var seed = 1;
            var random = new McraRandomGenerator(seed);
            Assert.AreEqual(0D, concentrationModel.DrawFromDistribution(random, concentrationModel.NonDetectsHandlingMethod));
            Assert.AreEqual(0.5, concentrationModel.DrawFromDistributionExceptZeroes(random, concentrationModel.NonDetectsHandlingMethod));
            Assert.AreEqual(0D, concentrationModel.GetDistributionMean(concentrationModel.NonDetectsHandlingMethod));
            Assert.AreEqual(0D, concentrationModel.FractionPositives);
            Assert.AreEqual(1D, concentrationModel.FractionTrueZeros);
            Assert.AreEqual(0D, concentrationModel.FractionCensored);
        }

        /// <summary>
        /// Maximum residue limits
        /// </summary>
        [TestMethod]
        [TestCategory("Concentration Modeling Tests")]
        public void CMMaximumResidueLimitTest6() {
            var lor = 0.1;
            var positives = new List<double>();
            var mrl = 0.5;

            var residues = new CompoundResidueCollection() {
                Positives = positives,
                CensoredValuesCollection = new List<CensoredValueCollection>() { new CensoredValueCollection() { LOD = lor, LOQ = lor } }
            };

            var concentrationModel = new CMMaximumResidueLimit() {
                DesiredModelType = ConcentrationModelType.Empirical,
                NonDetectsHandlingMethod = NonDetectsHandlingMethod.ReplaceByLOR,
                FractionOfLOR = 1,
                Residues = residues,
                MaximumResidueLimit = mrl,
                WeightedAgriculturalUseFraction = 1,
                CorrectedWeightedAgriculturalUseFraction = 1,
            };

            concentrationModel.CalculateParameters();

            var seed = 1;
            var random = new McraRandomGenerator(seed);
            Assert.AreEqual(0.5, concentrationModel.DrawFromDistribution(random, concentrationModel.NonDetectsHandlingMethod));
            Assert.AreEqual(0.5, concentrationModel.DrawFromDistributionExceptZeroes(random, concentrationModel.NonDetectsHandlingMethod));
            Assert.AreEqual(0.5, concentrationModel.GetDistributionMean(concentrationModel.NonDetectsHandlingMethod));
            Assert.AreEqual(1D, concentrationModel.FractionPositives);
            Assert.AreEqual(0, concentrationModel.FractionTrueZeros);
            Assert.AreEqual(0, concentrationModel.FractionCensored);
        }

        /// <summary>
        /// Maximum residue limits
        /// </summary>
        [TestMethod]
        [TestCategory("Concentration Modeling Tests")]
        public void CMMaximumResidueLimitTest7() {
            var lor = 0.1;
            var positives = new List<double>();
            var mrl = 0.5;

            var residues = new CompoundResidueCollection() {
                Positives = positives,
                CensoredValuesCollection = new List<CensoredValueCollection>() { new CensoredValueCollection() { LOD = lor, LOQ = lor } }
            };

            var concentrationModel = new CMMaximumResidueLimit() {
                DesiredModelType = ConcentrationModelType.Empirical,
                NonDetectsHandlingMethod = NonDetectsHandlingMethod.ReplaceByLOR,
                FractionOfLOR = 1,
                Residues = residues,
                MaximumResidueLimit = mrl,
                WeightedAgriculturalUseFraction = 0.5,
                CorrectedWeightedAgriculturalUseFraction = 0.5,
            };

            concentrationModel.CalculateParameters();

            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var generatedResidues = new List<double>();
            for (int i = 0; i < 10000; i++) {
                generatedResidues.Add(concentrationModel.DrawFromDistribution(random, concentrationModel.NonDetectsHandlingMethod));
            }
            var observed = generatedResidues.Sum() / generatedResidues.Count;

            Assert.AreEqual(observed, 0.25, 1e-2);
            Assert.AreEqual(0.5, concentrationModel.DrawFromDistributionExceptZeroes(random, concentrationModel.NonDetectsHandlingMethod));
            Assert.AreEqual(0.25, concentrationModel.GetDistributionMean(concentrationModel.NonDetectsHandlingMethod));
            Assert.AreEqual(0.5, concentrationModel.FractionPositives);
            Assert.AreEqual(0.5, concentrationModel.FractionTrueZeros);
            Assert.AreEqual(0, concentrationModel.FractionCensored);
        }

        /// <summary>
        /// Maximum residue limits
        /// </summary>
        [TestMethod]
        [TestCategory("Concentration Modeling Tests")]
        public void CMMaximumResidueLimitTest8() {
            var positives = new List<double>() { 0.2 };
            var mrl = 0.5;

            var residues = new CompoundResidueCollection() {
                Positives = positives,
                CensoredValuesCollection = new List<CensoredValueCollection>(),
            };

            var concentrationModel = new CMMaximumResidueLimit() {
                DesiredModelType = ConcentrationModelType.Empirical,
                NonDetectsHandlingMethod = NonDetectsHandlingMethod.ReplaceByLOR,
                FractionOfLOR = 1,
                Residues = residues,
                MaximumResidueLimit = mrl,
            };

            concentrationModel.CalculateParameters();

            var seed = 1;
            var random = new McraRandomGenerator(seed);
            Assert.AreEqual(mrl, concentrationModel.DrawFromDistribution(random, concentrationModel.NonDetectsHandlingMethod));
            Assert.AreEqual(mrl, concentrationModel.DrawFromDistributionExceptZeroes(random, concentrationModel.NonDetectsHandlingMethod));
            Assert.AreEqual(mrl, concentrationModel.GetDistributionMean(concentrationModel.NonDetectsHandlingMethod));
            Assert.AreEqual(1, concentrationModel.FractionPositives);
            Assert.AreEqual(0, concentrationModel.FractionTrueZeros);
            Assert.AreEqual(0, concentrationModel.FractionCensored);
        }

        /// <summary>
        /// Maximum residue limits
        /// </summary>
        [TestMethod]
        [TestCategory("Concentration Modeling Tests")]
        public void CMMaximumResidueLimitTest9() {
            var lor = 0.1;
            var positives = new List<double>() { 0.2 };
            var mrl = 0.5;

            var residues = new CompoundResidueCollection() {
                Positives = positives,
                CensoredValuesCollection = new List<CensoredValueCollection>() { new CensoredValueCollection() { LOD = lor, LOQ = lor } }
            };

            var concentrationModel = new CMMaximumResidueLimit() {
                DesiredModelType = ConcentrationModelType.Empirical,
                NonDetectsHandlingMethod = NonDetectsHandlingMethod.ReplaceByLOR,
                FractionOfLOR = 1,
                FractionOfMrl = 0.5,
                Residues = residues,
                MaximumResidueLimit = mrl,
                WeightedAgriculturalUseFraction = 0.5,
                CorrectedWeightedAgriculturalUseFraction = 0.5,
            };

            concentrationModel.CalculateParameters();

            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var repetitions = 1000;
            var generatedResidues = new List<double>(repetitions);
            for (int i = 0; i < repetitions; i++) {
                generatedResidues.Add(concentrationModel.DrawFromDistribution(random, concentrationModel.NonDetectsHandlingMethod));
            }
            var observed = generatedResidues.Sum() / generatedResidues.Count;
            Assert.AreEqual(observed, 0.5 * 0.5 * mrl, 1e-2);
            Assert.AreEqual(0.5 * mrl, concentrationModel.DrawFromDistributionExceptZeroes(random, concentrationModel.NonDetectsHandlingMethod));
            Assert.AreEqual(0.5 * 0.5 * mrl, concentrationModel.GetDistributionMean(concentrationModel.NonDetectsHandlingMethod));
            Assert.AreEqual(0.5, concentrationModel.FractionPositives);
            Assert.AreEqual(0.5, concentrationModel.FractionTrueZeros);
            Assert.AreEqual(0, concentrationModel.FractionCensored);
        }
    }
}
