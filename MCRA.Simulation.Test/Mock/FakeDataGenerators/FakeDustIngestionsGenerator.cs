using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.General;

namespace MCRA.Simulation.Test.Mock.FakeDataGenerators {

    /// <summary>
    /// Class for generating fake dust ingestions.
    /// </summary>
    public static class FakeDustIngestionsGenerator {

        /// <summary>
        /// Generates fake dust ingestions.
        /// </summary>
        /// <param name="seed"></param>
        /// <returns></returns>
        public static List<DustIngestion> Create(
            int seed = 1
        ) {
            var random = new McraRandomGenerator(seed);
            var dustIngestions = new List<DustIngestion>();
            for (int i = 0; i < 3; i++) {
                var mu = random.NextDouble() * 2;
                var sigma = 0.5 + random.NextDouble();
                var mean = Math.Exp(mu + (sigma * sigma) / 2);
                var variance = (Math.Exp(sigma * sigma) - 1) * Math.Exp(2 * mu + sigma * sigma);
                dustIngestions.Add(new DustIngestion() {
                    idSubgroup = i.ToString(),
                    AgeLower = 20D * i,
                    Sex = GenderType.Undefined,
                    DistributionType = DustIngestionDistributionType.LogNormal,
                    Value = mean,
                    CvVariability = mean / Math.Sqrt(variance)
                });
            }
            return dustIngestions;
        }
    }
}