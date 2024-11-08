using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Calculators.IntraSpeciesConversion;

namespace MCRA.Simulation.Test.Mock.FakeDataGenerators {

    /// <summary>
    /// Class for generating mock intraspecies factor models.
    /// </summary>
    public static class FakeIntraSpeciesFactorModelsGenerator {

        /// <summary>
        /// Creates a dictionary of intraspecies conversion models for each substance
        /// </summary>
        /// <param name="substances"></param>
        /// <param name="effect"></param>
        /// <param name="means"></param>
        /// <param name="gsds"></param>
        /// <param name="defaultFactor"></param>
        /// <returns></returns>
        public static Dictionary<(Effect, Compound), IntraSpeciesFactorModel> Create(
            ICollection<Compound> substances,
            Effect effect = null,
            double[] means = null,
            double[] gsds = null,
            double defaultFactor = 1
        ) {
            var result = CreateNullModelCollection(defaultFactor);
            for (int i = 0; i < substances.Count; i++) {
                var substance = substances.ElementAt(i);
                var model = new IntraSpeciesFactorModel() {
                    Effect = effect,
                    Substance = substance,
                    DegreesOfFreedom = double.NaN,
                    Factor = means != null ? means[i] : defaultFactor,
                    GeometricStandardDeviation = gsds != null ? gsds[i] : double.NaN,
                };
                result.Add((effect, substance), model);
            }
            return result;
        }

        public static Dictionary<(Effect, Compound), IntraSpeciesFactorModel> CreateNullModelCollection(double defaultFactor = 1) {
            var collection = new Dictionary<(Effect, Compound), IntraSpeciesFactorModel>();
            var nullModel = new IntraSpeciesFactorModel() {
                DegreesOfFreedom = double.NaN,
                Factor = defaultFactor,
                GeometricStandardDeviation = double.NaN,
            };
            collection.Add((null, null), nullModel);
            return collection;
        }
    }
}
