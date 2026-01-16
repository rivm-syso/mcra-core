using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.CompoundResidueCollectionCalculation;
using MCRA.Simulation.Calculators.ConcentrationModelCalculation.ConcentrationModels;
using MCRA.Simulation.Calculators.FoodConcentrationModelBuilders;
using MCRA.Simulation.Test.Mock.MockCalculatorSettings;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Test.Mock.FakeDataGenerators {

    /// <summary>
    /// Class for generating mock concentration models
    /// </summary>
    public static class FakeConcentrationsModelsGenerator {

        /// <summary>
        /// Creates  concentration models
        /// </summary>
        /// <param name="foods"></param>
        /// <param name="substances"></param>
        /// <param name="modelType"></param>
        /// <param name="mu"></param>
        /// <param name="sigma"></param>
        /// <param name="useFraction"></param>
        /// <param name="lor"></param>
        /// <param name="sampleSize"></param>
        /// <param name="nonDetectsHandlingMethod"></param>
        /// <returns></returns>
        public static IDictionary<(Food, Compound), ConcentrationModel> Create(
            ICollection<Food> foods,
            ICollection<Compound> substances,
            ConcentrationModelType modelType = ConcentrationModelType.Empirical,
            double mu = -1.1,
            double sigma = 2,
            double useFraction = .25,
            double lor = 0.05,
            int sampleSize = 30,
            NonDetectsHandlingMethod nonDetectsHandlingMethod = NonDetectsHandlingMethod.ReplaceByZero
        ) {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var concentrationModels = new Dictionary<(Food, Compound), ConcentrationModel>();
            var settings = new MockConcentrationModelCalculationSettings() {
                NonDetectsHandlingMethod = nonDetectsHandlingMethod,
                DefaultConcentrationModel = ConcentrationModelType.Empirical,
            };

            foreach (var food in foods) {
                foreach (var substance in substances) {
                    var concentrations = createConcentrations(mu, sigma, useFraction, sampleSize, random);
                    var compoundResidueCollection = createConcentrations(food, substance, concentrations, lor);
                    var factory = new FoodConcentrationModelFactory(settings);
                    var model = factory.CreateModelAndCalculateParameters(food, substance, modelType, compoundResidueCollection, null, null, useFraction, ConcentrationUnit.mgPerKg);
                    concentrationModels[(food, substance)] = model;
                }
            }
            return concentrationModels;
        }

        /// <summary>
        /// Creates  concentration models
        /// </summary>
        /// <param name="compoundResidueCollections"></param>
        /// <param name="modelType"></param>
        /// <returns></returns>
        public static IDictionary<(Food, Compound), ConcentrationModel> Create(
            IDictionary<(Food, Compound), FoodSubstanceResidueCollection> compoundResidueCollections,
            ConcentrationModelType modelType
        ) {
            var settings = new MockConcentrationModelCalculationSettings() {
                NonDetectsHandlingMethod = NonDetectsHandlingMethod.ReplaceByZero,
                DefaultConcentrationModel = ConcentrationModelType.Empirical,
            };
            var concentrationModels = new Dictionary<(Food, Compound), ConcentrationModel>();
            foreach (var compoundResidueCollection in compoundResidueCollections.Values) {
                var food = compoundResidueCollection.Food;
                var substance = compoundResidueCollection.Compound;
                var factory = new FoodConcentrationModelFactory(settings);
                var occurrenceFraction = Math.Min(
                    1D - compoundResidueCollection.FractionZeros,
                    compoundResidueCollection.FractionPositives + .5 * compoundResidueCollection.FractionCensoredValues
                );
                var model = factory.CreateModelAndCalculateParameters(food, substance, modelType, compoundResidueCollection, null, null, occurrenceFraction, ConcentrationUnit.mgPerKg);
                concentrationModels[(food, substance)] = model;
            }
            return concentrationModels;
        }

        /// <summary>
        /// Creates concentrations based on mu and sigmas
        /// </summary>
        /// <param name="mu"></param>
        /// <param name="sigma"></param>
        /// <param name="fractionZero"></param>
        /// <param name="n"></param>
        /// <param name="random"></param>
        /// <returns></returns>
        private static List<double> createConcentrations(
            double mu,
            double sigma,
            double fractionZero,
            int n,
            IRandom random
        ) {
            var positives = (int)(n - Math.Round(fractionZero * n));
            var zeros = n - positives;
            var x = Enumerable
                .Range(0, n)
                .Select(r => r < positives ? LogNormalDistribution.InvCDF(mu, sigma, random.NextDouble(0, 1)) : 0D)
                .ToList();
            return x.Shuffle(random).ToList();
        }

        /// <summary>
        /// Creates concentrations on specified concentrations
        /// </summary>
        /// <param name="food"></param>
        /// <param name="compound"></param>
        /// <param name="concentrations"></param>
        /// <param name="lor"></param>
        ///
        /// <returns></returns>
        private static FoodSubstanceResidueCollection createConcentrations(Food food, Compound compound, List<double> concentrations, double lor) {
            var positivesCount = concentrations.Count(r => r > 0);
            var zerosCount = concentrations.Count(r => r == 0);
            return new FoodSubstanceResidueCollection() {
                Food = food,
                Compound = compound,
                Positives = concentrations.Where(r => r >= lor).ToList(),
                CensoredValuesCollection = concentrations.Where(r => r < lor).Select(r => new CensoredValue() { LOD = lor, LOQ = lor, ResType = ResType.LOD }).ToList(),
                ZerosCount = zerosCount,
            };
        }
    }
}
