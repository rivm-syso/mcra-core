using MCRA.Utils.Collections;
using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers.ISampleOriginInfo;
using MCRA.General.Action.Settings.Dto;
using MCRA.Simulation.Calculators.OccurrenceFrequenciesCalculation;
using MCRA.Simulation.Calculators.OccurrencePatternsCalculation;
using MCRA.Simulation.Calculators.SampleOriginCalculation;
using MCRA.Simulation.OutputGeneration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using MCO = MCRA.Data.Compiled.Objects;

namespace MCRA.Simulation.Test.UnitTests.Calculators.OccurrencePatterns {

    /// <summary>
    /// Occurrence frequencies calculator.
    /// </summary>
    [TestClass]
    public class OccurrenceFrequenciesCalculatorTests {
        private static double _epsilon = 1e-3;

        private IDictionary<string, Food> _foods;
        private IDictionary<string, Compound> _substances;
        private ICollection<MCO.OccurrencePattern> _agriculturalUses;
        private IDictionary<Food, List<ISampleOrigin>> _sampleOrigins;
        private Food _foodApple;
        private Food _foodBananas;
        private Food _foodPineapple;
        private Compound _compoundA;
        private Compound _compoundB;
        private Compound _compoundC;
        private Compound _compoundD;

        /// <summary>
        /// Test initialization
        /// </summary>
        [TestInitialize]
        public void TestInitialize() {
            _foods = new Dictionary<string, Food>(StringComparer.OrdinalIgnoreCase);
            _substances = new Dictionary<string, Compound>(StringComparer.OrdinalIgnoreCase);
            _agriculturalUses = new List<MCO.OccurrencePattern>();
            _sampleOrigins = new Dictionary<Food, List<ISampleOrigin>>();
            _foodApple = null;
            _foodBananas = null;
            _foodPineapple = null;
            _compoundA = null;
            _compoundB = null;
            _compoundC = null;
            _compoundD = null;
        }

        private void populateDefaultTestData() {
            //create a string of uses, per line:
            //AgriculturalUseCode|FoodCode|SubstanceCode,SubstanceCode,...|OccurrenceFraction|Location \n
            var agriculturalUseData =
                "grpA  |Apple    |substA              |0.10|NL\n" +
                "grpAB |Apple    |substA,substB       |0.15|NL\n" +
                "grpABC|Apple    |substA,substB,substC|0.20|NL\n" +
                "grpA  |Bananas  |substA              |0.05|NL\n" +
                "grpABC|Bananas  |substA,substB,substC|0.10|NL\n" +
                "grpA  |Pineapple|substA              |0.05   \n" +
                "grpA  |Pineapple|substA              |0.10|NL\n" +
                "grpAB |Pineapple|substA,substB       |0.15   \n" +
                "grpABC|Pineapple|substA,substB,substC|0.20|DE  ";

            populateAgriculturalUsesFromString(agriculturalUseData);

            _foodApple = _foods["Apple"];
            _foodBananas = _foods["Bananas"];
            _foodPineapple = _foods["Pineapple"];
            _compoundA = _substances["substA"];
            _compoundB = _substances["substB"];
            _compoundC = _substances["substC"];
            //add an extra compound
            _compoundD = new Compound("substD");
            _substances.Add("substD", _compoundD);

            //Create sample origin data
            _sampleOrigins = new Dictionary<Food, List<ISampleOrigin>> {
                {_foodApple, new List<ISampleOrigin> {
                    new SampleOriginRecord { Food = _foodApple, Location = "NL", Fraction = 1F, NumberOfSamples = 5 },
                    new SampleOriginRecord { Food = _foodApple, Location = null, Fraction = 0F, NumberOfSamples = 0 }
                }},
                {_foodBananas, new List<ISampleOrigin> {
                    new SampleOriginRecord { Food = _foodBananas, Location = "DE", Fraction = 1F, NumberOfSamples = 5 },
                    new SampleOriginRecord { Food = _foodBananas, Location = null, Fraction = 0F, NumberOfSamples = 0 }
                }},
                {_foodPineapple, new List<ISampleOrigin> {
                    new SampleOriginRecord { Food = _foodPineapple, Location = "NL", Fraction = 0.1F, NumberOfSamples = 1 },
                    new SampleOriginRecord { Food = _foodPineapple, Location = "DE", Fraction = 0.3F, NumberOfSamples = 3 },
                    new SampleOriginRecord { Food = _foodPineapple, Location = null, Fraction = 0.6F, NumberOfSamples = 6 }
                }},
            };
        }

        /// <summary>
        /// Checks all food-substance combinations
        /// </summary>
        [TestMethod]
        public void OccurrenceFrequenciesCalculator_ComputeTest1() {
            var settings = new OccurrenceFractionsCalculatorSettings(new AgriculturalUseSettingsDto() {
                SetMissingAgriculturalUseAsUnauthorized = false,
                UseAgriculturalUsePercentage = false
            });
            var calculator = new OccurrenceFractionsCalculator(settings);
            //create a string of uses, per line:
            //AgriculturalUseCode|FoodCode|SubstanceCode,SubstanceCode,...|OccurrenceFraction \n
            var s = "AG1|fA|sA,sB,sC|0\n" +
                    "AG2|fB|sA,sD,sE|0\n" +
                    "AG3|fB|sA,sD,sX|0";

            populateAgriculturalUsesFromString(s);
            var result = calculator.Compute(_foods.Values, _substances.Values, _agriculturalUses.Cast<MarginalOccurrencePattern>().ToList());

            Assert.AreEqual(12, result.Count);
            //check all food-substance combinations, concatenated codes separated by comma
            Assert.AreEqual(
                "fAsA,fAsB,fAsC,fAsD,fAsE,fAsX,fBsA,fBsB,fBsC,fBsD,fBsE,fBsX",
                string.Join(",", result.Keys.Select(k => k.Food.Code + k.Substance.Code).Distinct())
            );
        }

        /// <summary>
        /// Checks all food-substance combinations
        /// </summary>
        [TestMethod]
        public void OccurrenceFrequenciesCalculator_ComputeTest2() {

            var settings = new OccurrenceFractionsCalculatorSettings(new AgriculturalUseSettingsDto() {
                SetMissingAgriculturalUseAsUnauthorized = false,
                UseAgriculturalUsePercentage = false
            });
            var calculator = new OccurrenceFractionsCalculator(settings);
            //create a string of uses, per line:
            //AgriculturalUseCode|FoodCode|SubstanceCode,SubstanceCode,...|OccurrenceFraction \n
            var s = "AG1|fA|sA,sB,sC|0\n" +
                    "AG2|fB|sA,sD,sE|0\n" +
                    "AG3|fB|sA,sD,sX|0";

            populateAgriculturalUsesFromString(s);
            //add some extra foods and substances
            _foods.Add("fC", new Food("fC"));
            _foods.Add("fZ", new Food("fZ"));
            _substances.Add("sQ", new Compound("sQ"));
            _substances.Add("sZ", new Compound("sZ"));

            var result = calculator.Compute(_foods.Values, _substances.Values, _agriculturalUses.Cast<MarginalOccurrencePattern>().ToList());

            //extra substances are listed
            Assert.AreEqual(16, result.Count);
            //check all food-substance combinations, concatenated codes separated by comma
            Assert.AreEqual(
                "fAsA,fAsB,fAsC,fAsD,fAsE,fAsX,fAsQ,fAsZ,fBsA,fBsB,fBsC,fBsD,fBsE,fBsX,fBsQ,fBsZ",
                string.Join(",", result.Keys.Select(k => k.Food.Code + k.Substance.Code).Distinct())
            );
        }

        /// <summary>
        /// OccurrenceFrequenciesCalculator_ComputeWithOccurrenceTest1
        /// And some summarize sections: AgriculturalUseByFoodSubstanceSummarySection
        ///                         and: AgriculturalUseMixtureSummarySection
        /// </summary>
        [TestMethod]
        public void OccurrenceFrequenciesCalculator_ComputeWithOccurrenceTest1() {
            var settings = new OccurrenceFractionsCalculatorSettings(new AgriculturalUseSettingsDto() {
                SetMissingAgriculturalUseAsUnauthorized = false,
                UseAgriculturalUsePercentage = false
            });
            var calculator = new OccurrenceFractionsCalculator(settings);
            //create a string of uses, per line:
            //AgriculturalUseCode|FoodCode|SubstanceCode,SubstanceCode,...|OccurrenceFraction \n
            var s = "AG1|fA|sA,sB,sC|0.1\n" +
                    "AG3|fB|sA,sD,sX|0.3\n" +
                    "AG4|fB|sA,sB,sX|0.4";

            populateAgriculturalUsesFromString(s);
            //add some extra foods and substances
            var result = calculator.Compute(_foods.Values, _substances.Values, _agriculturalUses.Cast<MarginalOccurrencePattern>().ToList());

            //extra substances are listed
            Assert.AreEqual(10, result.Count);
            //check all food-substance combinations, concatenated codes, followed by colon and
            //WeightedAgriculturalUseFraction separated by comma
            Assert.AreEqual(
                "fAsA:1,fAsB:1,fAsC:1,fAsD:0,fAsX:0,fBsA:1,fBsB:1,fBsC:0,fBsD:1,fBsX:1",
                string.Join(",", result
                    .Select(
                        k => $"{k.Key.Food.Code}{k.Key.Substance.Code}:{k.Value.OccurrenceFrequency}")
                        .Distinct())
            );


            var substanceAuthorisations = new Dictionary<(Food, Compound), SubstanceAuthorisation>();
            foreach (var food in _foods.Values) {
                foreach (var substance in _substances.Values) {
                    substanceAuthorisations[(food, substance)] = new SubstanceAuthorisation() {
                        Food = food,
                        Substance = substance,
                        Reference = ""
                    };
                }
            }

            var section = new OccurrenceFrequenciesSummarySection();
            section.Summarize(result, substanceAuthorisations);

            var sUnc = "AG1|fA|sA,sB,sC|0.11\n" +
                   "AG3|fB|sA,sD,sX|0.28\n" +
                   "AG4|fB|sA,sB,sX|0.41";

            populateAgriculturalUsesFromString(sUnc);
            //add some extra foods and substances
            var resultUnc = calculator.Compute(_foods.Values, _substances.Values, _agriculturalUses.Cast<MarginalOccurrencePattern>().ToList());

            section.SummarizeUncertain(resultUnc);

            var section1 = new OccurrencePatternMixtureSummarySection();
            section1.Summarize(_agriculturalUses.Cast<MarginalOccurrencePattern>().ToList());



        }

        /// <summary>
        /// Checks all food-substance combinations
        /// </summary>
        [TestMethod]
        public void OccurrenceFrequenciesCalculator_ComputeWithOccurrenceTest2() {
            var settings = new OccurrenceFractionsCalculatorSettings(new AgriculturalUseSettingsDto() {
                SetMissingAgriculturalUseAsUnauthorized = false,
                UseAgriculturalUsePercentage = true
            });
            var calculator = new OccurrenceFractionsCalculator(settings);
            //create a string of uses, per line:
            //AgriculturalUseCode|FoodCode|SubstanceCode,SubstanceCode,...|OccurrenceFraction \n
            var s = "AG1|fA|sA,sB,sC|0.1\n" +
                    "AG3|fB|sA,sD,sX|0.3\n" +
                    "AG4|fB|sA,sB,sX|0.4";

            populateAgriculturalUsesFromString(s);
            //add some extra foods and substances
            var result = calculator.Compute(_foods.Values, _substances.Values, _agriculturalUses.Cast<MarginalOccurrencePattern>().ToList());

            //extra substances are listed
            Assert.AreEqual(10, result.Count);
            //check all food-substance combinations, concatenated codes, followed by colon and
            //WeightedAgriculturalUseFraction separated by comma
            Assert.AreEqual(
                "fAsA:0.1,fAsB:0.1,fAsC:0.1,fAsD:0,fAsX:0,fBsA:0.7,fBsB:0.4,fBsC:0,fBsD:0.3,fBsX:0.7",
                string.Join(",", result
                    .Select(
                        k => $"{k.Key.Food.Code}{k.Key.Substance.Code}:{k.Value.OccurrenceFrequency.ToString(NumberFormatInfo.InvariantInfo)}")
                        .Distinct())
            );
            Assert.IsTrue(result.All(r => r.Value.LocationOccurrenceFractions.Count == 1));
            Assert.IsTrue(result.All(r => r.Value.LocationOccurrenceFractions[""].FractionAllSamples == 1));
            Assert.IsTrue(result.All(r => r.Value.LocationOccurrenceFractions[""].SubstanceUseFound == (r.Value.OccurrenceFrequency > 0)));
            Assert.IsTrue(result.All(r => r.Value.LocationOccurrenceFractions[""].OccurrenceFraction == r.Value.OccurrenceFrequency));
        }

        /// <summary>
        /// Checks all food-substance combinations, location based
        /// </summary>
        [TestMethod]
        public void OccurrenceFrequenciesCalculator_ComputeLocationBasedTest1() {
            var settings = new OccurrenceFractionsCalculatorSettings(new AgriculturalUseSettingsDto() {
                SetMissingAgriculturalUseAsUnauthorized = false,
                UseAgriculturalUsePercentage = true
            });
            var calculator = new OccurrenceFractionsCalculator(settings);
            //create a string of uses, per line:
            //AgriculturalUseCode|FoodCode|SubstanceCode,SubstanceCode,...|OccurrenceFraction \n
            var s = "AG1|fA|sA,sB,sC|0.1\n" +
                    "AG3|fB|sA,sD,sX|0.3\n" +
                    "AG4|fB|sA,sB,sX|0.4";

            populateAgriculturalUsesFromString(s);
            //add some extra foods and substances
            var result = calculator.ComputeLocationBased(_foods.Values, _substances.Values, _agriculturalUses, new Dictionary<Food, List<ISampleOrigin>>());

            //extra substances are listed
            Assert.AreEqual(10, result.Count);
            //check all food-substance combinations, concatenated codes, followed by colon and
            //WeightedAgriculturalUseFraction separated by comma
            Assert.AreEqual(
                "fAsA:1,fAsB:1,fAsC:1,fAsD:1,fAsX:1,fBsA:1,fBsB:1,fBsC:1,fBsD:1,fBsX:1",
                string.Join(",", result
                    .Select(
                        k => $"{k.Key.Food.Code}{k.Key.Substance.Code}:{k.Value.OccurrenceFrequency.ToString(NumberFormatInfo.InvariantInfo)}")
                        .Distinct())
            );
            Assert.IsTrue(result.All(r => r.Value.LocationOccurrenceFractions.Count == 0));
        }

        /// <summary>
        /// Checks all food-substance combinations, location based
        /// </summary>
        [TestMethod]
        public void OccurrenceFrequenciesCalculator_ComputeLocationBasedTest2() {
            var settings = new OccurrenceFractionsCalculatorSettings(new AgriculturalUseSettingsDto() {
                SetMissingAgriculturalUseAsUnauthorized = false,
                UseAgriculturalUsePercentage = true
            });
            var calculator = new OccurrenceFractionsCalculator(settings);
            //create a string of uses, per line:
            //AgriculturalUseCode|FoodCode|SubstanceCode,SubstanceCode,...|OccurrenceFraction \n
            var s = "AG1|fA|sA,sB|0.2\n" +
                    "AG2|fB|sA,sD|0.4";

            populateAgriculturalUsesFromString(s);
            //add some extra foods and substances
            var origins = new Dictionary<Food, List<ISampleOrigin>> {
                {_foods["fA"], new List<ISampleOrigin> {
                    new SampleOriginRecord { Food = _foods["fA"], Location = "NL", Fraction = 0.04F, NumberOfSamples = 1 },
                    new SampleOriginRecord { Food = _foods["fA"], Location = "DE", Fraction = 0.03F, NumberOfSamples = 2 }
                }},
                {_foods["fB"], new List<ISampleOrigin> {
                    new SampleOriginRecord { Food = _foods["fB"], Location = "NL", Fraction = 0.02F, NumberOfSamples = 3 },
                    new SampleOriginRecord { Food = _foods["fB"], Location = "DE", Fraction = 0.01F, NumberOfSamples = 4 }
                }},
            };

            var result = calculator.ComputeLocationBased(_foods.Values, _substances.Values, _agriculturalUses, origins);

            //extra substances are listed
            Assert.AreEqual(6, result.Count);
            Assert.IsTrue(result.All(r => r.Value.LocationOccurrenceFractions.Count == 2));
            Assert.IsTrue(result.All(r => r.Value.LocationOccurrenceFractions.ContainsKey("NL")));
            Assert.IsTrue(result.All(r => r.Value.LocationOccurrenceFractions.ContainsKey("DE")));
            Assert.AreEqual(1.00, result[(_foods["fA"], _substances["sA"])].LocationOccurrenceFractions["NL"].OccurrenceFraction, _epsilon);
            Assert.AreEqual(1.00, result[(_foods["fA"], _substances["sB"])].LocationOccurrenceFractions["NL"].OccurrenceFraction, _epsilon);
            Assert.AreEqual(0.80, result[(_foods["fA"], _substances["sD"])].LocationOccurrenceFractions["NL"].OccurrenceFraction, _epsilon);
            Assert.AreEqual(1.00, result[(_foods["fB"], _substances["sA"])].LocationOccurrenceFractions["NL"].OccurrenceFraction, _epsilon);
            Assert.AreEqual(0.60, result[(_foods["fB"], _substances["sB"])].LocationOccurrenceFractions["NL"].OccurrenceFraction, _epsilon);
            Assert.AreEqual(1.00, result[(_foods["fB"], _substances["sD"])].LocationOccurrenceFractions["NL"].OccurrenceFraction, _epsilon);
            Assert.AreEqual(0.04, result[(_foods["fA"], _substances["sA"])].LocationOccurrenceFractions["NL"].FractionAllSamples, _epsilon);
            Assert.AreEqual(0.04, result[(_foods["fA"], _substances["sB"])].LocationOccurrenceFractions["NL"].FractionAllSamples, _epsilon);
            Assert.AreEqual(0.04, result[(_foods["fA"], _substances["sD"])].LocationOccurrenceFractions["NL"].FractionAllSamples, _epsilon);
            Assert.AreEqual(0.02, result[(_foods["fB"], _substances["sA"])].LocationOccurrenceFractions["NL"].FractionAllSamples, _epsilon);
            Assert.AreEqual(0.02, result[(_foods["fB"], _substances["sB"])].LocationOccurrenceFractions["NL"].FractionAllSamples, _epsilon);
            Assert.AreEqual(0.02, result[(_foods["fB"], _substances["sD"])].LocationOccurrenceFractions["NL"].FractionAllSamples, _epsilon);

            Assert.AreEqual(1.00, result[(_foods["fA"], _substances["sA"])].LocationOccurrenceFractions["DE"].OccurrenceFraction, _epsilon);
            Assert.AreEqual(1.00, result[(_foods["fA"], _substances["sB"])].LocationOccurrenceFractions["DE"].OccurrenceFraction, _epsilon);
            Assert.AreEqual(0.80, result[(_foods["fA"], _substances["sD"])].LocationOccurrenceFractions["DE"].OccurrenceFraction, _epsilon);
            Assert.AreEqual(1.00, result[(_foods["fB"], _substances["sA"])].LocationOccurrenceFractions["DE"].OccurrenceFraction, _epsilon);
            Assert.AreEqual(0.60, result[(_foods["fB"], _substances["sB"])].LocationOccurrenceFractions["DE"].OccurrenceFraction, _epsilon);
            Assert.AreEqual(1.00, result[(_foods["fB"], _substances["sD"])].LocationOccurrenceFractions["DE"].OccurrenceFraction, _epsilon);
            Assert.AreEqual(0.03, result[(_foods["fA"], _substances["sA"])].LocationOccurrenceFractions["DE"].FractionAllSamples, _epsilon);
            Assert.AreEqual(0.03, result[(_foods["fA"], _substances["sB"])].LocationOccurrenceFractions["DE"].FractionAllSamples, _epsilon);
            Assert.AreEqual(0.03, result[(_foods["fA"], _substances["sD"])].LocationOccurrenceFractions["DE"].FractionAllSamples, _epsilon);
            Assert.AreEqual(0.01, result[(_foods["fB"], _substances["sA"])].LocationOccurrenceFractions["DE"].FractionAllSamples, _epsilon);
            Assert.AreEqual(0.01, result[(_foods["fB"], _substances["sB"])].LocationOccurrenceFractions["DE"].FractionAllSamples, _epsilon);
            Assert.AreEqual(0.01, result[(_foods["fB"], _substances["sD"])].LocationOccurrenceFractions["DE"].FractionAllSamples, _epsilon);

        }

        /// <summary>
        /// OccurrenceFrequenciesCalculator_ComputeMissingUnAuthorizedTest
        /// Test correct processing of agricultural uses for the food apple, bananas and pineapple, and substances A,
        /// B, C, and D. Tests the weighted agricultural use percentages per substance, per location
        /// and test the aggregated (over all locations) weighted agricultural use percentage.
        /// UseAgriculturalUseTable = true, UseAgriculturalUsePercentage = true,
        /// SetMissingAgriculturalUseAsUnauthorized = true.
        /// </summary>
        [TestMethod]
        public void OccurrenceFrequenciesCalculator_ComputeMissingUnAuthorizedTest() {
            var settings = new OccurrenceFractionsCalculatorSettings(new AgriculturalUseSettingsDto() {
                SetMissingAgriculturalUseAsUnauthorized = true,
                UseAgriculturalUsePercentage = true
            });
            var calculator = new OccurrenceFractionsCalculator(settings);
            populateDefaultTestData();
            var result = calculator.ComputeLocationBased(_foods.Values, _substances.Values, _agriculturalUses, _sampleOrigins);

            // Get the substance residue collections for APPLE - Compound A
            Assert.AreEqual(2, result[(_foodApple, _compoundA)].LocationOccurrenceFractions.Count);
            Assert.IsTrue(result[(_foodApple, _compoundA)].UseFound("NL"));
            Assert.AreEqual(0.45, result[(_foodApple, _compoundA)].GetOccurrenceFrequencyForLocation("NL"), _epsilon);
            Assert.IsFalse(result[(_foodApple, _compoundA)].UseFound("undefined"));
            Assert.AreEqual(0D, result[(_foodApple, _compoundA)].GetOccurrenceFrequencyForLocation("undefined"), _epsilon);
            Assert.AreEqual(0.45, result[(_foodApple, _compoundA)].OccurrenceFrequency, _epsilon);

            // Get the substance residue collections for APPLE - Compound B
            Assert.AreEqual(2, result[(_foodApple, _compoundB)].LocationOccurrenceFractions.Count);
            Assert.IsTrue(result[(_foodApple, _compoundB)].UseFound("NL"));
            Assert.AreEqual(0.35, result[(_foodApple, _compoundB)].GetOccurrenceFrequencyForLocation("NL"), _epsilon);
            Assert.IsFalse(result[(_foodApple, _compoundB)].UseFound("undefined"));
            Assert.AreEqual(0D, result[(_foodApple, _compoundB)].GetOccurrenceFrequencyForLocation("undefined"), _epsilon);
            Assert.AreEqual(0.35, result[(_foodApple, _compoundB)].OccurrenceFrequency, _epsilon);

            // Get the substance residue collections for APPLE - Compound C
            Assert.AreEqual(2, result[(_foodApple, _compoundC)].LocationOccurrenceFractions.Count);
            Assert.IsTrue(result[(_foodApple, _compoundC)].UseFound("NL"));
            Assert.AreEqual(0.2, result[(_foodApple, _compoundC)].GetOccurrenceFrequencyForLocation("NL"), _epsilon);
            Assert.IsFalse(result[(_foodApple, _compoundC)].UseFound("undefined"));
            Assert.AreEqual(0D, result[(_foodApple, _compoundC)].GetOccurrenceFrequencyForLocation("undefined"), _epsilon);
            Assert.AreEqual(0.2, result[(_foodApple, _compoundC)].OccurrenceFrequency, _epsilon);

            // Get the substance residue collections for APPLE - Compound D
            Assert.AreEqual(2, result[(_foodApple, _compoundD)].LocationOccurrenceFractions.Count);
            Assert.IsFalse(result[(_foodApple, _compoundD)].UseFound("NL"));
            Assert.AreEqual(0D, result[(_foodApple, _compoundD)].GetOccurrenceFrequencyForLocation("NL"), _epsilon);
            Assert.IsFalse(result[(_foodApple, _compoundD)].UseFound("undefined"));
            Assert.AreEqual(0D, result[(_foodApple, _compoundD)].GetOccurrenceFrequencyForLocation("undefined"), _epsilon);
            Assert.AreEqual(0D, result[(_foodApple, _compoundD)].OccurrenceFrequency, _epsilon);

            // Get the substance residue collections for BANANAS - Compound A
            Assert.AreEqual(2, result[(_foodBananas, _compoundA)].LocationOccurrenceFractions.Count);
            Assert.IsFalse(result[(_foodBananas, _compoundA)].UseFound("DE"));
            Assert.AreEqual(0D, result[(_foodBananas, _compoundA)].GetOccurrenceFrequencyForLocation("DE"), _epsilon);
            Assert.IsFalse(result[(_foodBananas, _compoundA)].UseFound("undefined"));
            Assert.AreEqual(0D, result[(_foodBananas, _compoundA)].GetOccurrenceFrequencyForLocation("undefined"), _epsilon);
            Assert.AreEqual(0D, result[(_foodBananas, _compoundA)].OccurrenceFrequency, _epsilon);

            // Get the substance residue collections for BANANAS - Compound B
            Assert.AreEqual(2, result[(_foodBananas, _compoundB)].LocationOccurrenceFractions.Count);
            Assert.IsFalse(result[(_foodBananas, _compoundB)].UseFound("DE"));
            Assert.AreEqual(0D, result[(_foodBananas, _compoundB)].GetOccurrenceFrequencyForLocation("DE"), _epsilon);
            Assert.IsFalse(result[(_foodBananas, _compoundB)].UseFound("undefined"));
            Assert.AreEqual(0D, result[(_foodBananas, _compoundB)].GetOccurrenceFrequencyForLocation("undefined"), _epsilon);
            Assert.AreEqual(0D, result[(_foodBananas, _compoundB)].OccurrenceFrequency, _epsilon);

            // Get the substance residue collections for BANANAS - Compound C
            Assert.AreEqual(2, result[(_foodBananas, _compoundC)].LocationOccurrenceFractions.Count);
            Assert.IsFalse(result[(_foodBananas, _compoundC)].UseFound("undefined"));
            Assert.AreEqual(0D, result[(_foodBananas, _compoundC)].GetOccurrenceFrequencyForLocation("undefined"), _epsilon);
            Assert.AreEqual(0D, result[(_foodBananas, _compoundC)].OccurrenceFrequency, _epsilon);

            // Get the substance residue collections for BANANAS - Compound D
            Assert.AreEqual(2, result[(_foodBananas, _compoundD)].LocationOccurrenceFractions.Count);
            Assert.IsFalse(result[(_foodBananas, _compoundD)].UseFound("undefined"));
            Assert.AreEqual(0D, result[(_foodBananas, _compoundD)].GetOccurrenceFrequencyForLocation("undefined"), _epsilon);
            Assert.AreEqual(0D, result[(_foodBananas, _compoundD)].OccurrenceFrequency, _epsilon);

            // Get the substance residue collections for PINEAPPLE - Compound A
            Assert.AreEqual(3, result[(_foodPineapple, _compoundA)].LocationOccurrenceFractions.Count);
            Assert.IsTrue(result[(_foodPineapple, _compoundA)].UseFound("NL"));
            Assert.AreEqual(0.25, result[(_foodPineapple, _compoundA)].GetOccurrenceFrequencyForLocation("NL"), _epsilon);
            Assert.IsTrue(result[(_foodPineapple, _compoundA)].UseFound("DE"));
            Assert.AreEqual(0.4, result[(_foodPineapple, _compoundA)].GetOccurrenceFrequencyForLocation("DE"), _epsilon);
            Assert.IsTrue(result[(_foodPineapple, _compoundA)].UseFound("undefined"));
            Assert.AreEqual(0.2, result[(_foodPineapple, _compoundA)].GetOccurrenceFrequencyForLocation("undefined"), _epsilon);
            Assert.AreEqual(0.265, result[(_foodPineapple, _compoundA)].OccurrenceFrequency, _epsilon);

            // Get the substance residue collections for PINEAPPLE - Compound B
            Assert.AreEqual(3, result[(_foodPineapple, _compoundB)].LocationOccurrenceFractions.Count);
            Assert.IsTrue(result[(_foodPineapple, _compoundB)].UseFound("undefined"));
            Assert.AreEqual(0.15, result[(_foodPineapple, _compoundB)].GetOccurrenceFrequencyForLocation("undefined"));
            Assert.AreEqual(0.21, result[(_foodPineapple, _compoundB)].OccurrenceFrequency, _epsilon);

            // Get the substance residue collections for PINEAPPLE - Compound C
            Assert.AreEqual(3, result[(_foodPineapple, _compoundC)].LocationOccurrenceFractions.Count);
            Assert.IsFalse(result[(_foodPineapple, _compoundC)].UseFound("NL"));
            Assert.AreEqual(0D, result[(_foodPineapple, _compoundC)].GetOccurrenceFrequencyForLocation("NL"), _epsilon);
            Assert.IsFalse(result[(_foodPineapple, _compoundC)].UseFound("undefined"));
            Assert.AreEqual(0D, result[(_foodPineapple, _compoundC)].GetOccurrenceFrequencyForLocation("undefined"), _epsilon);
            Assert.AreEqual(0.06, result[(_foodPineapple, _compoundC)].OccurrenceFrequency, _epsilon);

            // Get the substance residue collections for PINEAPPLE - Compound D
            Assert.AreEqual(3, result[(_foodPineapple, _compoundD)].LocationOccurrenceFractions.Count);
            Assert.IsFalse(result[(_foodPineapple, _compoundD)].UseFound("NL"));
            Assert.AreEqual(0D, result[(_foodPineapple, _compoundD)].GetOccurrenceFrequencyForLocation("NL"), _epsilon);
            Assert.IsFalse(result[(_foodPineapple, _compoundD)].UseFound("undefined"));
            Assert.AreEqual(0D, result[(_foodPineapple, _compoundD)].GetOccurrenceFrequencyForLocation("undefined"), _epsilon);
            Assert.AreEqual(0D, result[(_foodPineapple, _compoundD)].OccurrenceFrequency, _epsilon);
        }


        /// <summary>
        /// OccurrenceFrequenciesCalculator_ComputeMissingAuthorizedTest
        /// Test correct processing of agricultural uses for the food apple, bananas and pineapple, and substances A,
        /// B, C, and D. Tests the weighted agricultural use percentages per substance, per location
        /// and test the aggregated (over all locations) weighted agricultural use percentage.
        /// UseAgriculturalUseTable = true, UseAgriculturalUsePercentage = true,
        /// SetMissingAgriculturalUseAsUnauthorized = false.
        /// </summary>
        [TestMethod]
        public void OccurrenceFrequenciesCalculator_ComputeMissingAuthorizedTest() {

            var settings = new OccurrenceFractionsCalculatorSettings(new AgriculturalUseSettingsDto() {
                SetMissingAgriculturalUseAsUnauthorized = false,
                UseAgriculturalUsePercentage = true
            });
            var calculator = new OccurrenceFractionsCalculator(settings);
            populateDefaultTestData();
            var result = calculator.ComputeLocationBased(_foods.Values, _substances.Values, _agriculturalUses, _sampleOrigins);

            // Get the substance residue collections for APPLE - Compound A
            Assert.AreEqual(2, result[(_foodApple, _compoundA)].LocationOccurrenceFractions.Count);
            Assert.IsTrue(result[(_foodApple, _compoundA)].UseFound("NL"));
            Assert.AreEqual(0.45 + 0.55, result[(_foodApple, _compoundA)].GetOccurrenceFrequencyForLocation("NL"), _epsilon);
            Assert.IsTrue(result[(_foodApple, _compoundA)].UseFound("undefined"));
            Assert.AreEqual(1D, result[(_foodApple, _compoundA)].GetOccurrenceFrequencyForLocation("undefined"), _epsilon);
            Assert.AreEqual(0.45 + 0.55, result[(_foodApple, _compoundA)].OccurrenceFrequency, _epsilon);

            // Get the substance residue collections for APPLE - Compound B
            Assert.AreEqual(2, result[(_foodApple, _compoundB)].LocationOccurrenceFractions.Count);
            Assert.IsTrue(result[(_foodApple, _compoundB)].UseFound("NL"));
            Assert.AreEqual(0.35 + 0.55, result[(_foodApple, _compoundB)].GetOccurrenceFrequencyForLocation("NL"), _epsilon);
            Assert.IsTrue(result[(_foodApple, _compoundB)].UseFound("undefined"));
            Assert.AreEqual(1D, result[(_foodApple, _compoundB)].GetOccurrenceFrequencyForLocation("undefined"), _epsilon);
            Assert.AreEqual(0.35 + 0.55, result[(_foodApple, _compoundB)].OccurrenceFrequency, _epsilon);

            // Get the substance residue collections for APPLE - Compound C
            Assert.AreEqual(2, result[(_foodApple, _compoundC)].LocationOccurrenceFractions.Count);
            Assert.IsTrue(result[(_foodApple, _compoundC)].UseFound("NL"));
            Assert.AreEqual(0.2 + 0.55, result[(_foodApple, _compoundC)].GetOccurrenceFrequencyForLocation("NL"), _epsilon);
            Assert.IsTrue(result[(_foodApple, _compoundC)].UseFound("undefined"));
            Assert.AreEqual(1D, result[(_foodApple, _compoundC)].GetOccurrenceFrequencyForLocation("undefined"), _epsilon);
            Assert.AreEqual(0.2 + 0.55, result[(_foodApple, _compoundC)].OccurrenceFrequency, _epsilon);

            // Get the substance residue collections for APPLE - Compound D
            Assert.AreEqual(2, result[(_foodApple, _compoundD)].LocationOccurrenceFractions.Count);
            Assert.IsTrue(result[(_foodApple, _compoundD)].UseFound("NL"));
            Assert.AreEqual(0D + 0.55, result[(_foodApple, _compoundD)].GetOccurrenceFrequencyForLocation("NL"), _epsilon);
            Assert.IsTrue(result[(_foodApple, _compoundD)].UseFound("undefined"));
            Assert.AreEqual(1D, result[(_foodApple, _compoundD)].GetOccurrenceFrequencyForLocation("undefined"), _epsilon);
            Assert.AreEqual(0D + 0.55, result[(_foodApple, _compoundD)].OccurrenceFrequency, _epsilon);

            // Get the substance residue collections for BANANAS - Compound A
            Assert.AreEqual(2, result[(_foodBananas, _compoundA)].LocationOccurrenceFractions.Count);
            Assert.IsTrue(result[(_foodBananas, _compoundA)].UseFound("DE"));
            Assert.AreEqual(1D, result[(_foodBananas, _compoundA)].GetOccurrenceFrequencyForLocation("DE"), _epsilon);
            Assert.IsTrue(result[(_foodBananas, _compoundA)].UseFound("undefined"));
            Assert.AreEqual(1D, result[(_foodBananas, _compoundA)].GetOccurrenceFrequencyForLocation("undefined"), _epsilon);
            Assert.AreEqual(1D, result[(_foodBananas, _compoundA)].OccurrenceFrequency, _epsilon);

            // Get the substance residue collections for BANANAS - Compound B
            Assert.AreEqual(2, result[(_foodBananas, _compoundB)].LocationOccurrenceFractions.Count);
            Assert.IsTrue(result[(_foodBananas, _compoundB)].UseFound("DE"));
            Assert.AreEqual(1D, result[(_foodBananas, _compoundB)].GetOccurrenceFrequencyForLocation("DE"), _epsilon);
            Assert.IsTrue(result[(_foodBananas, _compoundB)].UseFound("undefined"));
            Assert.AreEqual(1D, result[(_foodBananas, _compoundB)].GetOccurrenceFrequencyForLocation("undefined"), _epsilon);
            Assert.AreEqual(1D, result[(_foodBananas, _compoundB)].OccurrenceFrequency, _epsilon);

            // Get the substance residue collections for BANANAS - Compound C
            Assert.AreEqual(2, result[(_foodBananas, _compoundC)].LocationOccurrenceFractions.Count);
            Assert.IsTrue(result[(_foodBananas, _compoundC)].UseFound("undefined"));
            Assert.AreEqual(1D, result[(_foodBananas, _compoundC)].GetOccurrenceFrequencyForLocation("undefined"), _epsilon);
            Assert.AreEqual(1D, result[(_foodBananas, _compoundC)].OccurrenceFrequency, _epsilon);

            // Get the substance residue collections for BANANAS - Compound D
            Assert.AreEqual(2, result[(_foodBananas, _compoundD)].LocationOccurrenceFractions.Count);
            Assert.IsTrue(result[(_foodBananas, _compoundD)].UseFound("undefined"));
            Assert.AreEqual(1D, result[(_foodBananas, _compoundD)].GetOccurrenceFrequencyForLocation("undefined"), _epsilon);
            Assert.AreEqual(1D, result[(_foodBananas, _compoundD)].OccurrenceFrequency, _epsilon);

            // Get the substance residue collections for PINEAPPLE - Compound A
            Assert.AreEqual(3, result[(_foodPineapple, _compoundA)].LocationOccurrenceFractions.Count);
            Assert.IsTrue(result[(_foodPineapple, _compoundA)].UseFound("NL"));
            Assert.AreEqual(1D, result[(_foodPineapple, _compoundA)].GetOccurrenceFrequencyForLocation("NL"), _epsilon);
            Assert.IsTrue(result[(_foodPineapple, _compoundA)].UseFound("DE"));
            Assert.AreEqual(1D, result[(_foodPineapple, _compoundA)].GetOccurrenceFrequencyForLocation("DE"), _epsilon);
            Assert.IsTrue(result[(_foodPineapple, _compoundA)].UseFound("undefined"));
            Assert.AreEqual(1D, result[(_foodPineapple, _compoundA)].GetOccurrenceFrequencyForLocation("undefined"), _epsilon);
            Assert.AreEqual(1D, result[(_foodPineapple, _compoundA)].OccurrenceFrequency, _epsilon);

            // Get the substance residue collections for PINEAPPLE - Compound B
            Assert.AreEqual(3, result[(_foodPineapple, _compoundB)].LocationOccurrenceFractions.Count);
            Assert.IsTrue(result[(_foodPineapple, _compoundB)].UseFound("undefined"));
            Assert.AreEqual(0.95, result[(_foodPineapple, _compoundB)].GetOccurrenceFrequencyForLocation("undefined"), _epsilon);
            Assert.AreEqual(0.945, result[(_foodPineapple, _compoundB)].OccurrenceFrequency, _epsilon);

            // Get the substance residue collections for PINEAPPLE - Compound C
            Assert.AreEqual(3, result[(_foodPineapple, _compoundC)].LocationOccurrenceFractions.Count);
            Assert.IsTrue(result[(_foodPineapple, _compoundC)].UseFound("NL"));
            Assert.AreEqual(0.75, result[(_foodPineapple, _compoundC)].GetOccurrenceFrequencyForLocation("NL"), _epsilon);
            Assert.IsTrue(result[(_foodPineapple, _compoundC)].UseFound("undefined"));
            Assert.AreEqual(0.8, result[(_foodPineapple, _compoundC)].GetOccurrenceFrequencyForLocation("undefined"), _epsilon);
            Assert.AreEqual(0.795, result[(_foodPineapple, _compoundC)].OccurrenceFrequency, _epsilon);

            // Get the substance residue collections for PINEAPPLE - Compound D
            Assert.AreEqual(3, result[(_foodPineapple, _compoundD)].LocationOccurrenceFractions.Count);
            Assert.IsTrue(result[(_foodPineapple, _compoundD)].UseFound("NL"));
            Assert.AreEqual(0.75, result[(_foodPineapple, _compoundD)].GetOccurrenceFrequencyForLocation("NL"), _epsilon);
            Assert.IsTrue(result[(_foodPineapple, _compoundD)].UseFound("undefined"));
            Assert.AreEqual(0.8, result[(_foodPineapple, _compoundD)].GetOccurrenceFrequencyForLocation("undefined"), _epsilon);
            Assert.AreEqual(0.735, result[(_foodPineapple, _compoundD)].OccurrenceFrequency, _epsilon);
        }


        /// <summary>
        /// Test the general weighted agricultural use fractions for each of the agricultural
        /// use groups for the foods apple, bananas, and pineapple. I.e., the weighted (location-
        /// aggregated) percentage of agricultural use for the groups A, AB, and ABC.
        /// UseAgriculturalUseTable = true, UseAgriculturalUsePercentage = true,
        /// SetMissingAgriculturalUseAsUnauthorized = true.
        /// </summary>
        [TestMethod]
        public void AgriculturalUsesTestWeightedUseGroupFractions() {
            var settings = new OccurrenceFractionsCalculatorSettings(new AgriculturalUseSettingsDto() {
                SetMissingAgriculturalUseAsUnauthorized = false,
                UseAgriculturalUsePercentage = true
            });
            var calculator = new OccurrenceFractionsCalculator(settings);
            populateDefaultTestData();

            var agriculturalUseCalculator = new MarginalOccurrencePatternsCalculator();
            var agriculturalUseInfo = agriculturalUseCalculator.ComputeMarginalOccurrencePatterns(_foods.Values, _agriculturalUses, _sampleOrigins);

            // Get the substance residue collections for APPLE
            var agriculturalUseInfoApple = agriculturalUseInfo[_foodApple];
            Assert.IsTrue(agriculturalUseInfoApple.Count == 3);
            Assert.IsTrue(agriculturalUseInfoApple.First(r => r.Code == "grpA").OccurrenceFraction == 0.1);
            Assert.IsTrue(agriculturalUseInfoApple.First(r => r.Code == "grpAB").OccurrenceFraction == 0.15);
            Assert.IsTrue(agriculturalUseInfoApple.First(r => r.Code == "grpABC").OccurrenceFraction == 0.2);

            // Get the substance residue collections for BANANAS
            var agriculturalUseInfoBananas = agriculturalUseInfo[_foodBananas];
            Assert.IsTrue(agriculturalUseInfoBananas.Count == 0);

            // Get the substance residue collections for PINEAPPLE
            var agriculturalUseInfoPineapple = agriculturalUseInfo[_foodPineapple];
            Assert.AreEqual(agriculturalUseInfoPineapple.Count, 3);
            Assert.AreEqual(agriculturalUseInfoPineapple.First(r => r.Code == "grpA").OccurrenceFraction, 0.055, _epsilon);
            Assert.AreEqual(agriculturalUseInfoPineapple.First(r => r.Code == "grpAB").OccurrenceFraction, 0.15, _epsilon);
            Assert.AreEqual(agriculturalUseInfoPineapple.First(r => r.Code == "grpABC").OccurrenceFraction, 0.06, _epsilon);
        }


        /// <summary>
        /// Every line contains a record of:
        /// AgriculturalUseCode | FoodCode | SubstanceCodes separated by ,
        /// and optionally:
        /// | OccurrenceFraction | Location
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private void populateAgriculturalUsesFromString(string data) {
            var blocks = data.Split('\n');

            _foods = new Dictionary<string, Food>(StringComparer.OrdinalIgnoreCase);
            _substances = new Dictionary<string, Compound>(StringComparer.OrdinalIgnoreCase);
            _agriculturalUses = new List<MCO.OccurrencePattern>(blocks.Length);

            for (int i = 0; i < blocks.Length; i++) {
                var values = blocks[i].Split('|');

                var code = values[0].Trim();
                var foodCode = values[1].Trim();
                if (!_foods.TryGetValue(foodCode, out var food)) {
                    food = new Food(foodCode);
                    _foods.Add(foodCode, food);
                }
                var maUse = new MarginalOccurrencePattern { Code = code, Food = food, Location = "" };

                var substanceData = values[2].Split(',');
                for (int j = 0; j < substanceData.Length; j++) {
                    var substCode = substanceData[j].Trim();
                    if (!_substances.TryGetValue(substCode, out var substance)) {
                        substance = new Compound(substCode);
                        _substances.Add(substCode, substance);
                    }
                    maUse.Compounds.Add(substance);
                }
                //optional columns
                if (values.Length > 3) {
                    maUse.OccurrenceFraction = double.Parse(values[3].Trim(), NumberFormatInfo.InvariantInfo);
                }
                if (values.Length > 4 && !string.IsNullOrWhiteSpace(values[4])) {
                    maUse.Location = values[4].Trim();
                }
                _agriculturalUses.Add(maUse);
            }
        }
    }
}
