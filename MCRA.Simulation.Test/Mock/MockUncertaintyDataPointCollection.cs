using MathNet.Numerics.Distributions;
using MCRA.Utils;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Test.Mock {
    public static class MockUncertaintyDataPointCollection {

        /// <summary>
        /// Creates a fake uncertain data point collection.
        /// </summary>
        public static UncertainDataPointCollection<double> Create(int numberOfSamples, bool doUncertainty) {
            var percentages = GriddingFunctions.Arange(0.1, 99.9, 100);
            var mu = 110.5;
            var sigma = 1.55;
            var draw = NormalDistribution.NormalSamples(numberOfSamples, mu, sigma).ToList();
            var collection = new UncertainDataPointCollection<double>() {
                XValues = percentages,
                ReferenceValues = draw.Percentiles(percentages),
            };
            if (doUncertainty) {
                for (int i = 0; i < 10; ++i) {
                    draw = NormalDistribution.NormalSamples(numberOfSamples, mu, sigma).ToList();
                    collection.AddUncertaintyValues(draw.Percentiles(percentages));
                }
            }
            return collection;
        }

        /// <summary>
        /// Creates a fake uncertain data point collection.
        /// </summary>
        public static UncertainDataPointCollection<double> Create(
            int numberOfSamples,
            double mu,
            double upper,
            double shift,
            double percentageZeros,
            bool doUncertainty
        ) {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var percentages = GriddingFunctions.Arange(0.1, 99.9, 100);
            var distribution = LogNormalDistribution.FromMeanAndUpper(mu, upper);
            var draws = distribution.Draws(random, numberOfSamples);
            var shifted = new List<double>();
            foreach (var item in draws) {
                shifted.Add(item + shift);
            }
            var zeros = Enumerable.Repeat(0D, (int)(numberOfSamples * percentageZeros / 100)).ToList();
            shifted.AddRange(zeros);

            var collection = new UncertainDataPointCollection<double>() {
                XValues = percentages,
                ReferenceValues = shifted.Percentiles(percentages),
            };
            if (doUncertainty) {
                for (int i = 0; i < 10; ++i) {
                    draws = distribution.Draws(random, numberOfSamples);
                    shifted = [];
                    foreach (var item in draws) {
                        shifted.Add(item + shift);
                    }
                    shifted.AddRange(zeros);
                    collection.AddUncertaintyValues(shifted.Percentiles(percentages));
                }
            }
            return collection;
        }
    }
}
