using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;

namespace MCRA.Simulation.Test.Mock.FakeDataGenerators {

    /// <summary>
    /// Class for generating mock responses.
    /// </summary>
    public static class FakeSubstanceConversionsGenerator {

        /// <summary>
        /// Generates mock deterministic substance conversion factors.
        /// </summary>
        /// <param name="measuredSubstances"></param>
        /// <param name="activeSubstances"></param>
        /// <param name="seed"></param>
        /// <returns></returns>
        public static ICollection<SubstanceConversion> Create(
            ICollection<Compound> measuredSubstances,
            ICollection<Compound> activeSubstances,
            int seed = 1
        ) {
            var rnd = new McraRandomGenerator(seed);
            var result = new List<SubstanceConversion>();
            foreach (var measuredSubstance in measuredSubstances) {
                var rdCase = rnd.Next(0, 3);
                switch (rdCase) {
                    case 0:
                        result.AddRange(mockComplexSubstanceConversion(
                            measuredSubstance,
                            activeSubstances.Take(rnd.Next(2, 3)),
                            false,
                            rnd)
                        );
                        break;
                    case 1:
                        result.AddRange(mockComplexSubstanceConversion(
                            measuredSubstance,
                            activeSubstances.Take(rnd.Next(2, 5)),
                            true,
                            rnd)
                        );
                        break;
                    default:
                        result.Add(mockIdentityConversion(measuredSubstance));
                        break;
                }
            }
            return result;
        }

        private static ICollection<SubstanceConversion> mockComplexSubstanceConversion(
            Compound measuredSubstance,
            IEnumerable<Compound> activeSubstances,
            bool allExclusive,
            IRandom random
        ) {
            var result = new List<SubstanceConversion>();
            var proportions = activeSubstances.Select(r => random.NextDouble()).ToList();
            proportions = proportions.Select(r => r / proportions.Sum()).ToList();
            for (int i = 0; i < activeSubstances.Count(); i++) {
                var record = new SubstanceConversion() {
                    ActiveSubstance = activeSubstances.ElementAt(i),
                    MeasuredSubstance = measuredSubstance,
                    ConversionFactor = LogNormalDistribution.Draw(random, 0, 1),
                    Proportion = proportions[i],
                    IsExclusive = allExclusive || i == 0
                };
                result.Add(record);
            }
            return result;
        }

        private static SubstanceConversion mockIdentityConversion(Compound substance) {
            return new SubstanceConversion() {
                ActiveSubstance = substance,
                MeasuredSubstance = substance,
                ConversionFactor = 1D,
                Proportion = 1D,
                IsExclusive = true
            };
        }
    }
}
