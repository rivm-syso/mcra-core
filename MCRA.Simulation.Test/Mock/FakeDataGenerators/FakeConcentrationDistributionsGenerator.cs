using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;

namespace MCRA.Simulation.Test.Mock.FakeDataGenerators {

    /// <summary>
    /// Class for generating mock single value consumptions by modelled food.
    /// </summary>
    public static class FakeConcentrationDistributionsGenerator {

        /// <summary>
        /// Generates mock concentration distributions for the specified foods and substances.
        /// </summary>
        /// <param name="foods"></param>
        /// <param name="substances"></param>
        /// <param name="seed"></param>
        /// <returns></returns>
        public static List<ConcentrationDistribution> Create(
            List<Food> foods,
            List<Compound> substances,
            int seed = 1
        ) {
            var random = new McraRandomGenerator(seed);
            var concentrationDistributions = new List<ConcentrationDistribution>();
            foreach (var food in foods) {
                foreach (var substance in substances) {
                    var mu = random.NextDouble() * 2;
                    var sigma = 0.5 + random.NextDouble();
                    var mean = Math.Exp(mu + (sigma * sigma) / 2);
                    var variance = (Math.Exp(sigma * sigma) - 1) * Math.Exp(2 * mu + sigma * sigma);
                    var p95 = LogNormalDistribution.InvCDF(mu, sigma, .95);
                    concentrationDistributions.Add(new ConcentrationDistribution() {
                        Compound = substance,
                        Food = food,
                        Mean = mean,
                        CV = mean / (Math.Sqrt(variance)),
                        Percentage = 95,
                        Percentile = p95,
                        Limit = (.9 + .2 * random.NextDouble()) * p95
                    });
                }
            }
            return concentrationDistributions;
        }
    }
}