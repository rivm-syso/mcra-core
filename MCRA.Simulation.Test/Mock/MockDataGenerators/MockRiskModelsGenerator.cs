using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;

namespace MCRA.Simulation.Test.Mock.MockDataGenerators {
    /// <summary>
    /// Class for generating mock dietary individual day intakes
    /// </summary>
    public static class MockRiskModelsGenerator {

        /// <summary>
        /// Generates mcok dietary exposure models.
        /// </summary>
        /// <param name="names"></param>
        /// <param name="percentages"></param>
        /// <param name="numUncertaintyRecords"></param>
        /// <param name="seed"></param>
        /// <returns></returns>
        public static ICollection<RiskModel> CreateMockRiskModels(
            string[] names,
            double[] percentages,
            int numUncertaintyRecords,
            int seed
        ) {
            var result = new List<RiskModel>();
            var random = new McraRandomGenerator(seed);
            foreach (var name in names) {
                var mu = NormalDistribution.Draw(random, 0, 1);
                var sigma = ContinuousUniformDistribution.Draw(0.1, 2);
                var useFraction = 0.25;
                var model = new RiskModel() {
                    Code = name,
                    Name = name,
                    Description = name,
                };
                var exposures = createExposures(mu, sigma, useFraction, 10000);
                var percentiles = exposures.Percentiles(percentages);
                model.RiskPercentiles = percentages
                    .Select((r, ix) => new RiskPercentile() {
                        Percentage = r,
                        MarginOfExposure = percentiles[ix],
                        MarginOfExposureUncertainties = numUncertaintyRecords > 0 ? new List<double>() : null
                    })
                    .ToDictionary(r => r.Percentage);
                if (numUncertaintyRecords > 0) {
                    for (int i = 0; i < numUncertaintyRecords; i++) {
                        var muUncertain = NormalDistribution.Draw(random, mu, .1);
                        exposures = createExposures(muUncertain, sigma, useFraction, 10000);
                        percentiles = exposures.Percentiles(percentages);
                        for (int j = 0; j < percentages.Length; j++) {
                            model.RiskPercentiles[percentages[j]].MarginOfExposureUncertainties.Add(percentiles[j]);
                        }
                    }
                }
                result.Add(model);
            }
            return result;
        }

        /// <summary>
        /// Creates exposure based drawn from a zero-spike lognormal distribution.
        /// </summary>
        /// <param name="mu"></param>
        /// <param name="sigma"></param>
        /// <param name="fractionZero"></param>
        /// <param name="n"></param>
        /// <param name="seed"></param>
        /// <returns></returns>
        private static List<double> createExposures(
            double mu,
            double sigma,
            double fractionZero,
            int n,
            int seed = 1
        ) {
            var random = new McraRandomGenerator(seed);
            var positives = (int)(n - Math.Round(fractionZero * n));
            var zeros = n - positives;
            var x = Enumerable
                .Range(0, n)
                .Select(r => r < positives ? LogNormalDistribution.InvCDF(mu, sigma, random.NextDouble(0, 1)) : 0D)
                .ToList();
            return x.ToList();
        }
    }
}
