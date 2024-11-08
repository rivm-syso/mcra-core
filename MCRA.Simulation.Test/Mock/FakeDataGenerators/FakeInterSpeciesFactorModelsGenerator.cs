using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Calculators.InterSpeciesConversion;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Test.Mock.FakeDataGenerators {
    /// <summary>
    /// Class for generating mock interspecies factors
    /// </summary>
    public static class FakeInterSpeciesFactorModelsGenerator {

        public static IDictionary<(string species, Compound substance, Effect effect), InterSpeciesFactorModel> Create(
            ICollection<InterSpeciesFactor> interSpeciesFactors
        ) {
            var result = new List<InterSpeciesFactorModel>();
            foreach (var record in interSpeciesFactors) {
                result.Add(new InterSpeciesFactorModel(record));
            }
            return result.ToDictionary(c => (c.InterSpeciesFactor.Species, c.InterSpeciesFactor.Compound, c.InterSpeciesFactor.Effect));
        }

        /// <summary>
        /// Creates a collection of random inter-species factor models for the provided
        /// substances and species.
        /// </summary>
        /// <param name="substances"></param>
        /// <param name="species"></param>
        /// <param name="effect"></param>
        /// <param name="random"></param>
        /// <param name="defaultGeometricFactor"></param>
        /// <returns></returns>
        public static IDictionary<(string species, Compound substance, Effect effect), InterSpeciesFactorModel> Create(
            ICollection<Compound> substances,
            ICollection<string> species,
            Effect effect,
            IRandom random,
            double defaultGeometricFactor = 10D
        ) {
            var records = substances
                .SelectMany(c => species, (sub, spec) => {
                    var factor = random.NextDouble();
                    return new InterSpeciesFactor() {
                        Compound = sub,
                        Effect = effect,
                        Species = spec,
                        InterSpeciesFactorGeometricMean = 5 * factor,
                        InterSpeciesFactorGeometricStandardDeviation = 0.5 * factor,
                        StandardAnimalBodyWeight = .4,
                        StandardHumanBodyWeight = 75,
                        AnimalBodyWeightUnit = General.BodyWeightUnit.kg,
                        HumanBodyWeightUnit = General.BodyWeightUnit.kg,
                    };
                })
                .ToList();
            var defaultFactor = new InterSpeciesFactor() {
                InterSpeciesFactorGeometricMean = defaultGeometricFactor,
                InterSpeciesFactorGeometricStandardDeviation = 1D
            };
            records.Add(defaultFactor);
            return Create(records);
        }

        /// <summary>
        /// Creates a collection of random inter-species factor models for the provided
        /// substances and species.
        /// </summary>
        /// <param name="substances"></param>
        /// <param name="species"></param>
        /// <param name="effect"></param>
        /// <param name="defaultGeometricMean"></param>
        /// <param name="geometricMeans"></param>
        /// <param name="geometricStandardDeviations"></param>
        /// <returns></returns>
        public static IDictionary<(string species, Compound substance, Effect effect), InterSpeciesFactorModel> Create(
            ICollection<Compound> substances,
            ICollection<string> species,
            Effect effect,
            double defaultGeometricMean = 10D,
            double[] geometricMeans = null,
            double[] geometricStandardDeviations = null
        ) {
            var records = new List<InterSpeciesFactor>();
            for (int i = 0; i < substances.Count; i++) {
                var substance = substances.ElementAt(i);
                foreach (var spec in species) {
                    var geometricMean = geometricMeans != null ? geometricMeans[i] : defaultGeometricMean;
                    var gsd = geometricStandardDeviations != null ? geometricStandardDeviations[i] : 1D;
                    var record = new InterSpeciesFactor() {
                        Compound = substance,
                        Effect = effect,
                        Species = spec,
                        InterSpeciesFactorGeometricMean = geometricMean,
                        InterSpeciesFactorGeometricStandardDeviation = gsd,
                        StandardAnimalBodyWeight = .4,
                        StandardHumanBodyWeight = 75,
                        AnimalBodyWeightUnit = General.BodyWeightUnit.kg,
                        HumanBodyWeightUnit = General.BodyWeightUnit.kg
                    };
                    records.Add(record);
                }
            }
            var defaultFactor = new InterSpeciesFactor() {
                InterSpeciesFactorGeometricMean = defaultGeometricMean,
                InterSpeciesFactorGeometricStandardDeviation = 1D
            };
            records.Add(defaultFactor);
            return Create(records);
        }
    }
}
