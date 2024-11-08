using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.SingleValueDietaryExposuresCalculation;

namespace MCRA.Simulation.Test.Mock.FakeDataGenerators {

    public class MockSingleValueDietaryExposure : ISingleValueDietaryExposure {
        public Food Food { get; set; }
        public Compound Substance { get; set; }
        public ProcessingType ProcessingType { get; set; }
        public SingleValueDietaryExposuresCalculationMethod CalculationMethod { get; set; }
        public double Exposure { get; set; }
    }

    /// <summary>
    /// Class for generating mock single value dietary exposures.
    /// </summary>
    public static class FakeSingleValueDietaryExposuresGenerator {

        /// <summary>
        /// Generates mock single value dietary exposure records.
        /// </summary>
        /// <param name="foods"></param>
        /// <param name="substances"></param>
        /// <param name="mus"></param>
        /// <param name="sigmas"></param>
        /// <param name="random"></param>
        /// <returns></returns>
        public static ICollection<ISingleValueDietaryExposure> Create(
            ICollection<Food> foods,
            ICollection<Compound> substances,
            IRandom random,
            double[] mus = null,
            double[] sigmas = null
        ) {
            mus = mus ?? NormalDistribution.Samples(random, 5, 1, foods.Count * substances.Count).ToArray();
            sigmas = sigmas ?? ContinuousUniformDistribution.Samples(random, 0, 0.2, foods.Count * substances.Count).ToArray();
            var result = new List<ISingleValueDietaryExposure>();
            var counter = 0;
            foreach (var food in foods) {
                var mu = mus[counter];
                var sigma = sigmas[counter];
                foreach (var substance in substances) {
                    var exposure = LogNormalDistribution.Draw(random, mu, sigma);
                    var model = new MockSingleValueDietaryExposure() {
                        Food = food.BaseFood ?? food,
                        Substance = substance,
                        ProcessingType = food.ProcessingTypes?.LastOrDefault(),
                        CalculationMethod = SingleValueDietaryExposuresCalculationMethod.IEDI,
                        Exposure = exposure
                    };
                    result.Add(model);
                    counter++;
                }
            }
            return result;
        }
    }
}