using MCRA.Data.Compiled.Objects;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Calculators.FoodSampleFilters;

namespace MCRA.Simulation.Test.UnitTests.Calculators.SampleFilters {
    [TestClass]
    public class SamplingPeriodFilterTests {

        #region Mock generation
        private static string[] dates = [
                "01/01/2018",
                "01/07/2018",
                "01/01/2019",
                "01/07/2019",
                "01/01/2020",
                "01/07/2020",
                null,
                null
            ];
        #endregion

        private static List<FoodSample> mockFoodSamples() {
            var samples = dates.Select(r => new SampleAnalysis()).ToList();
            return samples.Select((c, ix) => new FoodSample() {
                SampleAnalyses = [c],
                DateSampling = !string.IsNullOrEmpty(dates[ix]) ? DateTime.Parse(dates[ix]) : null
            }
            ).ToList();
        }


        /// <summary>
        /// Tests sampling period filter.
        /// </summary>
        [TestMethod]
        public void SamplingPeriodFilter_Test1() {
            var periodSubsetDefinition = new PeriodSubsetDefinition() {
                YearsSubset = ["2019"]
            };
            var filter = new SamplePeriodFilter(periodSubsetDefinition.YearsSubsetTimeRanges, true);
            var foodSamples = mockFoodSamples();
            var filteredSamples = foodSamples.Where(r => filter.Passes(r)).ToList();
            Assert.HasCount(4, filteredSamples);
        }

        /// <summary>
        /// Tests sampling period filter.
        /// </summary>
        [TestMethod]
        public void SamplingPeriodFilter_Test2() {
            var periodSubsetDefinition = new PeriodSubsetDefinition() {
                YearsSubset = ["2019"]
            };
            var filter = new SamplePeriodFilter(periodSubsetDefinition.YearsSubsetTimeRanges, false);
            var foodSamples = mockFoodSamples();
            var filteredSamples = foodSamples.Where(r => filter.Passes(r)).ToList();
            Assert.HasCount(2, filteredSamples);
        }

        /// <summary>
        /// Tests sampling period filter; no period subset definitions.
        /// </summary>
        [TestMethod]
        public void SamplingPeriodFilter_TestNoPeriodDefinition() {
            var periodSubsetDefinition = new PeriodSubsetDefinition();
            var filter = new SamplePeriodFilter(periodSubsetDefinition.YearsSubsetTimeRanges, true);
            var foodSamples = mockFoodSamples();
            var filteredSamples = foodSamples.Where(r => filter.Passes(r)).ToList();
            Assert.HasCount(8, filteredSamples);
        }
    }
}
