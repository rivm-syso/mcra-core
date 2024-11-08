using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Calculators.ProcessingFactorCalculation;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Test.Mock.MockDataGenerators {
    /// <summary>
    /// Class for generating mock processing factors
    /// </summary>
    public static class MockProcessingFactorsGenerator {

        /// <summary>
        /// Creates a collection of mock processing factor models for the specified foods,
        /// substances, and processing types.
        /// </summary>
        /// <param name="foods"></param>
        /// <param name="substances"></param>
        /// <param name="processingTypes"></param>
        /// <param name="random"></param>
        /// <param name="isProcessing"></param>
        /// <param name="isDistribution"></param>
        /// <param name="allowHigherThanOne"></param>
        /// <param name="fractionMissing">Fraction of missing processing factors.</param>
        /// <param name="includeUncertainty"></param>
        /// <returns></returns>
        public static ProcessingFactorModelCollection CreateProcessingFactorModelCollection(
            ICollection<Food> foods,
            ICollection<Compound> substances,
            ICollection<ProcessingType> processingTypes,
            IRandom random,
            bool isProcessing = true,
            bool isDistribution = false,
            bool allowHigherThanOne = false,
            double fractionMissing = 0,
            bool includeUncertainty = false
        ) {
            var factors = Create(foods, substances, random, processingTypes, includeUncertainty);
            if (fractionMissing > 0) {
                factors = factors.Where(r => random.NextDouble() > fractionMissing).ToList();
            }
            var settings = new ProcessingFactorsModuleConfig {
                IsProcessing = isProcessing,
                IsDistribution = isDistribution,
                AllowHigherThanOne = allowHigherThanOne
            };
            var builder = new ProcessingFactorModelCollectionBuilder(settings);
            var result = builder.Create(factors, substances);
            return result;
        }

        /// <summary>
        /// Creates a list of processing factors
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public static List<ProcessingFactor> Create(int number) {
            var processingFactors = new List<ProcessingFactor>();
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            for (int i = 0; i < number; i++) {
                var pf = Create(i, random);
                processingFactors.Add(pf);
            }
            return processingFactors;
        }

        /// <summary>
        /// Creates a processing factor
        /// </summary>
        /// <param name="i"></param>
        /// <param name="random"></param>
        /// <returns></returns>
        public static ProcessingFactor Create(int i, IRandom random) {
            var procType = new int[] { 0, 1 };
            var compound = new Compound() {
                Code = $"Compound {i}",
                Name = $"Compound {i}",
            };
            var food = new Food() {
                Code = $"Food {i}",
                Name = $"Food {i}",
            };
            var lower = random.NextDouble(.5, .9);
            var upper = random.NextDouble(lower, 1);
            var uncertaintyLower = lower * 1.1 < 1 ? lower * 1.1 : 1;
            var uncertaintyUpper = upper * 1.1 < 1 ? upper * 1.1 : 1;
            var pf = new ProcessingFactor() {
                Compound = compound,
                FoodProcessed = food,
                FoodUnprocessed = food,
                Nominal = lower,
                Upper = upper,
                NominalUncertaintyUpper = random.NextDouble(lower, uncertaintyLower),
                UpperUncertaintyUpper = random.NextDouble(upper, uncertaintyUpper),
                ProcessingType = new ProcessingType() {
                    Code = "processing",
                    DistributionType = (ProcessingDistributionType)procType[i % 2],
                }
            };
            return pf;
        }

        /// <summary>
        /// Creates a list of processing factors for the foods associated with
        /// the specified processing types (i.e., foods with those specific
        /// processing types).
        /// </summary>
        /// <param name="foods"></param>
        /// <param name="substances"></param>
        /// <param name="processingTypes"></param>
        /// <param name="includeUncertainty"></param>
        /// <param name="random"></param>
        /// <returns></returns>
        public static ICollection<ProcessingFactor> Create(
            ICollection<Food> foods,
            ICollection<Compound> substances,
            IRandom random,
            ICollection<ProcessingType> processingTypes,
            bool includeUncertainty = false
        ) {
            var processingFactors = new List<ProcessingFactor>();
            var i = 0;
            foreach (var food in foods) {
                if (food.ProcessingTypes?.Count > 0) {
                    var processingType = food.ProcessingTypes.First();
                    if (processingTypes.Contains(processingType)) {
                        foreach (var substance in substances) {
                            var nominal = .6 + .8 * random.NextDouble();
                            //var upper = 0;
                            var upper = nominal + nominal * 2 * random.NextDouble();
                            var nominalUncertaintyUpper = includeUncertainty
                                ? nominal + nominal * 2 * random.NextDouble()
                                : double.NaN;
                            var upperUncertaintyUpper = includeUncertainty
                                ? nominalUncertaintyUpper + nominalUncertaintyUpper * 2 * random.NextDouble()
                                : double.NaN;
                            var rescale = processingType.DistributionType == ProcessingDistributionType.LogisticNormal
                                ? 1D / ((!double.IsNaN(upperUncertaintyUpper) ? upperUncertaintyUpper : upper) * 1.1)
                                : 1D;
                            var pf = new ProcessingFactor() {
                                Compound = substance,
                                FoodProcessed = food,
                                FoodUnprocessed = food.BaseFood,
                                Nominal = rescale * nominal,
                                Upper = rescale * upper,
                                NominalUncertaintyUpper = rescale * nominalUncertaintyUpper,
                                UpperUncertaintyUpper = rescale * upperUncertaintyUpper,
                                ProcessingType = processingType
                            };
                            processingFactors.Add(pf);
                            i++;
                        }
                    }
                }
            }
            return processingFactors;
        }
    }
}