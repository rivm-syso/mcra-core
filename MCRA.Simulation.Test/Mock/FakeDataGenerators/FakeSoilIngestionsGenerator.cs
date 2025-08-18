using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.General;

namespace MCRA.Simulation.Test.Mock.FakeDataGenerators {

    /// <summary>
    /// Class for generating fake soil ingestions.
    /// </summary>
    public static class FakeSoilIngestionsGenerator {

        /// <summary>
        /// Generates fake soil ingestions.
        /// </summary>
        public static List<SoilIngestion> Create(
            List<GenderType> sexes,
            List<double?> ages,
            int seed = 1
        ) {
            var random = new McraRandomGenerator(seed);
            var soilIngestions = new List<SoilIngestion>();
            var i = 0;
            foreach (var sex in sexes) {
                foreach (var age in ages) {
                    var mu = random.NextDouble() * 2;
                    var sigma = 0.5 + random.NextDouble();
                    var mean = Math.Exp(mu + (sigma * sigma) / 2);
                    var variance = (Math.Exp(sigma * sigma) - 1) * Math.Exp(2 * mu + sigma * sigma);
                    soilIngestions.Add(new SoilIngestion() {
                        idSubgroup = i.ToString(),
                        AgeLower = age,
                        Sex = sex,
                        DistributionType = SoilIngestionDistributionType.LogNormal,
                        Value = mean,
                        CvVariability = mean / Math.Sqrt(variance)
                    });
                    i++;
                }
            }
            return soilIngestions;
        }
    }
}