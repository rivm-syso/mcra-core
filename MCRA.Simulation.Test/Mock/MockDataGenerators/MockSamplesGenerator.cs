using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Simulation.Test.Mock.MockDataGenerators {

    /// <summary>
    /// Class for generating mock samples
    /// </summary>
    public static class MockSamplesGenerator {

        /// <summary>
        /// Creates a random collection of samples for the provided foods and substances.
        /// </summary>
        /// <param name="foods"></param>
        /// <param name="substances"></param>
        /// <param name="numberOfSamples"></param>
        /// <param name="numberOfAnalyticalMethods"></param>
        /// <param name="mu"></param>
        /// <param name="sigma"></param>
        /// <param name="lod"></param>
        /// <param name="seed"></param>
        /// <returns></returns>
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
                        ConcentrationUnitString = ConcentrationUnit.mgPerKg.ToString(),
                        AnalyticalMethod = analyticalMethod
                    };
                }
                analyticalMethods.Add(analyticalMethod);
            }
        
            var foodSamples = new List<FoodSample>();
            for (int f = 0; f < foods.Count; f++) {
                int counter = 0;
                for (int s = 0; s < numberOfSamples; s++) {
                    var analyticalMethod = analyticalMethods.ElementAt(random.Next(analyticalMethods.Count));

                    var foodSample = new FoodSample() {
                        Code = $"FS{counter}_{f}",
                        Food = foods.ElementAt(f),
                        SampleAnalyses = new List<SampleAnalysis>(),
                        Location = sampleOrigins[random.Next() % sampleOrigins.Count],
                        DateSampling = new DateTime(2022, 6, 1),
                        Region = "Location1",
                        ProductionMethod = "ProductionMethod",
                    };
                    var sampleAnalysis = new SampleAnalysis() {
                        Code = $"S{counter}_{f}",
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
                                ResTypeString = ResType.VAL.ToString()
                            };
                        }
                    }
                    foodSample.SampleAnalyses.Add(sampleAnalysis);
                    foodSamples.Add(foodSample);
                    counter++;
                }
            }

            return foodSamples;
        }
    }
}
