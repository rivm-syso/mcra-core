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

            }
            return collection;
        }

        /// <summary>
        /// Creates a fake uncertain data point collection.
        /// </summary>
        public static UncertainDataPointCollection<double> Create(int numberOfSamples, double mu, double sigma, bool doUncertainty) {
            var percentages = GriddingFunctions.Arange(0.1, 99.9, 100);
            var draw = LogNormalDistribution.LogNormalSamples(numberOfSamples, mu, sigma).Select(c => c > 50 ? 100000 : c).ToList();
            var collection = new UncertainDataPointCollection<double>() {
                XValues = percentages,
                ReferenceValues = draw.Percentiles(percentages),
            };
            if (doUncertainty) {
                for (int i = 0; i < collection.Count; ++i) {
                    draw = NormalDistribution.NormalSamples(20, collection[i].ReferenceValue, 0.1 * collection[i].ReferenceValue)
                        .Select(c => c > 50 ? 100000 : c).ToList();
                    if (collection[i].ReferenceValue >= 10000) {
                        collection[i].XValue = 100;
                    }
                    collection[i].UncertainValues.AddRange(draw);
                }
            }
            return collection;
        }
    }
}
