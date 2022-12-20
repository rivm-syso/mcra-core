using MCRA.Utils.ExtensionMethods;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Simulation.Test.Mock.MockDataGenerators {

    /// <summary>
    /// Class for generating mock responses.
    /// </summary>
    public static class MockResponsesGenerator {

        private static (string, ResponseType)[] _responseTypes = {
            ("CellCount", ResponseType.Quantal),
            ("Weight", ResponseType.ContinuousMultiplicative),
            ("DeadCellCount", ResponseType.Quantal),
            ("Size", ResponseType.ContinuousMultiplicative)
        };

        /// <summary>
        /// Creates a collectino of fake responses for the provided test systems.
        /// </summary>
        /// <param name="testSystems"></param>
        /// <param name="responsesPerTestSystem"></param>
        /// <param name="responseTypes"></param>
        /// <returns></returns>
        public static List<Response> Create(
            List<TestSystem> testSystems,
            int responsesPerTestSystem = 1,
            ResponseType[] responseTypes = null
        ) {
            var result = new List<Response>();
            var availableResponseTypes = Enum.GetValues(typeof(ResponseType)).Cast<ResponseType>().ToList();
            foreach (var testSystem in testSystems) {
                for (int i = 0; i < responsesPerTestSystem; i++) {
                    var responseType = responseTypes == null
                        ? availableResponseTypes[result.Count % availableResponseTypes.Count]
                        : responseTypes[result.Count % responseTypes.Length];
                    var record = new Response() {
                        Code = $"{testSystem.Code}-R{i}-{responseType.GetShortDisplayName()}",
                        Name = $"{testSystem.Code}-R{i}-{responseType.GetShortDisplayName()}",
                        Description = $"{testSystem.Code}-R{i}-{responseType.GetShortDisplayName()}",
                        ResponseTypeString = responseType.ToString(),
                        ResponseUnit = "unit",
                        TestSystem = testSystem,
                        GuidelineMethod = "XXX"
                    };
                    result.Add(record);
                }
            }
            return result;
        }

        /// <summary>
        /// Creates a list of responses
        /// </summary>
        /// <param name="n"></param>
        /// <param name="species"></param>
        /// <returns></returns>
        public static List<Response> Create(
            int n,
            string[] species = null
        ) {
            if (n <= _responseTypes.Length) {
                var responses = _responseTypes
                    .Take(n)
                    .Select((r,ix) => new Response() {
                        Code = r.Item1,
                        Name = r.Item1,
                        Description = r.Item1,
                        GuidelineMethod = $"Guideline {r.Item1}",
                        ResponseTypeString = r.Item2.ToString(),
                        ResponseUnit = "unit",
                        TestSystem = new TestSystem() {
                            Code = species != null ? $"Test-system-{species[ix]}-Art" : $"Test-system-Art",
                            Description = "Artificial test-system",
                            Species = species?[ix],
                            Organ = "liver"
                        }
                    })
                    .ToList();
                return responses;
            }
            throw new Exception($"Cannot create more than {_responseTypes.Length} mock responses using this method!");
        }
    }
}
