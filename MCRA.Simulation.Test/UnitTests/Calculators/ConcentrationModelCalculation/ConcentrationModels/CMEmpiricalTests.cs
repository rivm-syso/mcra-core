using MCRA.Utils.ExtensionMethods;
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
    public class CMEmpiricalTests {

        /// <summary>
        /// Without data the model should always give 0 as result and the fractions positives/censored/truezeroes should be NaN
        /// </summary>
        [TestMethod]
        [TestCategory("Concentration Modeling Tests")]
        public void CMEmpiricalTest1() {
            var residues = new CompoundResidueCollection() {
                Positives = new List<double>(),
                CensoredValuesCollection = new List<CensoredValueCollection>(),
            };
            var concentrationModel = new CMEmpirical() {
                DesiredModelType = ConcentrationModelType.Empirical,
                NonDetectsHandlingMethod = NonDetectsHandlingMethod.ReplaceByLOR,
                WeightedAgriculturalUseFraction = residues.FractionPositives,
                CorrectedWeightedAgriculturalUseFraction = residues.FractionPositives,
                FractionOfLOR = 1,
                Residues = residues,
            };
            concentrationModel.CalculateParameters();
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            Assert.AreEqual(0D, concentrationModel.DrawFromDistribution(random, concentrationModel.NonDetectsHandlingMethod));
            Assert.AreEqual(0D, concentrationModel.DrawAccordingToNonDetectsHandlingMethod(random, concentrationModel.NonDetectsHandlingMethod, 1D));
            Assert.AreEqual(0D, concentrationModel.DrawFromDistributionExceptZeroes(random, concentrationModel.NonDetectsHandlingMethod));
            Assert.AreEqual(0D, concentrationModel.GetDistributionMean(concentrationModel.NonDetectsHandlingMethod));
            Assert.AreEqual(double.NaN, concentrationModel.FractionPositives);
            Assert.AreEqual(double.NaN, concentrationModel.FractionTrueZeros);
            Assert.AreEqual(double.NaN, concentrationModel.FractionCensored);
        }

        /// <summary>
        /// With only a nondetect, the Replace by LOR method and 0% agricultural use the model should always give 0 as result, 
        /// except for the DrawAccordingToNonDetectsHandlingMethod where the result should equal the LOR. 
        /// The fractions positives/censored/truezeroes should be 0/0/1
        /// </summary>
        [TestMethod]
        [TestCategory("Concentration Modeling Tests")]
        public void CMEmpiricalTest2a() {
            var lor = 0.1;

            var residues = new CompoundResidueCollection() {
                Positives = new List<double>(),
                CensoredValuesCollection = new List<CensoredValueCollection>() { new CensoredValueCollection() { LOD = lor, LOQ = lor } }
            };

            var concentrationModel = new CMEmpirical() {
                DesiredModelType = ConcentrationModelType.Empirical,
                NonDetectsHandlingMethod = NonDetectsHandlingMethod.ReplaceByLOR,
                FractionOfLOR = 1,
                WeightedAgriculturalUseFraction = 0,
                CorrectedWeightedAgriculturalUseFraction = 0,
                Residues = residues,
            };

            concentrationModel.CalculateParameters();

            var seed = 1;
            var random = new McraRandomGenerator(seed);
            Assert.AreEqual(0, concentrationModel.DrawFromDistribution(random, concentrationModel.NonDetectsHandlingMethod));
            Assert.AreEqual(lor, concentrationModel.DrawAccordingToNonDetectsHandlingMethod(random, concentrationModel.NonDetectsHandlingMethod, 1D));
            Assert.AreEqual(0, concentrationModel.DrawFromDistributionExceptZeroes(random, concentrationModel.NonDetectsHandlingMethod));
            Assert.AreEqual(0, concentrationModel.GetDistributionMean(concentrationModel.NonDetectsHandlingMethod));
            Assert.AreEqual(0, concentrationModel.FractionPositives);
            Assert.AreEqual(1, concentrationModel.FractionTrueZeros);
            Assert.AreEqual(0, concentrationModel.FractionCensored);

        }

        /// <summary>
        /// With only a nondetect, the ReplaceByLOQLODSystem method and 0% agricultural use the model should always give 0 as result, 
        /// except for the DrawAccordingToNonDetectsHandlingMethod where the result should equal the LOD. 
        /// The fractions positives/censored/truezeroes should be 0/0/1
        /// </summary>
        [TestMethod]
        [TestCategory("Concentration Modeling Tests")]
        public void CMEmpiricalTest2b() {
            var lod = 0.1;

            var residues = new CompoundResidueCollection() {
                Positives = new List<double>(),
                CensoredValuesCollection = new List<CensoredValueCollection>() { new CensoredValueCollection() { LOD = lod, LOQ = lod, ResType = ResType.LOD } }
            };

            var concentrationModel = new CMEmpirical() {
                DesiredModelType = ConcentrationModelType.Empirical,
                NonDetectsHandlingMethod = NonDetectsHandlingMethod.ReplaceByLODLOQSystem,
                FractionOfLOR = 1,
                WeightedAgriculturalUseFraction = 0,
                CorrectedWeightedAgriculturalUseFraction = 0,
                Residues = residues,
            };

            concentrationModel.CalculateParameters();

            var seed = 1;
            var random = new McraRandomGenerator(seed);
            Assert.AreEqual(0, concentrationModel.DrawFromDistribution(random, concentrationModel.NonDetectsHandlingMethod));
            Assert.AreEqual(lod, concentrationModel.DrawAccordingToNonDetectsHandlingMethod(random, concentrationModel.NonDetectsHandlingMethod, 1D));
            Assert.AreEqual(0, concentrationModel.DrawFromDistributionExceptZeroes(random, concentrationModel.NonDetectsHandlingMethod));
            Assert.AreEqual(0, concentrationModel.GetDistributionMean(concentrationModel.NonDetectsHandlingMethod));
            Assert.AreEqual(0, concentrationModel.FractionPositives);
            Assert.AreEqual(1, concentrationModel.FractionTrueZeros);
            Assert.AreEqual(0, concentrationModel.FractionCensored);

        }
        /// <summary>
        /// With only a nondetect, the ReplaceByLOQLODSystem method and 0% agricultural use the model should always give 0 as result, 
        /// except for the DrawAccordingToNonDetectsHandlingMethod where the result should equal the LOD + (LOQ-LOD). 
        /// The fractions positives/censored/truezeroes should be 0/0/1
        /// </summary>
        [TestMethod]
        [TestCategory("Concentration Modeling Tests")]
        public void CMEmpiricalTest2c() {
            var lod = 0.1;
            var loq = 0.2;

            var residues = new CompoundResidueCollection() {
                Positives = new List<double>(),
                CensoredValuesCollection = new List<CensoredValueCollection>() { new CensoredValueCollection() { LOD = lod, LOQ = loq, ResType = ResType.LOQ } }
            };

            var concentrationModel = new CMEmpirical() {
                DesiredModelType = ConcentrationModelType.Empirical,
                NonDetectsHandlingMethod = NonDetectsHandlingMethod.ReplaceByLODLOQSystem,
                FractionOfLOR = 1,
                WeightedAgriculturalUseFraction = 0,
                CorrectedWeightedAgriculturalUseFraction = 0,
                Residues = residues,
            };

            concentrationModel.CalculateParameters();

            var seed = 1;
            var random = new McraRandomGenerator(seed);
            Assert.AreEqual(0, concentrationModel.DrawFromDistribution(random, concentrationModel.NonDetectsHandlingMethod));
            Assert.AreEqual(loq, concentrationModel.DrawAccordingToNonDetectsHandlingMethod(random, concentrationModel.NonDetectsHandlingMethod, 1D));
            Assert.AreEqual(0, concentrationModel.DrawFromDistributionExceptZeroes(random, concentrationModel.NonDetectsHandlingMethod));
            Assert.AreEqual(0, concentrationModel.GetDistributionMean(concentrationModel.NonDetectsHandlingMethod));
            Assert.AreEqual(0, concentrationModel.FractionPositives);
            Assert.AreEqual(1, concentrationModel.FractionTrueZeros);
            Assert.AreEqual(0, concentrationModel.FractionCensored);

        }

        /// <summary>
        /// With only a nondetect, the Replace by LOR method and 100% agricultural use the model should always give the LOR as result. 
        /// The fractions positives/censored/truezeroes should be 0/0/1
        /// </summary>
        [TestMethod]
        [TestCategory("Concentration Modeling Tests")]
        public void CMEmpiricalTest3a() {
            var lor = 0.1;

            var residues = new CompoundResidueCollection() {
                Positives = new List<double>(),
                CensoredValuesCollection = new List<CensoredValueCollection>() { new CensoredValueCollection() { LOD = lor, LOQ = lor } }
            };

            var concentrationModel = new CMEmpirical() {
                DesiredModelType = ConcentrationModelType.Empirical,
                NonDetectsHandlingMethod = NonDetectsHandlingMethod.ReplaceByLOR,
                FractionOfLOR = 1,
                Residues = residues,
            };

            concentrationModel.CalculateParameters();
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            Assert.AreEqual(lor, concentrationModel.DrawFromDistribution(random, concentrationModel.NonDetectsHandlingMethod));
            Assert.AreEqual(lor, concentrationModel.DrawAccordingToNonDetectsHandlingMethod(random, concentrationModel.NonDetectsHandlingMethod, 1D));
            Assert.AreEqual(lor, concentrationModel.DrawFromDistributionExceptZeroes(random, concentrationModel.NonDetectsHandlingMethod));
            Assert.AreEqual(lor, concentrationModel.GetDistributionMean(concentrationModel.NonDetectsHandlingMethod));
            Assert.AreEqual(0, concentrationModel.FractionPositives);
            Assert.AreEqual(0, concentrationModel.FractionTrueZeros);
            Assert.AreEqual(1, concentrationModel.FractionCensored);

        }

        /// <summary>
        /// With only a nondetect, the ReplaceByLOQLODSystem method and 100% agricultural use the model should always give the LOD
        /// </summary>
        [TestMethod]
        [TestCategory("Concentration Modeling Tests")]
        public void CMEmpiricalTest3b() {
            var lod = 0.1;

            var residues = new CompoundResidueCollection() {
                Positives = new List<double>(),
                CensoredValuesCollection = new List<CensoredValueCollection>() { new CensoredValueCollection() { LOD = lod, ResType = ResType.LOD } }
            };

            var concentrationModel = new CMEmpirical() {
                DesiredModelType = ConcentrationModelType.Empirical,
                NonDetectsHandlingMethod = NonDetectsHandlingMethod.ReplaceByLODLOQSystem,
                FractionOfLOR = 1,
                Residues = residues,
            };

            concentrationModel.CalculateParameters();
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            Assert.AreEqual(lod, concentrationModel.DrawFromDistribution(random, concentrationModel.NonDetectsHandlingMethod));
            Assert.AreEqual(lod, concentrationModel.DrawAccordingToNonDetectsHandlingMethod(random, concentrationModel.NonDetectsHandlingMethod, 1D));
            Assert.AreEqual(lod, concentrationModel.DrawFromDistributionExceptZeroes(random, concentrationModel.NonDetectsHandlingMethod));
            Assert.AreEqual(lod, concentrationModel.GetDistributionMean(concentrationModel.NonDetectsHandlingMethod));
            Assert.AreEqual(0, concentrationModel.FractionPositives);
            Assert.AreEqual(0, concentrationModel.FractionTrueZeros);
            Assert.AreEqual(1, concentrationModel.FractionCensored);

        } /// <summary>
          /// With only a nondetect, the ReplaceByLOQLODSystem method and 100% agricultural use the model should always give the LOQ as result. 
          /// The fractions positives/censored/truezeroes should be 0/0/1
          /// </summary>
        [TestMethod]
        [TestCategory("Concentration Modeling Tests")]
        public void CMEmpiricalTest3() {
            var lod = 0.1;
            var loq = 0.2;

            var residues = new CompoundResidueCollection() {
                Positives = new List<double>(),
                CensoredValuesCollection = new List<CensoredValueCollection>() { new CensoredValueCollection() { LOD = lod, LOQ = loq, ResType = ResType.LOQ } }
            };

            var concentrationModel = new CMEmpirical() {
                DesiredModelType = ConcentrationModelType.Empirical,
                NonDetectsHandlingMethod = NonDetectsHandlingMethod.ReplaceByLOR,
                FractionOfLOR = 1,
                Residues = residues,
            };

            concentrationModel.CalculateParameters();
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            Assert.AreEqual(loq, concentrationModel.DrawFromDistribution(random, concentrationModel.NonDetectsHandlingMethod));
            Assert.AreEqual(loq, concentrationModel.DrawAccordingToNonDetectsHandlingMethod(random, concentrationModel.NonDetectsHandlingMethod, 1D));
            Assert.AreEqual(loq, concentrationModel.DrawFromDistributionExceptZeroes(random, concentrationModel.NonDetectsHandlingMethod));
            Assert.AreEqual(loq, concentrationModel.GetDistributionMean(concentrationModel.NonDetectsHandlingMethod));
            Assert.AreEqual(0, concentrationModel.FractionPositives);
            Assert.AreEqual(0, concentrationModel.FractionTrueZeros);
            Assert.AreEqual(1, concentrationModel.FractionCensored);

        }

        /// <summary>
        /// With only a nondetect, the Replace by LOR method and 50% agricultural use the model should give ca. 50% LORs.
        /// The methods DrawAccordingToNonDetectsHandlingMethod and DrawFromDistributionExceptZeroes should give LOR.
        /// The method GetDistributionMean should give LOR/2.
        /// The fractions positives/censored/truezeroes should be 0/0.5/0.5
        /// </summary>
        [TestMethod]
        [TestCategory("Concentration Modeling Tests")]
        public void CMEmpiricalTest4() {
            var lor = 0.1;

            var residues = new CompoundResidueCollection() {
                Positives = new List<double>(),
                CensoredValuesCollection = new List<CensoredValueCollection>() { new CensoredValueCollection() { LOD = lor, LOQ = lor } }
            };

            var concentrationModel = new CMEmpirical() {
                DesiredModelType = ConcentrationModelType.Empirical,
                NonDetectsHandlingMethod = NonDetectsHandlingMethod.ReplaceByLOR,
                FractionOfLOR = 1,
                WeightedAgriculturalUseFraction = .5,
                CorrectedWeightedAgriculturalUseFraction = .5,
                Residues = residues,
            };

            concentrationModel.CalculateParameters();

            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var repetitions = 1000;
            var generatedResidues = new List<double>(repetitions);
            for (int i = 0; i < repetitions; i++) {
                generatedResidues.Add(concentrationModel.DrawFromDistribution(random, concentrationModel.NonDetectsHandlingMethod));
            }
            var observed = generatedResidues.Count(r => r == lor);
            var pObserved = (double)observed / repetitions;
            var pExpected = concentrationModel.CorrectedWeightedAgriculturalUseFraction;
            var sigma = Math.Sqrt((pExpected * (1 - pExpected)) / repetitions) + 1e-10;
            Assert.IsTrue(pObserved > pExpected - 1.96 * sigma);
            Assert.IsTrue(pObserved < pExpected + 1.96 * sigma);

            Assert.AreEqual(lor, concentrationModel.DrawAccordingToNonDetectsHandlingMethod(random, concentrationModel.NonDetectsHandlingMethod, 1D));
            Assert.AreEqual(lor, concentrationModel.DrawFromDistributionExceptZeroes(random, concentrationModel.NonDetectsHandlingMethod));
            Assert.AreEqual(lor / 2, concentrationModel.GetDistributionMean(concentrationModel.NonDetectsHandlingMethod));
            Assert.AreEqual(0, concentrationModel.FractionPositives);
            Assert.AreEqual(.5, concentrationModel.FractionTrueZeros);
            Assert.AreEqual(.5, concentrationModel.FractionCensored);
        }

        /// <summary>
        /// With only 1 nondetect and 1 positive, the Replace by LOR method and 100% agricultural use the model should give ca. 50% LORs and ca. 50% positives.
        /// The methods DrawAccordingToNonDetectsHandlingMethod should give LOR.
        /// The method GetDistributionMean should give the mean of the lor and the positive..
        /// The fractions positives/censored/truezeroes should be 0.5/0.5/0
        /// </summary>
        [TestMethod]
        [TestCategory("Concentration Modeling Tests")]
        public void CMEmpiricalTest5() {
            var lor = 0.1;
            var pos = 0.2;

            var residues = new CompoundResidueCollection() {
                Positives = new List<double>() { pos },
                CensoredValuesCollection = new List<CensoredValueCollection>() { new CensoredValueCollection() { LOD = lor, LOQ = lor } }
            };

            var concentrationModel = new CMEmpirical() {
                DesiredModelType = ConcentrationModelType.Empirical,
                NonDetectsHandlingMethod = NonDetectsHandlingMethod.ReplaceByLOR,
                FractionOfLOR = 1,
                Residues = residues,
            };

            concentrationModel.CalculateParameters();

            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var repetitions = 1000;
            var generatedResidues = new List<double>(repetitions);
            for (int i = 0; i < repetitions; i++) {
                generatedResidues.Add(concentrationModel.DrawFromDistribution(random, concentrationModel.NonDetectsHandlingMethod));
            }

            // Test positives
            var observed = generatedResidues.Count(r => r == pos);
            var pObserved = (double)observed / repetitions;
            var pExpected = 0.5;
            var sigma = Math.Sqrt((pExpected * (1 - pExpected)) / repetitions);
            Assert.IsTrue(pObserved > pExpected - 1.96 * sigma);
            Assert.IsTrue(pObserved < pExpected + 1.96 * sigma);

            // Test lors
            observed = generatedResidues.Count(r => r == lor);
            pObserved = (double)observed / repetitions;
            pExpected = 0.5;
            sigma = Math.Sqrt((pExpected * (1 - pExpected)) / repetitions);
            Assert.IsTrue(pObserved > pExpected - 1.96 * sigma);
            Assert.IsTrue(pObserved < pExpected + 1.96 * sigma);

            // Draw from positives or censored
            generatedResidues = new List<double>(repetitions);
            for (int i = 0; i < repetitions; i++) {
                generatedResidues.Add(concentrationModel.DrawFromDistributionExceptZeroes(random, concentrationModel.NonDetectsHandlingMethod));
            }

            // Test positives
            observed = generatedResidues.Count(r => r == pos);
            pObserved = (double)observed / repetitions;
            pExpected = 0.5;
            sigma = Math.Sqrt((pExpected * (1 - pExpected)) / repetitions);
            Assert.IsTrue(pObserved > pExpected - 1.96 * sigma);
            Assert.IsTrue(pObserved < pExpected + 1.96 * sigma);

            // Test zeros
            observed = generatedResidues.Count(r => r == lor);
            pObserved = (double)observed / repetitions;
            pExpected = 0.5;
            sigma = Math.Sqrt((pExpected * (1 - pExpected)) / repetitions);
            Assert.IsTrue(pObserved > pExpected - 1.96 * sigma);
            Assert.IsTrue(pObserved < pExpected + 1.96 * sigma);

            Assert.AreEqual(lor, concentrationModel.DrawAccordingToNonDetectsHandlingMethod(random, concentrationModel.NonDetectsHandlingMethod, 1D));
            Assert.AreEqual((pos + lor) / 2, concentrationModel.GetDistributionMean(concentrationModel.NonDetectsHandlingMethod));
            Assert.AreEqual(.5, concentrationModel.FractionPositives);
            Assert.AreEqual(0, concentrationModel.FractionTrueZeros);
            Assert.AreEqual(.5, concentrationModel.FractionCensored);

        }

        /// <summary>
        /// With only 1 nondetect and 1 positive, the Replace by LOR method and 50% agricultural use the model should give ca. 50% LORs and ca. 50% positives.
        /// The methods DrawAccordingToNonDetectsHandlingMethod should give LOR.
        /// The methods DrawFromDistributionExceptZeroes should give the positive.
        /// The method GetDistributionMean should give the positive/2.
        /// The fractions positives/censored/truezeroes should be 0.5/0/0.5
        /// </summary>
        [TestMethod]
        [TestCategory("Concentration Modeling Tests")]
        public void CMEmpiricalTest6() {
            var lor = 0.1;
            var pos = 0.2;
            var positives = new List<double>() { pos };

            var residues = new CompoundResidueCollection() {
                Positives = positives,
                CensoredValuesCollection = new List<CensoredValueCollection>() { new CensoredValueCollection() { LOD = lor, LOQ = lor } }
            };

            var concentrationModel = new CMEmpirical() {
                DesiredModelType = ConcentrationModelType.Empirical,
                NonDetectsHandlingMethod = NonDetectsHandlingMethod.ReplaceByLOR,
                FractionOfLOR = 1,
                Residues = residues,
                WeightedAgriculturalUseFraction = .5,
                CorrectedWeightedAgriculturalUseFraction = .5
            };

            concentrationModel.CalculateParameters();

            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var repetitions = 1000;
            var generatedResidues = new List<double>(repetitions);
            for (int i = 0; i < repetitions; i++) {
                generatedResidues.Add(concentrationModel.DrawFromDistribution(random, concentrationModel.NonDetectsHandlingMethod));
            }

            // Test positives
            var observed = generatedResidues.Count(r => r == pos);
            var pObserved = (double)observed / repetitions;
            var pExpected = 0.5;
            var sigma = Math.Sqrt((pExpected * (1 - pExpected)) / repetitions);
            Assert.IsTrue(pObserved > pExpected - 1.96 * sigma);
            Assert.IsTrue(pObserved < pExpected + 1.96 * sigma);

            // Test zeros
            observed = generatedResidues.Count(r => r == 0);
            pObserved = (double)observed / repetitions;
            pExpected = 0.5;
            sigma = Math.Sqrt((pExpected * (1 - pExpected)) / repetitions);
            Assert.IsTrue(pObserved > pExpected - 1.96 * sigma);
            Assert.IsTrue(pObserved < pExpected + 1.96 * sigma);

            Assert.AreEqual(lor, concentrationModel.DrawAccordingToNonDetectsHandlingMethod(random, concentrationModel.NonDetectsHandlingMethod, 1D));
            Assert.AreEqual(pos, concentrationModel.DrawFromDistributionExceptZeroes(random, concentrationModel.NonDetectsHandlingMethod));
            Assert.AreEqual(pos / 2, concentrationModel.GetDistributionMean(concentrationModel.NonDetectsHandlingMethod));
            Assert.AreEqual(.5, concentrationModel.FractionPositives);
            Assert.AreEqual(.5, concentrationModel.FractionTrueZeros);
            Assert.AreEqual(0, concentrationModel.FractionCensored);

        }

        /// <summary>
        /// With only 1 nondetect and 1 positive, the Replace by LOR method and 75% agricultural use the model should give ca. 25% LORs and ca. 50% positives.
        /// The methods DrawAccordingToNonDetectsHandlingMethod should give LOR.
        /// The method GetDistributionMean should give the 0.5*positive+0.25*LOR.
        /// The fractions positives/censored/truezeroes should be 0.5/0.25/0.25
        /// </summary>
        [TestMethod]
        [TestCategory("Concentration Modeling Tests")]
        public void CMEmpiricalTest7() {
            var lor = 0.1;
            var pos = 0.2;
            var positives = new List<double>() { pos };

            var residues = new CompoundResidueCollection() {
                Positives = positives,
                CensoredValuesCollection = new List<CensoredValueCollection>() { new CensoredValueCollection() { LOD = lor, LOQ = lor } }
            };

            var concentrationModel = new CMEmpirical() {
                DesiredModelType = ConcentrationModelType.Empirical,
                NonDetectsHandlingMethod = NonDetectsHandlingMethod.ReplaceByLOR,
                WeightedAgriculturalUseFraction = .75,
                CorrectedWeightedAgriculturalUseFraction = .75,
                FractionOfLOR = 1,
                Residues = residues,
            };

            concentrationModel.CalculateParameters();

            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var repetitions = 1000;

            //==========================================
            // DrawFromDistribution
            //==========================================

            var generatedResidues = new List<double>(repetitions);
            for (int i = 0; i < repetitions; i++) {
                generatedResidues.Add(concentrationModel.DrawFromDistribution(random, concentrationModel.NonDetectsHandlingMethod));
            }

            // Test positives
            var observed = generatedResidues.Count(r => r == pos);
            var pObserved = (double)observed / repetitions;
            var pExpected = 0.5;
            var sigma = Math.Sqrt((pExpected * (1 - pExpected)) / repetitions);
            Assert.IsTrue(pObserved > pExpected - 1.96 * sigma);
            Assert.IsTrue(pObserved < pExpected + 1.96 * sigma);

            // Test lors
            observed = generatedResidues.Count(r => r == lor);
            pObserved = (double)observed / repetitions;
            pExpected = 0.25;
            sigma = Math.Sqrt((pExpected * (1 - pExpected)) / repetitions);
            Assert.IsTrue(pObserved > pExpected - 1.96 * sigma);
            Assert.IsTrue(pObserved < pExpected + 1.96 * sigma);

            //==========================================
            // DrawFromCensoredOrPositives
            //==========================================

            generatedResidues = new List<double>(repetitions);
            for (int i = 0; i < repetitions; i++) {
                generatedResidues.Add(concentrationModel.DrawFromDistributionExceptZeroes(random, concentrationModel.NonDetectsHandlingMethod));
            }

            // Test positives
            observed = generatedResidues.Count(r => r == pos);
            pObserved = (double)observed / repetitions;
            pExpected = 2D / 3;
            sigma = Math.Sqrt((pExpected * (1 - pExpected)) / repetitions);
            Assert.IsTrue(pObserved > pExpected - 1.96 * sigma);
            Assert.IsTrue(pObserved < pExpected + 1.96 * sigma);

            // Test zeros
            observed = generatedResidues.Count(r => r == lor);
            pObserved = (double)observed / repetitions;
            pExpected = 1D / 3;
            sigma = Math.Sqrt((pExpected * (1 - pExpected)) / repetitions);
            Assert.IsTrue(pObserved > pExpected - 1.96 * sigma);
            Assert.IsTrue(pObserved < pExpected + 1.96 * sigma);

            //==========================================
            // Other
            //==========================================

            Assert.AreEqual(lor, concentrationModel.DrawAccordingToNonDetectsHandlingMethod(random, concentrationModel.NonDetectsHandlingMethod, 1D));
            Assert.AreEqual(0.5 * pos + 0.25 * lor, concentrationModel.GetDistributionMean(concentrationModel.NonDetectsHandlingMethod));
            Assert.AreEqual(0.5, concentrationModel.FractionPositives);
            Assert.AreEqual(0.25, concentrationModel.FractionTrueZeros);
            Assert.AreEqual(0.25, concentrationModel.FractionCensored);

        }
        /// <summary>
        /// Concentration modelling empirical
        /// </summary>
        [TestMethod]
        [TestCategory("Concentration Modeling Tests")]
        public void CMEmpiricalTest8a() {
            var positives = new List<double> { 2, 2, 2, 3, 3, 3, 4, 4, 4 };
            var nonDetects = new List<double> { 1, 1, 1, 1, 1, 1, 1, 1, 1 };
            var nonDetectsCollection = nonDetects.Select(c => new CensoredValueCollection() { LOD = c, LOQ = c }).ToList();

            var residues = new CompoundResidueCollection() {
                Positives = positives,
                CensoredValuesCollection = nonDetectsCollection
            };

            var concentrationModel = new CMEmpirical() {
                DesiredModelType = ConcentrationModelType.Empirical,
                NonDetectsHandlingMethod = NonDetectsHandlingMethod.ReplaceByLOR,
                FractionOfLOR = 1,
                Residues = residues,
            };

            concentrationModel.CalculateParameters();

            var repetitions = 10000;
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var simulatedResidues = new List<double>(repetitions);
            for (int i = 0; i < repetitions; i++) {
                simulatedResidues.Add(concentrationModel.DrawFromDistribution(random, concentrationModel.NonDetectsHandlingMethod));
            }
            var mean = simulatedResidues.AverageOrZero();

            var tolerance = 0.05;
            Assert.AreEqual(2, mean, tolerance);
        }

        /// <summary>
        /// Concentration modelling empirical
        /// </summary>
        [TestMethod]
        [TestCategory("Concentration Modeling Tests")]
        public void CMEmpiricalTest8b() {
            var positives = new List<double> { 2, 2, 2, 3, 3, 3, 4, 4, 4 };
            var nonDetects = new List<double> { 1, 1, 1, 1, 1, 1, 1, 1, 1 };
            var nonDetectsCollection = nonDetects.Select(c => new CensoredValueCollection() { LOD = c, LOQ = c, ResType = ResType.LOD }).ToList();

            var residues = new CompoundResidueCollection() {
                Positives = positives,
                CensoredValuesCollection = nonDetectsCollection
            };

            var concentrationModel = new CMEmpirical() {
                DesiredModelType = ConcentrationModelType.Empirical,
                NonDetectsHandlingMethod = NonDetectsHandlingMethod.ReplaceByLODLOQSystem,
                FractionOfLOR = 1,
                Residues = residues,
            };

            concentrationModel.CalculateParameters();

            var repetitions = 10000;
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var simulatedResidues = new List<double>(repetitions);
            for (int i = 0; i < repetitions; i++) {
                simulatedResidues.Add(concentrationModel.DrawFromDistribution(random, concentrationModel.NonDetectsHandlingMethod));
            }
            var mean = simulatedResidues.AverageOrZero();
            var averageMean = concentrationModel.GetDistributionMean(concentrationModel.NonDetectsHandlingMethod);

            var tolerance = 0.05;
            Assert.AreEqual(2, mean, tolerance);
            Assert.AreEqual(2, averageMean, tolerance);
        }
        /// <summary>
        /// Concentration modelling empirical
        /// </summary>
        [TestMethod]
        [TestCategory("Concentration Modeling Tests")]
        public void CMEmpiricalTest8c() {
            var positives = new List<double> { 2, 2, 2, 3, 3, 3, 4, 4, 4 };
            var nonDetects = new List<double> { 1, 1, 1, 1, 1, 1, 1, 1, 1 };
            var nonDetectsCollection = nonDetects.Select((c, ix) => new CensoredValueCollection() { LOD = c, LOQ = 2 * c, ResType = ix % 2 == 0 ? ResType.LOD : ResType.LOQ }).ToList();

            var residues = new CompoundResidueCollection() {
                Positives = positives,
                CensoredValuesCollection = nonDetectsCollection
            };

            var concentrationModel = new CMEmpirical() {
                DesiredModelType = ConcentrationModelType.Empirical,
                NonDetectsHandlingMethod = NonDetectsHandlingMethod.ReplaceByLODLOQSystem,
                FractionOfLOR = 1,
                Residues = residues,
            };

            concentrationModel.CalculateParameters();

            var repetitions = 10000;
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var simulatedResidues = new List<double>(repetitions);
            for (int i = 0; i < repetitions; i++) {
                simulatedResidues.Add(concentrationModel.DrawFromDistribution(random, concentrationModel.NonDetectsHandlingMethod));
            }
            var mean = simulatedResidues.AverageOrZero();
            var averageMean = concentrationModel.GetDistributionMean(concentrationModel.NonDetectsHandlingMethod);

            var tolerance = 0.05;
            Assert.AreEqual(2.22, mean, tolerance);
            Assert.AreEqual(2.22, averageMean, tolerance);
        }
    }
}
