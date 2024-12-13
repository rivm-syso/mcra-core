using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers;

namespace MCRA.Simulation.Test.Mock.FakeDataGenerators {

    /// <summary>
    /// Class for generating mock single value consumptions by modelled food.
    /// </summary>
    public static class FakeSingleValueConsumptionModelsGenerator {

        /// <summary>
        /// Generates mock single value consumption per modelled food records.
        /// </summary>
        /// <param name="foods"></param>
        /// <param name="random"></param>
        /// <param name="mus"></param>
        /// <param name="sigmas"></param>
        /// <param name="percentages"></param>
        /// <param name="processingProportions"></param>
        /// <returns></returns>
        public static ICollection<SingleValueConsumptionModel> Create(
            ICollection<Food> foods,
            IRandom random,
            double[] mus = null,
            double[] sigmas = null,
            double[] percentages = null,
            IDictionary<(Food, string), double> processingProportions = null
        ) {
            mus = mus ?? NormalDistribution.Samples(random, 5, 1, foods.Count).ToArray();
            sigmas = sigmas ?? ContinuousUniformDistribution.Samples(random, 0, 0.2, foods.Count).ToArray();
            percentages = percentages ?? [50, 97.5];
            processingProportions = processingProportions ?? FakeFoodsGenerator.CreateReverseYieldFactors(foods, random);
            var result = new List<SingleValueConsumptionModel>();
            var counter = 0;
            foreach (var food in foods) {
                double reverseYieldFactor = double.NaN;
                if (food.ProcessingTypes?.Count > 0) {
                    processingProportions.TryGetValue((food.BaseFood, food.ProcessingFacetCode()), out reverseYieldFactor);
                }
                var mu = mus[counter];
                var sigma = sigmas[counter];
                var logNormal = new LogNormalDistribution(mu, sigma);
                var samples = logNormal.Draws(random, 20);
                var model = new SingleValueConsumptionModel() {
                    Food = food.BaseFood ?? food,
                    ProcessingTypes = food.ProcessingTypes,
                    ProcessingCorrectionFactor = reverseYieldFactor,
                    MeanConsumption = samples.Any() ? samples.Average() : double.NaN,
                    Percentiles = samples.Percentiles(percentages).Select((r, ix) => (percentages[ix], r)).ToList(),
                    BodyWeight = 70
                };
                result.Add(model);
                counter++;
            }
            return result;
        }
    }
}