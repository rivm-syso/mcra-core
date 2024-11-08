using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Filters.FoodSampleFilters;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.Filters.SampleFilters {
    [TestClass]
    public class SampleOriginFilterTests {

        #region Mock generation

        private static string[]  locations = new string[] {
                "NL",
                "nl",
                "DE",
                "DE",
                "be",
                "BE",
                null,
                null
            };

        private static List<FoodSample> mockFoodSamples() {
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
            var foodSamples = mockFoodSamples();
            var filteredSamples = foodSamples.Where(r => filter.Passes(r)).ToList();
            Assert.AreEqual(4, filteredSamples.Count);
        }

        /// <summary>
        /// Tests sample origin filter.
        /// </summary>
        [TestMethod]
        public void SampleOriginFilter_Test2() {
            var locationSubsetDefinition = new List<string>() { "NL" };
            var filter = new SampleLocationFilter(locationSubsetDefinition, false);
            var foodSamples = mockFoodSamples();
            var filteredSamples = foodSamples.Where(r => filter.Passes(r)).ToList();
            Assert.AreEqual(2, filteredSamples.Count);
        }

        /// <summary>
        /// Tests sampling origin filter.
        /// </summary>
        [TestMethod]
        public void SampleOriginFilter_TestNoLocationDefinition() {
            var locationSubsetDefinition = new List<string>();
            var filter = new SampleLocationFilter(locationSubsetDefinition, false);
            var foodSamples = mockFoodSamples();
            var filteredSamples = foodSamples.Where(r => filter.Passes(r)).ToList();
            Assert.AreEqual(8, filteredSamples.Count);
        }
    }
}
