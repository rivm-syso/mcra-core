using MCRA.Utils;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Test.Mock {
    public static class MockUncertaintyDataPointCollection {

        /// <summary>
        /// Creates a fake uncertain data point collection.
        /// </summary>
        /// <param name="numberOfSamples"></param>
        /// <param name="doUncertainty"></param>
        /// <returns></returns>
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
    }
}
