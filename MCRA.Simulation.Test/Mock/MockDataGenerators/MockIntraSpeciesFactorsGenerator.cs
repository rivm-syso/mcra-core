using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;

namespace MCRA.Simulation.Test.Mock.MockDataGenerators {
    /// <summary>
    /// Class for generating mock intraspecies factors
    /// </summary>
    public static class MockIntraSpeciesFactorsGenerator {
        /// <summary>
        /// Creates a list of intraspecies factors
        /// </summary>
        /// <param name="substances"></param>
        /// <param name="effect"></param>
        /// <param name="random"></param>
        /// <returns></returns>
        public static List<IntraSpeciesFactor> Create(ICollection<Compound> substances, Effect effect, IRandom random) {
            return substances.Select(c => {
                var factor = random.NextDouble();
                return new IntraSpeciesFactor() {
                    Compound = c,
                    Effect = effect,
                    Factor = factor,
                    IdPopulation = "population",
                    LowerVariationFactor = factor * .5,
                    UpperVariationFactor = 1,
                };
            }).ToList();
        }
    }
}
