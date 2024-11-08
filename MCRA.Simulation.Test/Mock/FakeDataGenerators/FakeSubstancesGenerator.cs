using MCRA.Data.Compiled.Objects;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Test.Mock.FakeDataGenerators {
    /// <summary>
    /// Class for generating mock substances
    /// </summary>
    public static class FakeSubstancesGenerator {
        /// <summary>
        /// Creates a list of substances
        /// </summary>
        /// <param name="number"></param>
        /// <param name="molecularWeights"></param>
        /// <param name="cramerClasses"></param>
        /// <returns></returns>
        public static List<Compound> Create(
            int number,
            double[] molecularWeights = null,
            int[] cramerClasses = null,
            bool? lipidSoluble = null,
            int seed = 1
        ) {
            var random = new McraRandomGenerator(seed);

            var compounds = new List<Compound>();
            for (int i = 0; i < number; i++) {
                var compound = new Compound() {
                    Code = $"CMP{i}",
                    Name = $"Compound {i}",
                    MolecularMass = molecularWeights?[i] ?? 291.1,
                    CramerClass = cramerClasses?[i],
                    IsLipidSoluble = lipidSoluble ?? (random.Next(0, 2) != 0),
                };
                compounds.Add(compound);
            }
            return compounds;
        }

        /// <summary>
        /// Creates a list of fake substances with the provided names,
        /// and optionally also including names.
        /// </summary>
        /// <param name="codes"></param>
        /// <param name="names"></param>
        /// <returns></returns>
        public static List<Compound> Create(
            string[] codes,
            string[] names = null
        ) {
            var compounds = new List<Compound>();
            for (int i = 0; i < codes.Length; i++) {
                var compound = new Compound() {
                    Code = codes[i],
                    Name = names?[i] ?? codes[i],
                };
                compounds.Add(compound);
            }
            return compounds;
        }
    }
}
