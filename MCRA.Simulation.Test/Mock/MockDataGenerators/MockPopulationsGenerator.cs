using MCRA.Data.Compiled.Objects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Simulation.Test.Mock.MockDataGenerators {
    /// <summary>
    /// Class for generating mock populations
    /// </summary>
    public static class MockPopulationsGenerator {

        private static string[] _defaultPopulations = {
            "NL", "DE", "BE"
        };

        /// <summary>
        /// Creates a list of populations
        /// </summary>
        /// <param name="names"></param>
        /// <returns></returns>
        public static List<Population> MockPopulations(params string[] names) {
            var result = names.Select(r => new Population() {
                Code = r,
                Name = r,
            }).ToList();
            return result;
        }

        /// <summary>
        /// Creates a list of populations
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public static List<Population> Create(int n) {
            if (n <= _defaultPopulations.Length) {
                var result = _defaultPopulations.Take(n).Select(r => new Population() {
                    Code = r,
                    Name = r,
                }).ToList();
                return result;
            }
            throw new Exception($"Cannot create more than {_defaultPopulations.Length} mock populations using this method!");
        }
    }
}
