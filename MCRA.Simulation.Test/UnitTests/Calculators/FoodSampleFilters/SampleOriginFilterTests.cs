using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Calculators.FoodSampleFilters;

namespace MCRA.Simulation.Test.UnitTests.Calculators.SampleFilters {
    [TestClass]
    public class SampleOriginFilterTests {

        #region Fake generation

        private static string[] locations = [
                "NL",
                "nl",
                "DE",
                "DE",
                "be",
                "BE",
                null,
                null
            ];

        private static List<FoodSample> fakeFoodSamples() {
            var samples = locations.Select(r => new SampleAnalysis()).ToList();
            return samples.Select((c, ix) => new FoodSample() {
                SampleAnalyses = [c],
                Location = locations[ix],
            }
            ).ToList();
        }

        #endregion

        /// <summary>
        /// Tests sample origin filter.
        /// </summary>
        [TestMethod]
        public void SampleOriginFilter_Test1() {
            var locationSubsetDefinition = new List<string>() { "NL" };
            var filter = new SampleLocationFilter(locationSubsetDefinition, true);
            var foodSamples = fakeFoodSamples();
            var filteredSamples = foodSamples.Where(filter.Passes).ToList();
            Assert.AreEqual(4, filteredSamples.Count);
        }

        /// <summary>
        /// Tests sample origin filter.
        /// </summary>
        [TestMethod]
        public void SampleOriginFilter_Test2() {
            var locationSubsetDefinition = new List<string>() { "NL" };
            var filter = new SampleLocationFilter(locationSubsetDefinition, false);
            var foodSamples = fakeFoodSamples();
            var filteredSamples = foodSamples.Where(filter.Passes).ToList();
            Assert.AreEqual(2, filteredSamples.Count);
        }

        /// <summary>
        /// Tests sampling origin filter.
        /// </summary>
        [TestMethod]
        public void SampleOriginFilter_TestNoLocationDefinition() {
            var locationSubsetDefinition = new List<string>();
            var filter = new SampleLocationFilter(locationSubsetDefinition, false);
            var foodSamples = fakeFoodSamples();
            var filteredSamples = foodSamples.Where(filter.Passes).ToList();
            Assert.AreEqual(8, filteredSamples.Count);
        }
    }
}
