using MCRA.Utils.ExtensionMethods;
using MCRA.Data.Compiled.Objects;
using MCRA.General;

namespace MCRA.Simulation.Test.Mock.FakeDataGenerators {

    /// <summary>
    /// Class for generating mock responses.
    /// </summary>
    public static class FakeResponsesGenerator {

        private static (string Code, ResponseType Response)[] _responseTypes = {
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
                        ResponseType = responseType,
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
        public static List<Response> Create(
            int n,
            string[] species = null,
            TestSystemType testSystemType = TestSystemType.CellLine
        ) {
            if (n <= _responseTypes.Length) {
                var responses = _responseTypes
                    .Take(n)
                    .Select((r,ix) => new Response() {
                        Code = r.Code,
                        Name = r.Code,
                        Description = r.Code,
                        GuidelineMethod = $"Guideline {r.Code}",
                        ResponseType = r.Response,
                        ResponseUnit = "unit",
                        TestSystem = new TestSystem() {
                            Code = species != null ? $"Test-system-{species[ix]}-Art" : $"Test-system-Art",
                            Description = "Artificial test-system",
                            Species = species?[ix],
                            Organ = "liver",
                            TestSystemType = testSystemType
                        }
                    })
                    .ToList();
                return responses;
            }
            throw new Exception($"Cannot create more than {_responseTypes.Length} mock responses using this method!");
        }
    }
}
