using MCRA.Utils.ExtensionMethods;
using MCRA.Data.Compiled.Objects;
using MCRA.General;

namespace MCRA.Simulation.Test.Mock.FakeDataGenerators {

    /// <summary>
    /// Class for generating fake test systems.
    /// </summary>
    public static class FakeTestSystemsGenerator {

        /// <summary>
        /// List of all available test system types.
        /// </summary>
        public static List<TestSystemType> AvailableTestSystemTypes =
            Enum.GetValues(typeof(TestSystemType)).Cast<TestSystemType>().ToList();

        /// <summary>
        /// Creates fake test systems.
        /// </summary>
        /// <param name="n"></param>
        /// <param name="testSystemTypes"></param>
        /// <param name="species"></param>
        /// <param name="organs"></param>
        /// <returns></returns>
        public static List<TestSystem> Create(
            int n,
            TestSystemType[] testSystemTypes = null,
            string[] species = null,
            string[] organs = null
        ) {
            var result = new List<TestSystem>();
            for (int i = 0; i < n; i++) {
                var testSystemType = testSystemTypes == null
                    ? AvailableTestSystemTypes[result.Count % AvailableTestSystemTypes.Count]
                    : testSystemTypes[result.Count];
                var record = new TestSystem() {
                    Code = $"R{i}-{testSystemType.GetShortDisplayName()}",
                    Name = $"R{i}-{testSystemType.GetShortDisplayName()}",
                    Description = $"R{i}-{testSystemType.GetShortDisplayName()}",
                    TestSystemType = testSystemType,
                    Species = species?[result.Count],
                    Organ = organs?[result.Count],
                };
                result.Add(record);
            }
            return result;
        }
    }
}
