using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers;

namespace MCRA.Simulation.Test.Mock.MockDataGenerators {

    /// <summary>
    /// Class for generating mock single value consumptions by modelled food.
    /// </summary>
    public static class MockSingleValueConcentrationModelsGenerator {

        /// <summary>
        /// Generates mock single value consumption per modelled food records.
        /// </summary>
        /// <param name="foods"></param>
        /// <param name="substances"></param>
        /// <param name="mus"></param>
        /// <param name="sigmas"></param>
        /// <param name="loqs"></param>
        /// <param name="positivesFactions"></param>
        /// <param name="mrls"></param>
        /// <param name="random"></param>
        /// <returns></returns>
        public static IDictionary<(Food, Compound), SingleValueConcentrationModel> Create(
            ICollection<Food> foods,
            ICollection<Compound> substances,
            IRandom random,
            double[] mus = null,
            double[] sigmas = null,
            double[] loqs = null,
            double[] positivesFactions = null,
            double[] mrls = null
        ) {
            var totalRecordsCount = foods.Count * substances.Count;
            var percentages = new double[] { 50, 97.5 };
            mus = mus ?? NormalDistribution.Samples(random, 0, .1, totalRecordsCount).ToArray();
            sigmas = sigmas ?? LogNormalDistribution.Samples(random, 0, 1, totalRecordsCount).ToArray();
            loqs = loqs ?? new double[] { 0.1 };
            mrls = mrls ?? new double[] { 1, 0.5, 0.1, 0.05, 0.01 };
            positivesFactions = ContinuousUniformDistribution.Samples(random, 0, 1, totalRecordsCount).ToArray();
            var result = new Dictionary<(Food, Compound), SingleValueConcentrationModel>();
            var counter = 0;
            foreach (var food in foods) {
                foreach (var substance in substances) {
                    var mu = mus.Length > counter ? mus[counter] : mus[0];
                    var sigma = sigmas.Length > counter ? sigmas[counter] : sigmas[0];
                    var loq = loqs.Length > counter ? loqs[counter] : loqs[0];
                    var fracPositives = positivesFactions.Length > counter ? positivesFactions[counter] : positivesFactions[0];
                    var mrl = mrls[random.Next(0, mrls.Length)];
                    var samples = Enumerable
                        .Range(0, 20)
                        .Select(r => random.NextDouble() < fracPositives ? LogNormalDistribution.Draw(random, mu, sigma) : double.NaN)
                        .ToList()
                        .Where(r => r > loq);
                    counter++;
                    var percentiles = samples.Any() ? samples.Percentiles(percentages) : null;
                    var record = new SingleValueConcentrationModel() {
                        Food = food,
                        Substance = substance,
                        Loq = loq,
                        MeanConcentration = samples.Any() ? samples.Average() : double.NaN,
                        HighestConcentration = samples.Any() ? samples.Max() : double.NaN,
                        Percentiles = percentiles?.Select((r, ix) => (percentages[ix], r)).ToList(),
                        Mrl = mrl
                    };
                    result.Add((food, substance), record);
                }
            }
            return result;
        }
    }
}