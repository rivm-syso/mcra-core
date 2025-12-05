using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Calculators.IntraSpeciesConversion;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;

namespace MCRA.Simulation.Test.UnitTests.Calculators.IntraSpeciesModels {

    /// <summary>
    /// EffectModelling calculatorIntraSpeciesModels
    /// </summary>
    [TestClass]
    public class IntraSpeciesFactorModelBuilderTests {

        /// <summary>
        /// Calculate percentiles based on intraspecies lower and intraspecies upper factor
        /// van der Voet et al. 2009 Food and Chemical Toxicology 2926-2940
        /// </summary>
        [TestMethod]
        public void IntraSpeciesFactorModelBuilderTests_TestCalculateParameters() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var effects = FakeEffectsGenerator.Create(1);
            var substances = FakeSubstancesGenerator.Create(1);
            var factors = substances
                .Select(r => new IntraSpeciesFactor() {
                    Effect = effects[0],
                    Compound = r,
                    LowerVariationFactor = 2,
                    UpperVariationFactor = 10,
                })
                .ToList();
            var builder = new IntraSpeciesFactorModelBuilder();
            var intraSpeciesFactorModels = builder.Create(effects, substances, factors, 10);
            var intraSpeciesFactor = intraSpeciesFactorModels[(effects.First(), substances.First())];
            var gsd = intraSpeciesFactor.GeometricStandardDeviation;
            var draw = new List<double>();
            for (int i = 0; i < 10000; i++) {
                draw.Add(Math.Exp(NormalDistribution.Draw(random, 0, 1) * Math.Log(gsd)));
            }
            var perc = draw.Percentiles([50, 95]);
            Assert.AreEqual(1D, perc[0], 10E-1);
            Assert.AreEqual(2.860, perc[1], 10E-1);
            Assert.AreEqual(6.25, intraSpeciesFactor.DegreesOfFreedom, 1E-2);
        }

        /// <summary>
        /// Tests whether the created collection contains a default model for the null
        /// substance / general substance.
        /// </summary>
        [TestMethod]
        public void IntraSpeciesFactorModelBuilderTests_TestContainsDefault() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var effects = FakeEffectsGenerator.Create(1);
            var substances = FakeSubstancesGenerator.Create(1);
            var factors = substances
                .Select(r => new IntraSpeciesFactor() {
                    Effect = effects[0],
                    Compound = r,
                    LowerVariationFactor = 2,
                    UpperVariationFactor = 10,
                })
                .ToList();
            var builder = new IntraSpeciesFactorModelBuilder();
            var intraSpeciesFactorModels = builder.Create(effects, substances, factors, 10);
            Assert.HasCount(2, intraSpeciesFactorModels);
            CollectionAssert.Contains(intraSpeciesFactorModels.Keys, ((Effect)null, (Compound)null));
        }

        /// <summary>
        /// Test resample intra-species factor geometric standard deviation for bootstrap.
        /// When bootstrapping for a number of times, we expect different GSD values and
        /// that the average GSD is equal to the nominal GSD.
        /// </summary>
        [TestMethod]
        public void IntraSpeciesFactorModelBuilderTests_TestResample() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var effects = FakeEffectsGenerator.Create(1);
            var substances = FakeSubstancesGenerator.Create(1);
            var factors = substances
                .Select(r => new IntraSpeciesFactor() {
                    Effect = effects[0],
                    Compound = r,
                    LowerVariationFactor = 2,
                    UpperVariationFactor = 3,
                })
                .ToList();
            var builder = new IntraSpeciesFactorModelBuilder();
            var intraSpeciesFactorModels = builder.Create(effects, substances, factors, 10);
            var nominalGsd = intraSpeciesFactorModels[(effects.First(), substances.First())].GeometricStandardDeviation;

            var draw = new List<double>();
            for (int i = 0; i < 10000; i++) {
                var bootstrap = builder.Resample(intraSpeciesFactorModels, random);
                draw.Add(bootstrap[(effects.First(), substances.First())].GeometricStandardDeviation);
            }
            var average = draw.Average();
            Assert.AreEqual(nominalGsd, average, 1E-1);
            Assert.IsTrue(draw.Any(r => r > nominalGsd + 1e-1));
            Assert.IsTrue(draw.Any(r => r < nominalGsd - 1e-1));
        }
    }
}
