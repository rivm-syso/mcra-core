using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Simulation.Test.Mock.MockDataGenerators {

    /// <summary>
    /// Class for generating mock single value consumptions by modelled food.
    /// </summary>
    public static class MockConcentrationSingleValuesGenerator {

        /// <summary>
        /// Creates a mock concentration single value record collection.
        /// </summary>
        /// <param name="foods"></param>
        /// <param name="substances"></param>
        /// <param name="mus"></param>
        /// <param name="sigmas"></param>
        /// <param name="loqs"></param>
        /// <param name="positivesFactions"></param>
        /// <param name="mrls"></param>
        /// <param name="concentrationUnit"></param>
        /// <param name="seed"></param>
        /// <returns></returns>
        public static IList<ConcentrationSingleValue> Create(
            ICollection<Food> foods,
            ICollection<Compound> substances,
            double[] mus = null,
            double[] sigmas = null,
            double[] loqs = null,
            double[] positivesFactions = null,
            double[] mrls = null,
            ConcentrationUnit concentrationUnit = ConcentrationUnit.mgPerKg,
            int seed = 1
        ) {
            var random = new McraRandomGenerator(seed);
            var totalRecordsCount = foods.Count * substances.Count;
            var percentages = new double[] { 50, 97.5 };
            mus = mus ?? NormalDistribution.Samples(random, 0, .1, totalRecordsCount).ToArray();
            sigmas = sigmas ?? LogNormalDistribution.Samples(random, 0, 1, totalRecordsCount).ToArray();
            loqs = loqs ?? new double[] { 0.1 };
            mrls = mrls ?? new double[] { 1, 0.5, 0.1, 0.05, 0.01 };
            positivesFactions = ContinuousUniformDistribution.Samples(random, 0, 1, totalRecordsCount).ToArray();
            var result = new List<ConcentrationSingleValue>();
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
                    if (!samples.Any()) {
                        result.Add(createLoqRecord(concentrationUnit, food, substance, loq));
                    } else {
                        result.Add(createHighestConcentrationRecord(concentrationUnit, food, substance, samples));
                        result.Add(createMeanConcentrationRecord(concentrationUnit, food, substance, samples));
                        var percentiles = samples.Any() ? samples.Percentiles(percentages) : null;
                        var percentileRecords = createPercentileRecords(concentrationUnit, percentages, percentiles, food, substance);
                        result.AddRange(percentileRecords);
                    }
                    if (!double.IsNaN(mrl)) {
                        result.Add(createMrlRecord(concentrationUnit, food, substance, mrl));
                    }
                }
            }
            return result;
        }

        private static ConcentrationSingleValue createMrlRecord(ConcentrationUnit concentrationUnit, Food food, Compound substance, double mrl) {
            return new ConcentrationSingleValue() {
                Food = food,
                Substance = substance,
                Value = mrl,
                ConcentrationUnit = concentrationUnit,
                ValueType = ConcentrationValueType.MRL
            };
        }

        private static ConcentrationSingleValue createLoqRecord(ConcentrationUnit concentrationUnit, Food food, Compound substance, double loq) {
            return new ConcentrationSingleValue() {
                Food = food,
                Substance = substance,
                Value = loq,
                ConcentrationUnit = concentrationUnit,
                ValueType = ConcentrationValueType.LOQ
            };
        }

        private static ConcentrationSingleValue createHighestConcentrationRecord(ConcentrationUnit concentrationUnit, Food food, Compound substance, IEnumerable<double> samples) {
            var highestConcentration = samples.Any() ? samples.Max() : double.NaN;
            var record = new ConcentrationSingleValue() {
                Food = food,
                Substance = substance,
                Value = highestConcentration,
                ConcentrationUnit = concentrationUnit,
                ValueType = ConcentrationValueType.HighestConcentration
            };
            return record;
        }

        private static ConcentrationSingleValue createMeanConcentrationRecord(ConcentrationUnit concentrationUnit, Food food, Compound substance, IEnumerable<double> samples) {
            var meanConcentration = samples.Any() ? samples.Average() : double.NaN;
            var record = new ConcentrationSingleValue() {
                Food = food,
                Substance = substance,
                Value = meanConcentration,
                ConcentrationUnit = concentrationUnit,
                ValueType = ConcentrationValueType.MeanConcentration
            };
            return record;
        }

        private static IEnumerable<ConcentrationSingleValue> createPercentileRecords(ConcentrationUnit concentrationUnit, double[] percentages, double[] percentiles, Food food, Compound substance) {
            return percentiles.Select((r, ix) => new ConcentrationSingleValue() {
                Food = food,
                Substance = substance,
                Value = r,
                ConcentrationUnit = concentrationUnit,
                ValueType = ConcentrationValueType.Percentile,
                Percentile = percentages[ix]
            });
        }
    }
}