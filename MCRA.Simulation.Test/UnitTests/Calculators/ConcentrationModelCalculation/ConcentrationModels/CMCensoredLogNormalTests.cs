using MCRA.Simulation.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.CompoundResidueCollectionCalculation;
using MCRA.Simulation.Calculators.ConcentrationModelCalculation.ConcentrationModels;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using MCRA.Utils.Statistics;

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
        public void CMCensoredLogNormal_Test1() {
            var residues = new CompoundResidueCollection() {
                Positives = [],
                CensoredValuesCollection = [],
            };
            var concentrationModel = new CMCensoredLogNormal() {
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
        public void CMCensoredLogNormal_Test2() {
            var lor = 0.1;
            var residues = new CompoundResidueCollection() {
                Positives = [],
                CensoredValuesCollection = [new CensoredValue() { LOD = lor, LOQ = lor }]
            };
            var concentrationModel = new CMCensoredLogNormal() {
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
        public void CMCensoredLogNormal_Test3() {
            var lor = 0.1;
            var residues = new CompoundResidueCollection() {
                Positives = [],
                CensoredValuesCollection = [new CensoredValue() { LOD = lor, LOQ = lor }]
            };
            var concentrationModel = new CMCensoredLogNormal() {
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
        public void CMCensoredLogNormal_Test4() {
            var lor = 0.1;
            var residues = new CompoundResidueCollection() {
                Positives = [0.1, 0.2, 0.3],
                CensoredValuesCollection = [new CensoredValue() { LOD = lor, LOQ = lor }]
            };
            var concentrationModel = new CMCensoredLogNormal() {
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
        public void CMCensoredLogNormal_Test5() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);

            var mu = .25;
            var sigma = 1;
            var residues = FakeCompoundResidueCollectionsGenerator
                .CreateSingle(
                    food: null,
                    substance: null,
                    mu: mu,
                    sigma: sigma,
                    fractionZero: 0.25,
                    treatZerosAsCensored: true,
                    lods: [0.5],
                    loqs: [0.5],
                    numberOfSamples: 100
            );

            var concentrationModel = new CMCensoredLogNormal() {
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
            Assert.IsGreaterThan(pExpectedPositives - 1.96 * sigma, pObservedPositives);
            Assert.IsLessThan(pExpectedPositives + 1.96 * sigma, pObservedPositives);

            // Test zeros
            var generatedZeros = generatedResidues.Where(s => s == 0).ToList();
            var pObservedZeros = (double)generatedZeros.Count / repetitions;
            var pExpectedZeros = 0D;
            var sigmaLors = Math.Sqrt((pExpectedZeros * (1 - pExpectedZeros)) / repetitions);
            Assert.IsGreaterThan(pExpectedZeros - 1.96 * sigma, pObservedZeros);
            Assert.IsLessThan(pExpectedZeros + 1.96 * sigma, pObservedZeros);

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
            Assert.IsGreaterThan(pExpectedPositives - 1.96 * sigma, pObservedPositives);
            Assert.IsLessThan(pExpectedPositives + 1.96 * sigma, pObservedPositives);

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
        [TestCategory("Concentration Modeling Tests")]
        public void CMCensoredLogNormal_Test6() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);

            var mu = .25;
            var sigma = 1;
            var residues = FakeCompoundResidueCollectionsGenerator
                .CreateSingle(
                    food: null,
                    substance: null,
                    mu: mu,
                    sigma: sigma,
                    fractionZero: 0.25,
                    treatZerosAsCensored: true,
                    lods: [0.5],
                    loqs: [0.5],
                    numberOfSamples: 100
            );

            var concentrationModel = new CMCensoredLogNormal() {
                WeightedAgriculturalUseFraction = 0.75,
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
            var generatedPositives = generatedResidues.Where(s => s > 0).ToList();
            var pObservedPositives = (double)generatedPositives.Count / repetitions;
            var pExpectedPositives = 0.75;
            var sigmaPositives = Math.Sqrt((pExpectedPositives * (1 - pExpectedPositives)) / repetitions);
            Assert.IsGreaterThan(pExpectedPositives - 1.96 * sigma, pObservedPositives);
            Assert.IsLessThan(pExpectedPositives + 1.96 * sigma, pObservedPositives);

            // Test lors
            var generatedLors = generatedResidues.Where(s => s == 0).ToList();
            var pObservedLors = (double)generatedLors.Count / repetitions;
            var pExpectedLors = 0.25;
            var sigmaLors = Math.Sqrt((pExpectedLors * (1 - pExpectedLors)) / repetitions);
            Assert.IsGreaterThan(pExpectedLors - 1.96 * sigma, pObservedLors);
            Assert.IsLessThan(pExpectedLors + 1.96 * sigma, pObservedLors);

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
            Assert.IsGreaterThan(pExpectedPositives - 1.96 * sigma, pObservedPositives);
            Assert.IsLessThan(pExpectedPositives + 1.96 * sigma, pObservedPositives);

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
        /// Test fit of censored log normal model from known distribution.
        /// </summary>
        [TestMethod]
        [DataRow(0.5D)]
        [DataRow(1D)]
        public void CMCensoredLogNormal_Test7(double useFraction) {
            var lod = 0.1;
            var loq = 0.2;
            var n = 10000;
            var rnd = new McraRandomGenerator(1);
            var values = LogNormalDistribution.Samples(rnd, 0, 1, (int)(useFraction * 10000));
            values.AddRange(Enumerable.Repeat(0D, (int)((1 - useFraction) * n)));
            var residues = new CompoundResidueCollection() {
                Positives = values.Where(r => r > loq).ToList(),
                CensoredValuesCollection = values
                    .Where(r => r <= loq)
                    .Select(r => new CensoredValue() {
                        ResType = r < lod ? ResType.LOD : ResType.LOQ,
                        LOD = lod,
                        LOQ = loq
                    })
                    .ToList(),
            };

            var concentrationModel = new CMCensoredLogNormal() {
                Residues = residues,
                CorrectedWeightedAgriculturalUseFraction = useFraction,
            };

            Assert.IsTrue(concentrationModel.CalculateParameters());
            Assert.AreEqual(0, concentrationModel.Mu, 1e-2);
            Assert.AreEqual(1, concentrationModel.Sigma, 1e-1);
        }

        [TestMethod]
        [TestCategory("Concentration Modeling Tests")]
        [DataRow(ResType.LOD)]
        [DataRow(ResType.LOQ)]
        public void CMCensoredLogNormal_TestGetImputedCensoredValue(ResType resType) {
            var seed = 1;
            var random = new McraRandomGenerator(seed);

            // Create censored lognormal model
            var concentrationModel = new CMCensoredLogNormal() {
                Mu = 0,
                Sigma = 1
            };
            var lod = 0.1;
            var loq = 0.3;

            // Create a sample substance representing a non-detect measurement
            var sampleSubstance = new SampleCompound() {
                Lod = lod,
                Loq = loq,
                ResType = resType
            };
            var n = 100000;
            var draws = Enumerable
                .Range(0, n)
                .Select(r => concentrationModel.GetImputedCensoredValue(sampleSubstance, random))
                .ToList();

            var lower = resType == ResType.LOQ ? lod : 0;
            var upper = resType == ResType.LOQ ? loq : lod;

            // Assert that we draw distinct values somewhat in the interval [lower,upper]
            Assert.IsGreaterThan(n - 10, draws.Distinct().Count());
            Assert.IsGreaterThan(lower + 0.9 * (upper - lower), draws.Max());
            Assert.IsLessThan(lower + 0.1 * (upper - lower), draws.Min());
            Assert.IsTrue(draws.All(r => r > lower && r < upper));
        }
    }
}
