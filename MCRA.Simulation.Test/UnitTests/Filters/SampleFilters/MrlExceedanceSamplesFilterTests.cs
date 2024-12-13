using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Filters.FoodSampleFilters;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.Filters.SampleFilters {

    /// <summary>
    /// Tests for the MRL exceedance sample filter.
    /// </summary>
    [TestClass]
    public class MrlExceedanceSamplesFilterTests {

        #region Mock generation

        private static string[] _substanceCodes = ["A", "B", "C", "D", "E"];
        private static string[] _foodCodes = ["A", "B", "C", "D", "E"];

        private static readonly Dictionary<string, Compound> _substances =
            _substanceCodes.Select(r => new Compound(r)).ToDictionary(r => r.Code);

        private static readonly Dictionary<string, Food> _foods =
            _foodCodes.Select(r => new Food(r)).ToDictionary(r => r.Code);

        private static FoodSample mockFoodSample(
            Food food,
            Compound[] substances,
            double[] concentrations,
            ConcentrationUnit concentrationUnit,
            bool tabulated
        ) {

            var sample = new SampleAnalysis() {
                Concentrations = substances
                    .Select((r, ix) => new ConcentrationPerSample() {
                        Compound = r,
                        Concentration = concentrations[ix]
                    })
                    .ToDictionary(r => r.Compound),
            };
            if (!tabulated) {
                sample.AnalyticalMethod = new AnalyticalMethod() {
                    AnalyticalMethodCompounds = substances.ToDictionary(
                        r => r,
                        r => new AnalyticalMethodCompound() {
                            ConcentrationUnit = concentrationUnit
                        })
                };
            }
            var foodSample = new FoodSample() { SampleAnalyses = [sample], Food = food };
            return foodSample;
        }

        #endregion

        /// <summary>
        /// Tests nu mrls.
        /// </summary>
        [TestMethod]
        public void MrlExceedanceSamplesFilter_TestNoMrls() {
            var foodSample = mockFoodSample(
                _foods["A"],
                _substances.Values.ToArray(),
                _substances.Select(r => 1D).ToArray(),
                ConcentrationUnit.mgPerKg,
                false
            );
            var filter = new MrlExceedanceSamplesFilter(null, 2);
            Assert.IsTrue(filter.Passes(foodSample));
        }

        /// <summary>
        /// Simple tests of the MRL exceedance samples filter with different
        /// MRL threshold multiplication factors.
        /// </summary>
        [TestMethod]
        public void MrlExceedanceSamplesFilter_TestSimple() {
            var foodSample = mockFoodSample(
                _foods["A"],
                _substances.Values.ToArray(),
                _substances.Select(r => 1D).ToArray(),
                ConcentrationUnit.mgPerKg,
                false
            );
            var mrls = new List<ConcentrationLimit>() {
                new() {
                    Food = _foods["A"],
                    Compound = _substances["A"],
                    Limit = 1,
                    ConcentrationUnit = ConcentrationUnit.mgPerKg
                },
            };
            var filter = new MrlExceedanceSamplesFilter(mrls, 1);
            Assert.IsTrue(filter.Passes(foodSample));

            filter = new MrlExceedanceSamplesFilter(mrls, 2);
            Assert.IsTrue(filter.Passes(foodSample));

            filter = new MrlExceedanceSamplesFilter(mrls, .5);
            Assert.IsFalse(filter.Passes(foodSample));
        }

        /// <summary>
        /// Tests the MRL exceedance samples with different concentration units.
        /// </summary>
        [TestMethod]
        public void MrlExceedanceSamplesFilter_TestConcentrationUnit() {
            var foodSample = mockFoodSample(
                _foods["A"],
                _substances.Values.ToArray(),
                _substances.Select(r => 1.1).ToArray(),
                ConcentrationUnit.ugPerKg,
                false
            );
            var mrls = new List<ConcentrationLimit>() {
                new() {
                    Food = _foods["A"],
                    Compound = _substances["A"],
                    Limit = 1,
                    ConcentrationUnit= ConcentrationUnit.mgPerKg
                },
            };
            var filter = new MrlExceedanceSamplesFilter(mrls, 1);
            Assert.IsTrue(filter.Passes(foodSample));

            filter = new MrlExceedanceSamplesFilter(mrls, 0.001);
            Assert.IsFalse(filter.Passes(foodSample));
        }

        /// <summary>
        /// Tests the MRL exceedance samples with MRL for different foods/substances.
        /// </summary>
        [TestMethod]
        public void MrlExceedanceSamplesFilter_TestNoMatch() {
            var foodSample = mockFoodSample(
                _foods["A"],
                [_substances["B"]],
                _substances.Select(r => 1.1).ToArray(),
                ConcentrationUnit.ugPerKg,
                false
            );
            var mrls = new List<ConcentrationLimit>() {
                new() {
                    Food = _foods["A"],
                    Compound = _substances["A"],
                    Limit = 1,
                    ConcentrationUnit = ConcentrationUnit.mgPerKg
                },
                new() {
                    Food = _foods["B"],
                    Compound = _substances["B"],
                    Limit = 1,
                    ConcentrationUnit= ConcentrationUnit.mgPerKg
                },
            };

            var filter = new MrlExceedanceSamplesFilter(mrls, 1);
            Assert.IsTrue(filter.Passes(foodSample));
        }
    }
}
