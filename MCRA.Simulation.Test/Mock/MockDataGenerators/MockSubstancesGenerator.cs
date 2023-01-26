using MCRA.Data.Compiled.Objects;
using System.Collections.Generic;

namespace MCRA.Simulation.Test.Mock.MockDataGenerators {
    /// <summary>
    /// Class for generating mock substances
    /// </summary>
    public static class MockSubstancesGenerator {
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
            int[] cramerClasses = null
        ) {
            var compounds = new List<Compound>();
            for (int i = 0; i < number; i++) {
                var compound = new Compound() {
                    Code = $"CMP{i}",
                    Name = $"Compound {i}",
                    MolecularMass = molecularWeights != null ? molecularWeights[i] : 291.1,
                    CramerClass = cramerClasses != null ? (int?)cramerClasses[i] : null,
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
                    Name = codes != null ? codes[i] : names[i],
                };
                compounds.Add(compound);
            }
            return compounds;
        }
    }
}
