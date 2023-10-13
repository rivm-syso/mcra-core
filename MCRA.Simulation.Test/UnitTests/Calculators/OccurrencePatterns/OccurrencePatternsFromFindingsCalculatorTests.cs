using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers;
using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Calculators.OccurrencePatternsCalculation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.Calculators.OccurrencePatterns {
    /// <summary>
    /// OccurrencePatterns calculator
    /// </summary>
    [TestClass]
    public class OccurrencePatternsFromFindingsCalculatorTests {
        /// <summary>
        /// Test empty sets
        /// </summary>
        [TestMethod]
        public void OccurrencePatternsFromFindingsCalculator_TestComputeEmptySets() {
            var settings = new OccurrencePatternsFromFindingsCalculatorSettings(new AgriculturalUseSettings() {
                ScaleUpOccurencePatterns = true,
                RestrictOccurencePatternScalingToAuthorisedUses = true,
            });
            var calculator = new OccurrencePatternsFromFindingsCalculator(settings);
            var foods = new List<Food> { };
            var sampleCompounds = new Dictionary<Food, SampleCompoundCollection>();
            var result = calculator.Compute(foods, sampleCompounds);
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
            foods.Add(new Food());
            result = calculator.Compute(foods, sampleCompounds);
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(foods[0], result.Single().Food);
            Assert.AreEqual(0, result.Single().PositiveFindingsCount);
        }

        /// <summary>
        /// Test single positive
        /// </summary>
        [TestMethod]
        public void OccurrencePatternsFromFindingsCalculator_TestComputeSinglePositive() {
            var foodA = new Food("FoodA");
            var foods = new List<Food> { foodA };
            var collection = buildFoodSampleCompoundCollectionFromString(foodA, "1*0|A,0,0,1,1");
            var settings = new OccurrencePatternsFromFindingsCalculatorSettings(new AgriculturalUseSettings() {
                ScaleUpOccurencePatterns = false,
                RestrictOccurencePatternScalingToAuthorisedUses = false,

            });
            var calculator = new OccurrencePatternsFromFindingsCalculator(settings);

            var result = calculator.Compute(foods, collection);
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            var marginalUse = result.Single();

            Assert.AreEqual(1, marginalUse.Compounds.Count);
            Assert.AreEqual(foodA, marginalUse.Food);
            Assert.AreEqual(1, marginalUse.PositiveFindingsCount);
            Assert.AreEqual(1, marginalUse.OccurrenceFraction);
            Assert.AreEqual(1, marginalUse.AnalyticalScopeCount);
            Assert.AreEqual(false, marginalUse.AuthorisedUse);

        }
        /// <summary>
        /// Test single negative
        /// </summary>
        [TestMethod]
        public void OccurrencePatternsFromFindingsCalculator_TestComputeSingleNegative() {
            var foodA = new Food("FoodA");
            var foods = new List<Food> { foodA };
            var collection = buildFoodSampleCompoundCollectionFromString(
                food: foodA, data: "1*1|A,0,0,0,1"
             );
            var settings = new OccurrencePatternsFromFindingsCalculatorSettings(new AgriculturalUseSettings() {
                ScaleUpOccurencePatterns = false,
                RestrictOccurencePatternScalingToAuthorisedUses = false,

            });
            var calculator = new OccurrencePatternsFromFindingsCalculator(settings);

            var result = calculator.Compute(foods, collection);
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            var marginalUse = result.Single();

            Assert.AreEqual(0, marginalUse.Compounds.Count);
            Assert.AreEqual(foodA, marginalUse.Food);
            Assert.AreEqual(0, marginalUse.PositiveFindingsCount);
            Assert.AreEqual(1, marginalUse.OccurrenceFraction);
            Assert.AreEqual(0, marginalUse.AnalyticalScopeCount);
            Assert.AreEqual(true, marginalUse.AuthorisedUse);
        }

        /// <summary>
        /// Test single missing
        /// </summary>
        [TestMethod]
        public void OccurrencePatternsFromFindingsCalculator_TestComputeSingleMissing() {
            var foodA = new Food("FoodA");
            var foods = new List<Food> { foodA };
            var collection = buildFoodSampleCompoundCollectionFromString(
                foodA, "1*1|A,1,0,1,1"
             );
            var settings = new OccurrencePatternsFromFindingsCalculatorSettings(new AgriculturalUseSettings() {
                ScaleUpOccurencePatterns = false,
                RestrictOccurencePatternScalingToAuthorisedUses = false,

            });
            var calculator = new OccurrencePatternsFromFindingsCalculator(settings);

            var result = calculator.Compute(foods, collection);
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            var marginalUse = result.Single();

            Assert.AreEqual(0, marginalUse.Compounds.Count);
            Assert.AreEqual(foodA, marginalUse.Food);
            Assert.AreEqual(0, marginalUse.PositiveFindingsCount);
            Assert.AreEqual(1, marginalUse.OccurrenceFraction);
            Assert.AreEqual(0, marginalUse.AnalyticalScopeCount);
            Assert.AreEqual(true, marginalUse.AuthorisedUse);
        }

        /// <summary>
        /// Test occurrence fraction
        /// </summary>
        [TestMethod]
        public void OccurrencePatternsFromFindingsCalculator_TestComputeMultiple() {
            var foodA = new Food("FoodA");
            var foods = new List<Food> { foodA };
            var collection = buildFoodSampleCompoundCollectionFromString(
                foodA,
                "1*0|A,0,0,1,1;B,0,0,1,1;C,0,0,0,1|" +
                "1*0|A,0,0,0,1;B,0,0,0,1;C,0,0,0,1"
             );
            var settings = new OccurrencePatternsFromFindingsCalculatorSettings(new AgriculturalUseSettings() {
                ScaleUpOccurencePatterns = false,
                RestrictOccurencePatternScalingToAuthorisedUses = false,
            });
            var calculator = new OccurrencePatternsFromFindingsCalculator(settings);

            var result = calculator.Compute(foods, collection).ToArray();

            Assert.AreEqual(2, result.Length);

            var marginalUse = result[0];
            Assert.AreEqual(2, marginalUse.Compounds.Count);
            Assert.AreEqual(1, marginalUse.PositiveFindingsCount);
            Assert.AreEqual(.5, marginalUse.OccurrenceFraction);
            Assert.AreEqual(2, marginalUse.AnalyticalScopeCount);
            Assert.AreEqual(false, marginalUse.AuthorisedUse);
            marginalUse = result[1];
            Assert.AreEqual(0, marginalUse.Compounds.Count);
            Assert.AreEqual(0, marginalUse.PositiveFindingsCount);
            Assert.AreEqual(.5, marginalUse.OccurrenceFraction);
            Assert.AreEqual(0, marginalUse.AnalyticalScopeCount);
            Assert.AreEqual(true, marginalUse.AuthorisedUse);

            //with rescale
            settings = new OccurrencePatternsFromFindingsCalculatorSettings(new AgriculturalUseSettings() {
                ScaleUpOccurencePatterns = true,
                RestrictOccurencePatternScalingToAuthorisedUses = false,

            });
            calculator = new OccurrencePatternsFromFindingsCalculator(settings);
            result = calculator.Compute(foods, collection).ToArray();

            Assert.AreEqual(1, result.Length);

            marginalUse = result[0];

            Assert.AreEqual(2, marginalUse.Compounds.Count);
            Assert.AreEqual(1, marginalUse.PositiveFindingsCount);
            Assert.AreEqual(1, marginalUse.OccurrenceFraction);
            Assert.AreEqual(2, marginalUse.AnalyticalScopeCount);
            Assert.AreEqual(false, marginalUse.AuthorisedUse);

            //with rescale, only authorized
            settings = new OccurrencePatternsFromFindingsCalculatorSettings(new AgriculturalUseSettings() {
                ScaleUpOccurencePatterns = true,
                RestrictOccurencePatternScalingToAuthorisedUses = true,

            });
            calculator = new OccurrencePatternsFromFindingsCalculator(settings);
            result = calculator.Compute(foods, collection).ToArray();

            Assert.AreEqual(2, result.Length);

            marginalUse = result[0];
            Assert.AreEqual(2, marginalUse.Compounds.Count);
            Assert.AreEqual(1, marginalUse.PositiveFindingsCount);
            Assert.AreEqual(.5, marginalUse.OccurrenceFraction);
            Assert.AreEqual(2, marginalUse.AnalyticalScopeCount);
            Assert.AreEqual(false, marginalUse.AuthorisedUse);
            marginalUse = result[1];
            Assert.AreEqual(0, marginalUse.Compounds.Count);
            Assert.AreEqual(0, marginalUse.PositiveFindingsCount);
            Assert.AreEqual(.5, marginalUse.OccurrenceFraction);
            Assert.AreEqual(0, marginalUse.AnalyticalScopeCount);
            Assert.AreEqual(true, marginalUse.AuthorisedUse);

        }
        /// <summary>
        /// Test rescale
        /// </summary>
        [TestMethod]
        public void OccurrencePatternsFromFindingsCalculator_TestRescaleMultiple1() {
            var foodA = new Food("FoodA");
            var foods = new List<Food> { foodA };
            var collection = buildFoodSampleCompoundCollectionFromString(
                foodA,
                " 1*1|A,0,0,1,1;B,0,0,1,1;C,0,0,1,1;D,0,0,1,1|" +
                " 2*1|A,0,0,1,1;B,0,0,1,1;C,0,0,1,1;D,0,0,0,1|" +
                " 3*1|A,0,0,1,1;B,0,0,1,1;C,0,0,0,1;D,0,0,0,1|" +
                " 4*1|A,0,0,1,1;B,0,0,0,1;C,0,0,0,1;D,0,0,0,1|" +
                "10*1|A,0,0,0,1;B,0,0,0,1;C,0,0,0,1;D,0,0,0,1|"
             );
            var settings = new OccurrencePatternsFromFindingsCalculatorSettings(new AgriculturalUseSettings() {
                ScaleUpOccurencePatterns = false,
                RestrictOccurencePatternScalingToAuthorisedUses = false,

            });
            var calculator = new OccurrencePatternsFromFindingsCalculator(settings);

            var result = calculator.Compute(foods, collection).ToArray();

            Assert.AreEqual(5, result.Length);

            Assert.AreEqual(4, result[0].Compounds.Count);
            Assert.AreEqual(1, result[0].PositiveFindingsCount);
            Assert.AreEqual(0.05, result[0].OccurrenceFraction);
            Assert.AreEqual(true, result[0].AuthorisedUse);
            Assert.AreEqual(3, result[1].Compounds.Count);
            Assert.AreEqual(2, result[1].PositiveFindingsCount);
            Assert.AreEqual(0.1, result[1].OccurrenceFraction);
            Assert.AreEqual(true, result[1].AuthorisedUse);
            Assert.AreEqual(2, result[2].Compounds.Count);
            Assert.AreEqual(3, result[2].PositiveFindingsCount);
            Assert.AreEqual(0.15, result[2].OccurrenceFraction);
            Assert.AreEqual(true, result[2].AuthorisedUse);
            Assert.AreEqual(1, result[3].Compounds.Count);
            Assert.AreEqual(4, result[3].PositiveFindingsCount);
            Assert.AreEqual(0.2, result[3].OccurrenceFraction);
            Assert.AreEqual(true, result[3].AuthorisedUse);
            Assert.AreEqual(0, result[4].Compounds.Count);
            Assert.AreEqual(0, result[4].PositiveFindingsCount);
            Assert.AreEqual(0.5, result[4].OccurrenceFraction);
            Assert.AreEqual(true, result[4].AuthorisedUse);
        }

        /// <summary>
        /// Test rescale
        /// </summary>
        [TestMethod]
        public void OccurrencePatternsFromFindingsCalculator_TestRescaleMultiple2() {
            var foodA = new Food("FoodA");
            var foods = new List<Food> { foodA };
            var collection = buildFoodSampleCompoundCollectionFromString(
                foodA,
                " 1*1|A,0,0,1,1;B,0,0,1,1;C,0,0,1,1;D,0,0,1,1|" +
                " 2*1|A,0,0,1,1;B,0,0,1,1;C,0,0,1,1;D,0,0,0,1|" +
                " 3*1|A,0,0,1,1;B,0,0,1,1;C,0,0,0,1;D,0,0,0,1|" +
                " 4*1|A,0,0,1,1;B,0,0,0,1;C,0,0,0,1;D,0,0,0,1|" +
                "10*1|A,0,0,0,1;B,0,0,0,1;C,0,0,0,1;D,0,0,0,1|"
            );
            var settings = new OccurrencePatternsFromFindingsCalculatorSettings(new AgriculturalUseSettings() {
                ScaleUpOccurencePatterns = true,
                RestrictOccurencePatternScalingToAuthorisedUses = false,

            });
            var calculator = new OccurrencePatternsFromFindingsCalculator(settings);

            var result = calculator.Compute(foods, collection).ToArray();

            Assert.AreEqual(4, result.Length);
            Assert.AreEqual(4, result[0].Compounds.Count);
            Assert.AreEqual(1, result[0].PositiveFindingsCount);
            Assert.AreEqual(0.1, result[0].OccurrenceFraction);
            Assert.AreEqual(true, result[0].AuthorisedUse);
            Assert.AreEqual(3, result[1].Compounds.Count);
            Assert.AreEqual(2, result[1].PositiveFindingsCount);
            Assert.AreEqual(0.2, result[1].OccurrenceFraction);
            Assert.AreEqual(true, result[1].AuthorisedUse);
            Assert.AreEqual(2, result[2].Compounds.Count);
            Assert.AreEqual(3, result[2].PositiveFindingsCount);
            Assert.AreEqual(0.3, result[2].OccurrenceFraction);
            Assert.AreEqual(true, result[2].AuthorisedUse);
            Assert.AreEqual(1, result[3].Compounds.Count);
            Assert.AreEqual(4, result[3].PositiveFindingsCount);
            Assert.AreEqual(0.4, result[3].OccurrenceFraction);
            Assert.AreEqual(true, result[3].AuthorisedUse);
        }

        /// <summary>
        /// Test rescale authorized use
        /// </summary>
        [TestMethod]
        public void OccurrencePatternsFromFindingsCalculator_TestRescaleAuthorized1() {
            var foodA = new Food("FoodA");
            var foods = new List<Food> { foodA };
            var collection = buildFoodSampleCompoundCollectionFromString(
                foodA,
                " 1*1|A,0,0,1,1;B,0,0,1,1;C,0,0,0,1;D,0,0,1,1|" +
                " 2*0|A,0,0,1,1;B,0,0,1,1;C,0,0,1,1;D,0,0,1,1|" +
                " 3*0|A,0,0,1,1;B,0,0,1,1;C,0,0,1,1;D,0,0,0,1|" +
                " 4*1|A,0,0,1,1;B,0,0,0,1;C,0,0,0,1;D,0,0,0,1|" +
                "10*1|A,0,0,0,1;B,0,0,0,1;C,0,0,0,1;D,0,0,0,1|"
            );
            var settings = new OccurrencePatternsFromFindingsCalculatorSettings(new AgriculturalUseSettings() {
                ScaleUpOccurencePatterns = false,
                RestrictOccurencePatternScalingToAuthorisedUses = false,

            });
            var calculator = new OccurrencePatternsFromFindingsCalculator(settings);
            var result = calculator.Compute(foods, collection).ToArray();

            Assert.AreEqual(5, result.Length);
            Assert.AreEqual(3, result[0].Compounds.Count);
            Assert.AreEqual(1, result[0].PositiveFindingsCount);
            Assert.AreEqual(0.05, result[0].OccurrenceFraction);
            Assert.AreEqual(true, result[0].AuthorisedUse);
            Assert.AreEqual(4, result[1].Compounds.Count);
            Assert.AreEqual(2, result[1].PositiveFindingsCount);
            Assert.AreEqual(0.1, result[1].OccurrenceFraction);
            Assert.AreEqual(false, result[1].AuthorisedUse);
            Assert.AreEqual(3, result[2].Compounds.Count);
            Assert.AreEqual(3, result[2].PositiveFindingsCount);
            Assert.AreEqual(0.15, result[2].OccurrenceFraction);
            Assert.AreEqual(false, result[2].AuthorisedUse);
            Assert.AreEqual(1, result[3].Compounds.Count);
            Assert.AreEqual(4, result[3].PositiveFindingsCount);
            Assert.AreEqual(0.2, result[3].OccurrenceFraction);
            Assert.AreEqual(true, result[3].AuthorisedUse);
            Assert.AreEqual(0, result[4].Compounds.Count);
            Assert.AreEqual(0, result[4].PositiveFindingsCount);
            Assert.AreEqual(0.5, result[4].OccurrenceFraction);
            Assert.AreEqual(true, result[4].AuthorisedUse);
        }

        /// <summary>
        /// Test rescale authorized use
        /// </summary>
        [TestMethod]
        public void OccurrencePatternsFromFindingsCalculator_TestRescaleAuthorized2() {
            var foodA = new Food("FoodA");
            var foods = new List<Food> { foodA };
            var collection = buildFoodSampleCompoundCollectionFromString(
                foodA,
                " 1*1|A,0,0,1,1;B,0,0,1,1;C,0,0,0,1;D,0,0,1,1|" +
                " 2*0|A,0,0,1,1;B,0,0,1,1;C,0,0,1,1;D,0,0,1,1|" +
                " 3*0|A,0,0,1,1;B,0,0,1,1;C,0,0,1,1;D,0,0,0,1|" +
                " 4*1|A,0,0,1,1;B,0,0,0,1;C,0,0,0,1;D,0,0,0,1|" +
                "10*1|A,0,0,0,1;B,0,0,0,1;C,0,0,0,1;D,0,0,0,1|"
            );
            var settings = new OccurrencePatternsFromFindingsCalculatorSettings(new AgriculturalUseSettings() {
                ScaleUpOccurencePatterns = true,
                RestrictOccurencePatternScalingToAuthorisedUses = false,

            });
            var calculator = new OccurrencePatternsFromFindingsCalculator(settings);

            var result = calculator.Compute(foods, collection).ToArray();

            Assert.AreEqual(4, result.Length);
            Assert.AreEqual(3, result[0].Compounds.Count);
            Assert.AreEqual(1, result[0].PositiveFindingsCount);
            Assert.AreEqual(0.1, result[0].OccurrenceFraction, 1E-10);
            Assert.AreEqual(true, result[0].AuthorisedUse);
            Assert.AreEqual(4, result[1].Compounds.Count);
            Assert.AreEqual(2, result[1].PositiveFindingsCount);
            Assert.AreEqual(0.2, result[1].OccurrenceFraction, 1E-10);
            Assert.AreEqual(false, result[1].AuthorisedUse);
            Assert.AreEqual(3, result[2].Compounds.Count);
            Assert.AreEqual(3, result[2].PositiveFindingsCount);
            Assert.AreEqual(0.3, result[2].OccurrenceFraction, 1E-10);
            Assert.AreEqual(false, result[2].AuthorisedUse);
            Assert.AreEqual(1, result[3].Compounds.Count);
            Assert.AreEqual(4, result[3].PositiveFindingsCount);
            Assert.AreEqual(0.4, result[3].OccurrenceFraction, 1E-10);
            Assert.AreEqual(true, result[3].AuthorisedUse);
        }

        /// <summary>
        /// Test rescale authorized use
        /// </summary>
        [TestMethod]
        public void OccurrencePatternsFromFindingsCalculator_RescaleAuthorizedTest3() {
            var foodA = new Food("FoodA");
            var foods = new List<Food> { foodA };
            var collection = buildFoodSampleCompoundCollectionFromString(
                foodA,
                " 1*1|A,0,0,1,1;B,0,0,1,1;C,0,0,0,1;D,0,0,1,1|" +
                " 2*0|A,0,0,1,1;B,0,0,1,1;C,0,0,1,1;D,0,0,1,1|" +
                " 3*0|A,0,0,1,1;B,0,0,1,1;C,0,0,1,1;D,0,0,0,1|" +
                " 4*1|A,0,0,1,1;B,0,0,0,1;C,0,0,0,1;D,0,0,0,1|" +
                "10*1|A,0,0,0,1;B,0,0,0,1;C,0,0,0,1;D,0,0,0,1|"
            );
            var settings = new OccurrencePatternsFromFindingsCalculatorSettings(new AgriculturalUseSettings() {
                ScaleUpOccurencePatterns = true,
                RestrictOccurencePatternScalingToAuthorisedUses = true,

            });
            var calculator = new OccurrencePatternsFromFindingsCalculator(settings);

            var result = calculator.Compute(foods, collection).ToArray();

            Assert.AreEqual(4, result.Length);
            Assert.AreEqual(3, result[0].Compounds.Count);
            Assert.AreEqual(1, result[0].PositiveFindingsCount);
            Assert.AreEqual(0.15, result[0].OccurrenceFraction, 1E-10);
            Assert.AreEqual(true, result[0].AuthorisedUse);
            Assert.AreEqual(4, result[1].Compounds.Count);
            Assert.AreEqual(2, result[1].PositiveFindingsCount);
            Assert.AreEqual(0.1, result[1].OccurrenceFraction, 1E-10);
            Assert.AreEqual(false, result[1].AuthorisedUse);
            Assert.AreEqual(3, result[2].Compounds.Count);
            Assert.AreEqual(3, result[2].PositiveFindingsCount);
            Assert.AreEqual(0.15, result[2].OccurrenceFraction, 1E-10);
            Assert.AreEqual(false, result[2].AuthorisedUse);
            Assert.AreEqual(1, result[3].Compounds.Count);
            Assert.AreEqual(4, result[3].PositiveFindingsCount);
            Assert.AreEqual(0.6, result[3].OccurrenceFraction, 1E-10);
            Assert.AreEqual(true, result[3].AuthorisedUse);
        }

        /// <summary>
        /// Test compute occurrence patterns from findings. Should fail when scaling
        /// up occurrence patterns of a sample compound collection containing patterns
        /// with both authorised and unauthorised uses.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(AggregateException), AllowDerivedTypes = false)]
        public void OccurrencePatternsFromFindingsCalculator_TestFailRescaleAuthorized() {
            var foodA = new Food("FoodA");
            var foods = new List<Food> { foodA };
            var collection = buildFoodSampleCompoundCollectionFromString(
                foodA,
                "1*1|A,0,0,0,1;B,0,0,1,1;C,0,0,1,1|" +
                "1*0|A,0,0,0,1;B,0,0,1,1;C,0,0,1,1|"
             );
            var settings = new OccurrencePatternsFromFindingsCalculatorSettings(new AgriculturalUseSettings() {
                ScaleUpOccurencePatterns = true,
                RestrictOccurencePatternScalingToAuthorisedUses = true,

            });
            var calculator = new OccurrencePatternsFromFindingsCalculator(settings);
            calculator.Compute(foods, collection).ToList();
        }

        /// <summary>
        /// Test rescale with missing value
        /// </summary>
        [TestMethod]
        public void OccurrencePatternsFromFindingsCalculator_TestRescaleWithMissing1() {
            var foodA = new Food("FoodA");
            var foods = new List<Food> { foodA };
            var collection = buildFoodSampleCompoundCollectionFromString(
                foodA,
                "10*1|A,0,0,1,1;B,0,0,1,1;C,1,0,0,1;D,1,0,0,1|" +
                "10*0|A,1,0,0,1;B,1,0,0,1;C,0,0,1,1;D,0,0,1,1|" +
                "2*1|A,0,0,0,1;B,0,0,0,1;C,0,0,0,1;D,0,0,0,1|"
            );
            var settings = new OccurrencePatternsFromFindingsCalculatorSettings(new AgriculturalUseSettings() {
                ScaleUpOccurencePatterns = false,
                RestrictOccurencePatternScalingToAuthorisedUses = false,

            });
            var calculator = new OccurrencePatternsFromFindingsCalculator(settings);

            var result = calculator.Compute(foods, collection).ToArray();

            Assert.AreEqual(2, result.Length);

            Assert.AreEqual(2, result[0].Compounds.Count);
            Assert.AreEqual(10, result[0].PositiveFindingsCount);
            Assert.AreEqual(0.5, result[0].OccurrenceFraction, 1E-10);
            Assert.AreEqual(true, result[0].AuthorisedUse);
            Assert.AreEqual(2, result[1].Compounds.Count);
            Assert.AreEqual(10, result[1].PositiveFindingsCount);
            Assert.AreEqual(0.5, result[1].OccurrenceFraction, 1E-10);
            Assert.AreEqual(false, result[1].AuthorisedUse);
        }

        /// <summary>
        /// Test rescale with missing value
        /// </summary>
        [TestMethod]
        public void OccurrencePatternsFromFindingsCalculator_TestRescaleWithMissing2() {
            var foodA = new Food("FoodA");
            var foods = new List<Food> { foodA };
            var collection = buildFoodSampleCompoundCollectionFromString(
                foodA,
                "10*1|A,0,0,1,1;B,0,0,1,1;C,1,0,0,1;D,1,0,0,1|" +
                "10*0|A,1,0,0,1;B,1,0,0,1;C,0,0,1,1;D,0,0,1,1|" +
                "2*1|A,0,0,0,1;B,0,0,0,1;C,0,0,0,1;D,0,0,0,1|"
            );
            var settings = new OccurrencePatternsFromFindingsCalculatorSettings(new AgriculturalUseSettings() {
                ScaleUpOccurencePatterns = true,
                RestrictOccurencePatternScalingToAuthorisedUses = true,

            });
            var calculator = new OccurrencePatternsFromFindingsCalculator(settings);
            var result = calculator.Compute(foods, collection).ToArray();

            Assert.AreEqual(2, result.Length);
            Assert.AreEqual(2, result[0].Compounds.Count);
            Assert.AreEqual(10, result[0].PositiveFindingsCount);
            Assert.AreEqual(0.5, result[0].OccurrenceFraction, 1E-10);
            Assert.AreEqual(true, result[0].AuthorisedUse);
            Assert.AreEqual(2, result[1].Compounds.Count);
            Assert.AreEqual(10, result[1].PositiveFindingsCount);
            Assert.AreEqual(0.5, result[1].OccurrenceFraction, 1E-10);
            Assert.AreEqual(false, result[1].AuthorisedUse);
        }

        /// <summary>
        /// Build sampleCompoundCollection out of formatted string
        /// Record: NumberOfrecords*IsAuthorized|SubstCode,Missing,NonDetect,Residue,LOR;...|...
        /// </summary>
        /// <param name="food">Food to create the test collection for</param>
        /// <param name="data">String with concise notation of collection contents</param>
        /// <returns>List of SampleCompoundCollection with the data</returns>
        private IDictionary<Food, SampleCompoundCollection> buildFoodSampleCompoundCollectionFromString(Food food, string data) {
            var blocks = data.Split('|');
            var scRecords = new List<SampleCompoundRecord>(blocks.Length);
            var compounds = new Dictionary<string, Compound>();

            for (int i = 0; i < blocks.Length / 2; i++) {
                var recData = blocks[2 * i].Split('*');
                var num = int.Parse(recData[0].Trim());
                var authorized = int.Parse(recData[1].Trim()) != 0;

                var rec = new SampleCompoundRecord { AuthorisedUse = authorized };
                rec.SampleCompounds = new Dictionary<Compound, SampleCompound>();

                var sccs = blocks[2 * i + 1].Split(';');
                for (int j = 0; j < sccs.Length; j++) {
                    var values = sccs[j].Split(',');
                    var substCode = values[0].Trim();
                    if (!compounds.TryGetValue(substCode, out var compound)) {
                        compound = new Compound(substCode);
                        compounds.Add(substCode, compound);
                    }
                    var resType = int.Parse(values[2]) != 0 ? ResType.LOQ : (int.Parse(values[1]) != 0 ? ResType.MV : ResType.VAL);
                    rec.SampleCompounds.Add(compound, new SampleCompound(
                        compound: compound,
                        resType: resType,
                        residue: double.Parse(values[3]),
                        lod: double.Parse(values[4]),
                        loq: double.Parse(values[4])
                    ));
                }
                //add num of times to collection
                //we don't care whether the instances are the same
                //object which are added multiple times
                for (int n = 0; n < num; n++) {
                    scRecords.Add(rec);
                }
            }

            var result = new List<SampleCompoundCollection> { new SampleCompoundCollection(food, scRecords) };
            return result.ToDictionary(r => r.Food);
        }
    }
}
