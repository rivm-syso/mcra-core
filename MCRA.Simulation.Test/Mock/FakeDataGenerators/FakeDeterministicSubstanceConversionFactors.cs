using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using System.Collections;

namespace MCRA.Simulation.Test.Mock.FakeDataGenerators {

    /// <summary>
    /// Class for generating mock deterministic substance conversion factors.
    /// </summary>
    public static class FakeDeterministicSubstanceConversionFactorsGenerator {

        /// <summary>
        /// Generates mock deterministic substance conversion factors.
        /// </summary>
        /// <param name="measuredSubstances"></param>
        /// <param name="activeSubstances"></param>
        /// <param name="foods"></param>
        /// <param name="random"></param>
        /// <returns></returns>
        public static ICollection<DeterministicSubstanceConversionFactor> Create(
            ICollection<Compound> measuredSubstances,
            ICollection<Compound> activeSubstances,
            IRandom random,
            ICollection<Food> foods = null
        ) {

            // First the identity translations
            var result = measuredSubstances
                .Where(r => activeSubstances.Contains(r))
                .Select(r => new DeterministicSubstanceConversionFactor() {
                    ActiveSubstance = r,
                    MeasuredSubstance = r,
                    ConversionFactor = LogNormalDistribution.Draw(random, 0, 1)
                })
                .ToList();

            // Randomly combine the remaining measured substances and active substances
            var remainingActiveSubstances = activeSubstances.Except(measuredSubstances).ToList();
            var remainingMeasuredSubstances = measuredSubstances.Except(activeSubstances).ToList();
            var candidates = new Stack(remainingActiveSubstances.OrderBy(r => random.NextDouble()).ToList());
            foreach (var measuredSubstance in remainingMeasuredSubstances) {
                if (candidates.Count > 0) {
                    var activeSubstance = candidates.Pop() as Compound;
                    if (foods == null) {
                        var record = new DeterministicSubstanceConversionFactor() {
                            ActiveSubstance = activeSubstance,
                            MeasuredSubstance = measuredSubstance,
                            ConversionFactor = LogNormalDistribution.Draw(random, 0, 1),
                        };
                        result.Add(record);
                    } else {
                        foreach (var food in foods) {
                            var record = new DeterministicSubstanceConversionFactor() {
                                ActiveSubstance = activeSubstance,
                                MeasuredSubstance = measuredSubstance,
                                ConversionFactor = LogNormalDistribution.Draw(random, 0, 1),
                                Food  = food
                            };
                            result.Add(record);
                        }
                    }
                }
            }
            return result;
        }
    }
}
