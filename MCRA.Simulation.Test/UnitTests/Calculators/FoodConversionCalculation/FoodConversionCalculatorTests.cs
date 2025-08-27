using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Calculators.FoodConversionCalculation;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;

namespace MCRA.Simulation.Test.UnitTests.Calculators.FoodConversionCalculation {

    /// <summary>
    /// Food conversion calculator tests.
    /// </summary>
    [TestClass]
    public class FoodConversionCalculatorTests {

        /// <summary>
        /// Test empty lists
        /// </summary>
        [TestMethod]
        public void FoodConversionCalculator_TestNoData() {
            var foods = new List<Food>();
            var substances = new List<Compound>();
            var settings = new FoodConversionsModuleConfig() {
                UseComposition = true,
                UseDefaultProcessingFactor = true,
                FoodIncludeNonDetects = true,
                SubstanceIncludeNonDetects = true
            };
            var calculator = new FoodConversionCalculator(settings, null, null, null);
            Assert.ThrowsExactly<Exception>(() => calculator.CalculateFoodConversions(foods, substances));
        }

        /// <summary>
        /// Calculate conversions using composition link
        /// </summary>
        [TestMethod]
        public void FoodConversionCalculator_TestTranslation() {
            var foodsAsEaten = mockFoods("Fae");
            var foodsAsMeasured = mockFoods("Fam");
            var allFoods = foodsAsEaten.Union(foodsAsMeasured).ToDictionary(r => r.Code);
            var substances = FakeSubstancesGenerator.Create(1);
            var translations = foodsAsEaten
                .SelectMany(r => foodsAsMeasured, (fae, fam) => new FoodTranslation(fae, fam, 0.5))
                .ToLookup(r => r.FoodFrom);
            var samplesPerFoodCompound = FakeModelledFoodsInfosGenerator.Create(foodsAsMeasured, substances);
            var settings = new FoodConversionsModuleConfig() {
                UseComposition = true,
                UseDefaultProcessingFactor = true,
                FoodIncludeNonDetects = true,
                SubstanceIncludeNonDetects = true
            };
            var calculator = new FoodConversionCalculator(
                settings,
                allFoods,
                samplesPerFoodCompound,
                foodsAsMeasured,
                foodCompositions: translations
            );
            var result = calculator.CalculateFoodConversions(foodsAsEaten, substances);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(FoodConversionStepType.CompositionExact, result.First().ConversionStepResults.First().Step);
        }

        /// <summary>
        /// Calculate conversions using food facets processing link. The first facet determines the processing factors
        /// </summary>
        [TestMethod]
        [DataRow("AR4WS", "F20.AO7QF$F28.BO7QG")]
        [DataRow("AR4WS", "F20.AO7QF")]
        public void FoodConversionCalculator_TestProcessingTranslationWithFoodEx2CompositeFacet(
            string foodBaseCode,
            string facet
        ) {
            var foodsAsEaten = mockFoods($"{foodBaseCode}#{facet}");
            var foodsAsMeasured = mockFoods(foodBaseCode);
            var allFoods = foodsAsEaten.Union(foodsAsMeasured).ToDictionary(r => r.Code);
            var translations = foodsAsEaten
                .SelectMany(r => foodsAsMeasured, (fae, fam) => new FoodTranslation(fae, fam, 0.5))
                .ToLookup(r => r.FoodFrom);
            var processingTypes = mockProcessingTypes(facet);
            var substances = FakeSubstancesGenerator.Create(1);
            var samplesPerFoodCompound = FakeModelledFoodsInfosGenerator.Create(foodsAsMeasured, substances);
            var settings = new FoodConversionsModuleConfig() {
                UseComposition = true,
                UseDefaultProcessingFactor = true
            };
            var calculator = new FoodConversionCalculator(
                settings,
                allFoods,
                samplesPerFoodCompound,
                foodsAsMeasured,
                foodCompositions: translations,
                processingTypes: processingTypes
            );
            var result = calculator.CalculateFoodConversions(foodsAsEaten, substances);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(FoodConversionStepType.ProcessingTranslation, result.First().ConversionStepResults.First().Step);
            CollectionAssert.AreEquivalent(processingTypes.ToList(), result.First().ProcessingTypes.ToList());
        }

        /// <summary>
        /// Calculate conversions using default processing link
        /// </summary>
        [TestMethod]
        public void FoodConversionCalculator_TestDefaultProcessingLink() {
            var foodsAsEaten = mockFoods("Apple-peeled");
            var foodsAsMeasured = mockFoods("Apple");
            var allFoods = foodsAsEaten.Union(foodsAsMeasured).ToDictionary(r => r.Code);
            var substances = FakeSubstancesGenerator.Create(1);
            var samplesPerFoodCompound = FakeModelledFoodsInfosGenerator.Create(foodsAsMeasured, substances);
            var settings = new FoodConversionsModuleConfig() {
                UseComposition = true,
                UseDefaultProcessingFactor = true,
                FoodIncludeNonDetects = true,
                SubstanceIncludeNonDetects = true
            };
            var calculator = new FoodConversionCalculator(settings, allFoods, samplesPerFoodCompound, foodsAsMeasured);
            var result = calculator.CalculateFoodConversions(foodsAsEaten, substances);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(FoodConversionStepType.DefaultProcessing, result.First().ConversionStepResults.First().Step);
        }

        /// <summary>
        /// Calculate conversions using sub type link
        /// </summary>
        [TestMethod]
        public void FoodConversionCalculator_TestSubTypeLink() {
            var foodsAsEaten = mockFoods("Fruit");
            var foodsAsMeasured = mockFoods("Fruit$Apple", "Fruit$Orange");
            var allFoods = foodsAsEaten.Union(foodsAsMeasured).ToDictionary(r => r.Code);
            var substances = FakeSubstancesGenerator.Create(1);

            var samplesPerFoodCompound = FakeModelledFoodsInfosGenerator.Create(foodsAsMeasured, substances);

            var foodCompoundProcessingFactors = new Dictionary<(Food, Compound), ProcessingFactor>();

            var marketShares = foodsAsMeasured
                .Select(r => new MarketShare() {
                    Food = r,
                    Percentage = 50D
                })
                .ToList();
            var settings = new FoodConversionsModuleConfig() {
                UseComposition = true,
                UseDefaultProcessingFactor = true,
                FoodIncludeNonDetects = true,
                SubstanceIncludeNonDetects = true,
                UseSubTypes = true,
                UseMarketShares = true
            };
            var calculator = new FoodConversionCalculator(
                settings,
                allFoods,
                samplesPerFoodCompound,
                foodsAsMeasured,
                marketShares: marketShares
            );
            var result = calculator.CalculateFoodConversions(foodsAsEaten, substances);
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(FoodConversionStepType.Subtype, result.First().ConversionStepResults.First().Step);
            Assert.AreEqual(0.5, result.First().MarketShare);
        }

        /// <summary>
        /// Calculate conversions using super type link.
        /// </summary>
        [TestMethod]
        public void FoodConversionCalculator_TestSuperTypeLink() {
            var foodsAsMeasured = mockFoods("Fruit");
            var foodsAsEaten = mockFoods("Fruit$Apple");
            var allFoods = foodsAsEaten.Union(foodsAsMeasured).ToDictionary(r => r.Code);
            var substances = FakeSubstancesGenerator.Create(1);
            var samplesPerFoodCompound = FakeModelledFoodsInfosGenerator.Create(foodsAsMeasured, substances);
            var foodCompoundProcessingFactors = new Dictionary<(Food, Compound), ProcessingFactor>();
            var settings = new FoodConversionsModuleConfig() {
                UseComposition = true,
                UseDefaultProcessingFactor = true,
                FoodIncludeNonDetects = true,
                SubstanceIncludeNonDetects = true,
                UseSuperTypes = true
            };
            var calculator = new FoodConversionCalculator(
                settings,
                allFoods,
                samplesPerFoodCompound,
                foodsAsMeasured
            );
            var result = calculator.CalculateFoodConversions(foodsAsEaten, substances);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(FoodConversionStepType.Supertype, result.First().ConversionStepResults.First().Step);
        }

        /// <summary>
        /// Calculate conversions using tds composition link
        /// </summary>
        [TestMethod]
        public void FoodConversionCalculator_TestTdsComposition() {
            var foodsAsEaten = mockFoods("Apple");
            var foodsAsMeasured = mockFoods("FruitMix");
            var allFoods = foodsAsEaten.Union(foodsAsMeasured).ToDictionary(r => r.Code);
            var substances = FakeSubstancesGenerator.Create(1);
            var tdsCompositions = foodsAsEaten
                .SelectMany(r => foodsAsMeasured, (fae, fam) => new TDSFoodSampleComposition() {
                    Food = fae,
                    TDSFood = fam,
                })
                .ToList();
            var samplesPerFoodCompound = FakeModelledFoodsInfosGenerator.Create(foodsAsMeasured, substances);
            var settings = new FoodConversionsModuleConfig() {
                TotalDietStudy = true,
                UseComposition = true,
                UseDefaultProcessingFactor = true,
                FoodIncludeNonDetects = true,
                SubstanceIncludeNonDetects = true,
            };
            var calculator = new FoodConversionCalculator(
                settings,
                allFoods,
                samplesPerFoodCompound,
                foodsAsMeasured,
                tdsFoodSampleCompositionDictionary: tdsCompositions.ToDictionary(r => r.Food, r => r.TDSFood)
            );
            var result = calculator.CalculateFoodConversions(foodsAsEaten, substances);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(FoodConversionStepType.TDSCompositionExact, result.First().ConversionStepResults.First().Step);
        }

        /// <summary>
        /// Calculate conversions using read across link
        /// </summary>
        [TestMethod]
        public void FoodConversionCalculator_TestReadAcross() {
            var foodsAsEaten = mockFoods("Apple");
            var foodsAsMeasured = mockFoods("Pear");
            var allFoods = foodsAsEaten.Union(foodsAsMeasured).ToDictionary(r => r.Code);
            var substances = FakeSubstancesGenerator.Create(1);
            var readAcrossFoodTranslations = foodsAsEaten
                .SelectMany(r => foodsAsMeasured, (fae, fam) => new {
                    FoodTo = fae,
                    FoodFrom = fam,
                })
                .GroupBy(r => r.FoodTo)
                .ToDictionary(g => g.Key, g => g.Select(r => r.FoodFrom).ToList() as ICollection<Food>);
            var samplesPerFoodCompound = FakeModelledFoodsInfosGenerator.Create(foodsAsMeasured, substances);
            var settings = new FoodConversionsModuleConfig() {
                UseComposition = true,
                UseDefaultProcessingFactor = true,
                FoodIncludeNonDetects = true,
                SubstanceIncludeNonDetects = true,
                UseReadAcrossFoodTranslations = true
            };
            var calculator = new FoodConversionCalculator(
                settings,
                allFoods,
                samplesPerFoodCompound,
                foodsAsMeasured,
                readAcrossFoodTranslations
            );
            var result = calculator.CalculateFoodConversions(foodsAsEaten, substances);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(FoodConversionStepType.ReadAcross, result.First().ConversionStepResults.First().Step);
        }

        /// <summary>
        /// Calculate conversions using food facets processing link. The first facet determines the processing factors
        /// </summary>
        [TestMethod]
        public void FoodConversionCalculator_TestRemoveFacets() {
            var f1 = "F20.AO7QF";
            var f2 = "F28.BO7QG";
            var f3 = "F29.CO7QG";
            var facet = $"{f2}${f3}";
            var facets = new List<string> { f1, f2, f3, facet };
            var foodsAsEaten = mockFoods($"AR4WS#{facet}");
            var foodsAsMeasured = foodsAsEaten.Select(c => new Food() { Code = c.Code.Split('#').First(), Name = c.Code.Split('#').First() }).ToList();
            var allFoods = foodsAsEaten.Union(foodsAsMeasured).ToDictionary(r => r.Code);
            var substances = FakeSubstancesGenerator.Create(1);
            var samplesPerFoodCompound = FakeModelledFoodsInfosGenerator.Create(foodsAsMeasured, substances);
            var settings = new FoodConversionsModuleConfig() {
                UseComposition = true,
                UseDefaultProcessingFactor = true,
                FoodIncludeNonDetects = true,
                SubstanceIncludeNonDetects = true,
            };

            var calculator = new FoodConversionCalculator(
                settings,
                allFoods,
                samplesPerFoodCompound,
                foodsAsMeasured
            );
            var result = calculator.CalculateFoodConversions(foodsAsEaten, substances);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(FoodConversionStepType.RemoveFacets, result.First().ConversionStepResults.First().Step);
        }

        /// <summary>
        /// Calculate conversions using food facets processing link. The first facet determines the processing factors
        /// </summary>
        [TestMethod]
        public void FoodConversionCalculator_TestCompositeProcessingFacetLink() {
            var f1 = "F20.AO7QF";
            var f2 = "F28.BO7QG";
            var facet = $"{f1}${f2}";
            var processingType = new ProcessingType(facet);
            var processingTypes = new List<ProcessingType>() { processingType };
            var foodsAsEaten = mockFoods($"AR4WS#{facet}");
            var foodsAsMeasured = mockFoods($"AR4WS");
            var allFoods = foodsAsEaten.Union(foodsAsMeasured).ToDictionary(r => r.Code);
            var substances = FakeSubstancesGenerator.Create(1);
            var samplesPerFoodCompound = FakeModelledFoodsInfosGenerator.Create(foodsAsMeasured, substances);
            var settings = new FoodConversionsModuleConfig() {
                UseDefaultProcessingFactor = true,
                FoodIncludeNonDetects = true,
                SubstanceIncludeNonDetects = true,
            };
            var calculator = new FoodConversionCalculator(
                settings,
                allFoods,
                samplesPerFoodCompound,
                foodsAsMeasured,
                processingTypes: processingTypes
            );
            var result = calculator.CalculateFoodConversions(foodsAsEaten, substances);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(FoodConversionStepType.CompositeFacetProcessing, result.First().ConversionStepResults.First().Step);
            CollectionAssert.AreEquivalent(processingTypes, result.First().ProcessingTypes.ToList());
        }

        private ICollection<Food> mockFoods(params string[] codes) {
            return codes.Select(r => new Food(r)).ToList();
        }

        private ICollection<ProcessingType> mockProcessingTypes(params string[] codes) {
            return codes.Select(r => new ProcessingType(r)).ToList();
        }
    }
}
