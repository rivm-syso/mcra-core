using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.General;

namespace MCRA.Simulation.Test.Mock.FakeDataGenerators {

    /// <summary>
    /// Class for generating mock samples
    /// </summary>
    public static class FakeSamplesGenerator {

        /// <summary>
        /// Creates a random collection of samples for the provided foods and substances.
        /// </summary>
        public static List<FoodSample> CreateFoodSamples(
            ICollection<Food> foods,
            ICollection<Compound> substances,
            int numberOfSamples = 100,
            int numberOfAnalyticalMethods = 5,
            double mu = -3,
            double sigma = 0.5,
            double lod = 0.05,
            int seed = 1
        ) {
            var random = new McraRandomGenerator(seed);
            var sampleOrigins = new List<string>() { "Unknown", "Location1", "Location2" };
            var analyticalMethods = new List<AnalyticalMethod>();

            for (int i = 0; i < numberOfAnalyticalMethods; i++) {
                var analyticalMethod = new AnalyticalMethod() {
                    Code = $"AM{i}",
                    Description = $"Analytical Method {i}",
                    AnalyticalMethodCompounds = new Dictionary<Compound, AnalyticalMethodCompound>(),
                };
                foreach (var substance in substances) {
                    analyticalMethod.AnalyticalMethodCompounds[substance] = new AnalyticalMethodCompound() {
                        Compound = substance,
                        LOD = lod,
                        ConcentrationUnit = ConcentrationUnit.mgPerKg,
                        AnalyticalMethod = analyticalMethod
                    };
                }
                analyticalMethods.Add(analyticalMethod);
            }

            var foodSamples = new List<FoodSample>();
            for (int f = 0; f < foods.Count; f++) {
                for (int s = 0; s < numberOfSamples; s++) {
                    var food = foods.ElementAt(f);
                    var analyticalMethod = analyticalMethods.ElementAt(random.Next(analyticalMethods.Count));
                    var foodSample = new FoodSample() {
                        Code = $"FS_{food.Code}_{s}",
                        Food = food,
                        SampleAnalyses = [],
                        Location = sampleOrigins[random.Next() % sampleOrigins.Count],
                        DateSampling = new DateTime(2022, 6, 1),
                        Region = "Location1",
                        ProductionMethod = "ProductionMethod",
                    };
                    var sampleAnalysis = new SampleAnalysis() {
                        Code = $"SA_{food.Code}_{s}",
                        AnalyticalMethod = analyticalMethod,
                        Concentrations = new Dictionary<Compound, ConcentrationPerSample>(),
                        AnalysisDate = new DateTime(2022, 6, 1)
                    };
                    foreach (var substance in substances) {
                        var concentration = LogNormalDistribution.Draw(random, mu, sigma);
                        if (concentration > analyticalMethod.AnalyticalMethodCompounds[substance].LOD) {
                            sampleAnalysis.Concentrations[substance] = new ConcentrationPerSample() {
                                Compound = substance,
                                Concentration = random.NextDouble(),
                                Sample = sampleAnalysis,
                                ResType = ResType.VAL
                            };
                        }
                    }
                    foodSample.SampleAnalyses.Add(sampleAnalysis);
                    foodSamples.Add(foodSample);
                }
            }

            return foodSamples;
        }
    }
}
