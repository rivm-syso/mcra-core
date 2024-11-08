using MCRA.Data.Compiled.Objects;
using MCRA.General;

namespace MCRA.Simulation.Test.Mock.FakeDataGenerators {

    /// <summary>
    /// Class for generating mock processing types
    /// </summary>
    public static class FakeProcessingTypesGenerator {
        private static string[] _types = {
            "Juicing", "Baking", "Milling", "Frying", "Mashing", "Cooking", "Drying", "Washing"
        };

        /// <summary>
        /// Creates a list of processing types
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public static List<ProcessingType> Create(int n) {
            if (n <= _types.Length) {
                var processingType = _types
                    .Take(n)
                    .Select((r, ix) => CreateSingle($"{ix + 1}", r, (ProcessingDistributionType)(ix % 2 + 1), (ix % 2 + 1) == 1))
                    .ToList();
                return processingType;
            }
            throw new Exception($"Cannot create more than {_types.Length} mock foods using this method!");
        }

        /// <summary>
        /// Creates a single processing type.
        /// </summary>
        /// <param name="code"></param>
        /// <param name="name"></param>
        /// <param name="distributionType"></param>
        /// <param name="isBulkingBlending"></param>
        /// <returns></returns>
        public static ProcessingType CreateSingle(
            string code,
            string name = null,
            ProcessingDistributionType distributionType = ProcessingDistributionType.LogNormal,
            bool isBulkingBlending = false
        ) {
            name = string.IsNullOrEmpty(name) ? code : name;
            var description = name == code ? name : $"{name} ({code})";
            return new ProcessingType() {
                Code = code,
                Name = name,
                Description = description,
                IsBulkingBlending = isBulkingBlending,
                DistributionType = distributionType
            };
        }
    }
}
