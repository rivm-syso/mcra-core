using MCRA.Data.Compiled.Objects;

namespace MCRA.Simulation.Test.Mock.MockDataGenerators {

    /// <summary>
    /// Class for generating fake non-dietary exposure sources.
    /// </summary>
    public static class FakeNonDietaryExposureSourcesGenerator {

        private static readonly string[] _defaultSources = {
            "Aftershave", "Body lotion", "Conditioner",
            "Deodorant", "Eye cream", "Face cleanser"
        };

        /// <summary>
        /// Creates a list of non-dietary exposure sources.
        /// </summary>
        /// <param name="names"></param>
        /// <returns></returns>
        public static List<NonDietaryExposureSource> FakeNonDietaryExposureSources(params string[] names) {
            var result = names
                .Select(r => new NonDietaryExposureSource() {
                    Code = r,
                    Name = r,
                })
                .ToList();
            return result;
        }

        /// <summary>
        /// Creates a list of non-dietary exposure sources.
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public static List<NonDietaryExposureSource> Create(int n) {
            if (n <= _defaultSources.Length) {
                var result = _defaultSources
                    .Take(n)
                    .Select(r => new NonDietaryExposureSource() {
                        Code = r,
                        Name = r,
                    })
                    .ToList();
                return result;
            }
            throw new Exception($"Cannot create more than {_defaultSources.Length} mock non-dietary exposure sources using this method!");
        }
    }
}
