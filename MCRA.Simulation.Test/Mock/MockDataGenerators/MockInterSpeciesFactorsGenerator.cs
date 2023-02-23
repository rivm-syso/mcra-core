using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;

namespace MCRA.Simulation.Test.Mock.MockDataGenerators {
    /// <summary>
    /// Class for generating mock interspecies factors
    /// </summary>
    public static class MockInterSpeciesFactorsGenerator {

        /// <summary>
        /// Creates a list of interspecies factors
        /// </summary>
        /// <param name="substances"></param>
        /// <param name="effect"></param>
        /// <param name="species"></param>
        /// <param name="random"></param>
        /// <returns></returns>
        public static List<InterSpeciesFactor> Create(
            ICollection<Compound> substances,
            Effect effect,
            string species,
            IRandom random
        ) {
            return substances.Select(c => {
                var factor = random.NextDouble();
                return new InterSpeciesFactor() {
                    Compound = c,
                    Effect = effect,
                    Species = species,
                    InterSpeciesFactorGeometricMean = 5 * factor,
                    InterSpeciesFactorGeometricStandardDeviation = 0.5 * factor,
                    StandardAnimalBodyWeight = .4,
                    StandardHumanBodyWeight = 75,
                    AnimalBodyWeightUnitString = General.BodyWeightUnit.kg.ToString(),
                    HumanBodyWeightUnitString = General.BodyWeightUnit.kg.ToString(),
                };
            }).ToList();
        }
    }
}
