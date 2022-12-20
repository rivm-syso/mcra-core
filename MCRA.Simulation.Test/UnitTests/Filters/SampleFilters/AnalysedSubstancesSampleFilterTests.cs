using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Filters.FoodSampleFilters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Simulation.Test.UnitTests.Filters.SampleFilters {

    /// <summary>
    /// Tests for the analysed substance sample filter.
    /// </summary>
    [TestClass]
    public class AnalysedSubstancesSampleFilterTests {

        #region Mock generation

        private static string[] _substanceCodes =
            { "A", "B", "C", "D", "E" };

        private static Dictionary<string, Compound> _substances =
            _substanceCodes.Select(r => new Compound(r)).ToDictionary(r => r.Code);

        private static SampleAnalysis mockSampleWithConcentration(params string[] substanceCodes) {
            var result = new SampleAnalysis() {
                Concentrations = substanceCodes
                    .ToDictionary(
                        r => _substances[r],
                        r => new ConcentrationPerSample() { Concentration = 1D }
                    )
            };
            return result;
        }

        private static SampleAnalysis mockSampleWithAnalyticalMethod(params string[] substanceCodes) {
            var result = new SampleAnalysis() {
                AnalyticalMethod = new AnalyticalMethod() {
                    AnalyticalMethodCompounds = substanceCodes
                        .ToDictionary(
                            r => _substances[r],
                            r => new AnalyticalMethodCompound()
                        )
                }
            };
            return result;
        }

        private static List<FoodSample> mockFoodSamples() {
            var samples = new List<SampleAnalysis>() {
                mockSampleWithConcentration("A", "B"),
                mockSampleWithConcentration("B", "C"),
                mockSampleWithAnalyticalMethod("B", "C"),
                mockSampleWithAnalyticalMethod("C", "D"),
                new SampleAnalysis(),
                new SampleAnalysis()
            };
            return samples.Select(c => new FoodSample() {
                SampleAnalyses = new List<SampleAnalysis>() { c },
                DateSampling = new System.DateTime(),
            }
            ).ToList();
        }
        #endregion

        /// <summary>
        /// Tests sampling period filter.
        /// </summary>
        [TestMethod]
        public void AnalysedSubstancesSampleFilter_Test1() {
            var substances = new List<Compound>() {
                _substances["A"],
                _substances["D"]
            };
            var filter = new AnalysedSubstancesFoodSampleFilter(substances);
            var foodSamples = mockFoodSamples();
            var filteredSamples = foodSamples.Where(r => filter.Passes(r)).ToList();
            Assert.AreEqual(2, filteredSamples.Count);
        }

        /// <summary>
        /// Tests sampling period filter.
        /// </summary>
        [TestMethod]
        public void AnalysedSubstancesSampleFilter_Test2() {
            var substances = new List<Compound>() {
                _substances["E"]
            };
            var filter = new AnalysedSubstancesFoodSampleFilter(substances);
            var foodSamples = mockFoodSamples();
            var filteredSamples = foodSamples.Where(r => filter.Passes(r)).ToList();
            Assert.AreEqual(0, filteredSamples.Count);
        }

        /// <summary>
        /// Tests sampling period filter.
        /// </summary>
        [TestMethod]
        public void AnalysedSubstancesSampleFilter_TestNoSelectionDefinition() {
            var substances = new List<Compound>();
            var filter = new AnalysedSubstancesFoodSampleFilter(substances);
            var foodSamples = mockFoodSamples();
            var filteredSamples = foodSamples.Where(r => filter.Passes(r)).ToList();
            Assert.AreEqual(6, filteredSamples.Count);
        }
    }
}
