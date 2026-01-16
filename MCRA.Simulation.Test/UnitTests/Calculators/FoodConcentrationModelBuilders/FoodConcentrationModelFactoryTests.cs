using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.CompoundResidueCollectionCalculation;
using MCRA.Simulation.Calculators.FoodConcentrationModelBuilders;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Helpers;
using MCRA.Simulation.Test.Mock.MockCalculatorSettings;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Test.UnitTests.Calculators.FoodConcentrationModelBuilders {
    /// <summary>
    /// ResidueGeneration calculator
    /// </summary>
    [TestClass]
    public class FoodConcentrationModelFactoryTests {
        /// <summary>
        /// Creates concentration model charts
        /// </summary>
        [TestMethod]
        public void FoodConcentrationModelFactory_TestCreateCharts() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var outputPath = TestUtilities.ConcatWithOutputPath("TestCreateConcentrationModels");
            if (Directory.Exists(outputPath)) {
                Directory.Delete(outputPath, true);
                Thread.Sleep(100);
            }
            Directory.CreateDirectory(outputPath);

            var sampleSizes = new int[] { 1, 2, 3 };
            var useFractions = new double[] { 0, 0.25, 0.6, 1 };
            var mus = new double[] { 1, 2 };
            var sigmas = new double[] { 0.1, 1 };
            var lors = new double[] { 0.1, 0.5, 1, 2 };
            var concentrationModelTypes = Enum.GetValues(typeof(ConcentrationModelType))
                .Cast<ConcentrationModelType>()
                .Where(cm => cm != ConcentrationModelType.LogNormal);

            var food = new Food("MyFood");
            var compound = new Compound("MyCompound");
            foreach (var sampleSize in sampleSizes) {
                foreach (var mu in mus) {
                    foreach (var sigma in sigmas) {
                        foreach (var useFraction in useFractions) {
                            foreach (var lor in lors) {
                                var concentrations = createConcentrations(mu, sigma, useFraction, sampleSize, random);
                                var compoundResidueCollection = createConcentrations(food, compound, concentrations, lor, random);
                                var settings = new MockConcentrationModelCalculationSettings() {
                                    NonDetectsHandlingMethod = NonDetectsHandlingMethod.ReplaceByLOR,
                                    FractionOfLor = 1d,
                                };
                                var factory = new FoodConcentrationModelFactory(settings);
                                foreach (var modelType in concentrationModelTypes) {
                                    var occurrenceFraction = Math.Min(
                                        1D - compoundResidueCollection.FractionZeros,
                                        compoundResidueCollection.FractionPositives + .5 * compoundResidueCollection.FractionCensoredValues
                                    );
                                    var model = factory.CreateModelAndCalculateParameters(food, compound, modelType, compoundResidueCollection, null, null, occurrenceFraction, ConcentrationUnit.mgPerKg);
                                    Assert.IsNotNull(model);
                                    if (model.ModelType == modelType) {
                                        var record = new ConcentrationModelRecord();
                                        record.Summarize(food, compound, model, false);
                                        var chartCreator = new ConcentrationModelChartCreator(record, 300, 300, true);
                                        var id = $"{sampleSize}-{mu}-{sigma}-{lor}-{useFraction}-{modelType}".Replace(".", "p");
                                        var filename = Path.Combine(outputPath, $"{id}.png");
                                        chartCreator.CreateToPng(filename);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Creates concentration model charts
        /// </summary>
        [TestMethod]
        public void FoodConcentrationModelFactory_CreateChartsDocumentation() {
            var outputPath = TestUtilities.ConcatWithOutputPath("CreateChartsDocumentation");
            if (Directory.Exists(outputPath)) {
                Directory.Delete(outputPath, true);
                Thread.Sleep(100);
            }
            Directory.CreateDirectory(outputPath);

            var food = new Food("MyFood");
            var compound = new Compound("MyCompound");
            var mu = 2;
            var sigma = 1;
            var useFraction = 0.25;
            var lor = 2;
            var sampleSize = 200;
            var concentrationModelTypes = Enum.GetValues(typeof(ConcentrationModelType))
                .Cast<ConcentrationModelType>()
                .Where(cm => cm != ConcentrationModelType.LogNormal);

            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var concentrations = createConcentrations(mu, sigma, useFraction, sampleSize, random);
            var compoundResidueCollection = createConcentrations(food, compound, concentrations, lor, random);
            var settings = new MockConcentrationModelCalculationSettings() {
                NonDetectsHandlingMethod = NonDetectsHandlingMethod.ReplaceByZero,
            };
            var factory = new FoodConcentrationModelFactory(settings);
            foreach (var modelType in concentrationModelTypes) {
                var occurrenceFraction = Math.Min(
                    1D - compoundResidueCollection.FractionZeros,
                    compoundResidueCollection.FractionPositives + .5 * compoundResidueCollection.FractionCensoredValues
                );
                var model = factory.CreateModelAndCalculateParameters(food, compound, modelType, compoundResidueCollection, null, null, occurrenceFraction, ConcentrationUnit.mgPerKg);
                Assert.IsNotNull(model);
                if (model.ModelType == modelType) {
                    var record = new ConcentrationModelRecord();
                    record.Summarize(food, compound, model, false);
                    var chartCreator = new ConcentrationModelChartCreator(record, 300, 300, true);
                    var id = $"{modelType}";
                    var filename = Path.Combine(outputPath, $"{id}.svg");
                    chartCreator.CreateToSvg(filename);
                    chartCreator.CreateToPng(Path.Combine(outputPath, $"{id}.png"));
                }
            }
        }

        private List<double> createConcentrations(double mu, double sigma, double fractionZero, int n, IRandom random) {
            var positives = (int)(n - Math.Round(fractionZero * n));
            var zeros = n - positives;
            var x = Enumerable
                .Range(0, n)
                .Select(r => r < positives ? NormalDistribution.InvCDF(0, 1, random.NextDouble(0, 1)) * sigma + mu : 0D)
                .ToList();
            return x;
        }

        private FoodSubstanceResidueCollection createConcentrations(Food food, Compound compound, List<double> concentrations, double lor, IRandom random) {
            var positivesCount = concentrations.Count(r => r > 0);
            var zerosCount = concentrations.Count(r => r == 0);
            return new FoodSubstanceResidueCollection() {
                Food = food,
                Compound = compound,
                Positives = concentrations.Where(r => r >= lor).ToList(),
                CensoredValuesCollection = concentrations.Where(r => r < lor).Select(r => new CensoredValue() { LOD = lor, LOQ = lor }).ToList(),
                ZerosCount = zerosCount,
            };
        }
    }
}
