using MCRA.Utils.Statistics;
using MCRA.Simulation.Calculators.IntakeModelling.ModelThenAddIntakeModelCalculation;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.DietaryExposures {
    public class MtaFakeDataGenerator {

        public static List<Category> CreateFakeCategories() {
            return new List<Category>() {
                new Category("model 1", "model 1 BBN (n=1)"),
                new Category("model 2", "model 2 BBN (n=2)"),
                new Category("model 3", "model 3 OIM (n=1)"),
                new Category("model 4", "model 4 OIM (n=100)"),
                new Category("model 5", "model 5 OIM (n=100)")
            };
        }

        public static List<CategorizedIndividualExposure> CreateFakeIndividualExposuresByCategory(
            int number,
            List<Category> categories,
            int seed = 1
        ) {
            var result = new List<CategorizedIndividualExposure>();
            var random = new McraRandomGenerator(seed);
            var logData = categories
                .Select(c => createExposuresDistribution(
                    random.Next(10, 20),
                    1 + .5 * random.NextDouble(),
                    .2 + .8 * random.NextDouble(),
                    number,
                    random
                ))
                .ToList();
            for (int i = 0; i < number; i++) {
                var categoryExposures = categories.Select((r, ix) => new CategoryExposure(r.Id, logData[ix][i])).ToList();
                var detailedindividualAmount = new CategorizedIndividualExposure() {
                    SamplingWeight = 1,
                    SimulatedIndividualId = i,
                    CategoryExposures = categoryExposures
                };
                result.Add(detailedindividualAmount);
            }
            return result;
        }

        private static List<double> createExposuresDistribution(
            double mu,
            double sigma,
            double fractionZero,
            int samples,
            IRandom random
        ) {
            var positives = (int)(samples - Math.Round(fractionZero * samples));
            var x = Enumerable
                .Range(0, samples)
                .Select(r => r < positives ? NormalDistribution.InvCDF(mu, sigma, random.NextDouble()) : 0D)
                .ToList();
            return x.OrderBy(r => random.NextDouble()).ToList();
        }
    }
}
