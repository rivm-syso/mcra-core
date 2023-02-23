using MCRA.Data.Compiled.Objects;
using MCRA.General;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Data.Management.Test.UnitTests.DataManagement {
    /// <summary>
    /// Note: this is not marked as TestClass, the subclasses define the method to use
    /// to retrieve the foods (_getFoodsDelegate)
    /// These tests are run multiple times (the subclasses) because of the redundancy in food retrieval
    /// in the CompiledDataManager and SubsetManager: this needs refactoring...
    /// </summary>
    public class CompiledFoodTests : CompiledTestsBase {

        protected Func<IDictionary<string, Food>> _getFoodsDelegate;

        [TestMethod]
        public virtual void CompiledFoods_TestGetAllFoods() {
            _rawDataProvider.SetDataTables(
                (ScopingType.Foods, @"FoodsTests\FoodsSimple")
            );

            var foods = _getFoodsDelegate.Invoke();

            Assert.AreEqual(3, foods.Count);

            Food f;
            Assert.IsTrue(foods.TryGetValue("A", out f) && f.Name.Equals("Apple"));
            Assert.IsTrue(foods.TryGetValue("B", out f) && f.Name.Equals("Banana"));
            Assert.IsTrue(foods.TryGetValue("C", out f) && f.Name.Equals("Cucumber"));
        }

        [TestMethod]
        public virtual void CompiledFoods_TestGetAllFoodsFiltered() {
            _rawDataProvider.SetDataTables(
                (ScopingType.Foods, @"FoodsTests\FoodsSimple")
            );
            _rawDataProvider.SetFilterCodes(ScopingType.Foods, new[] { "A", "C" });

            var foods = _getFoodsDelegate.Invoke();
            Food f;
            Assert.AreEqual(2, foods.Count);

            Assert.IsTrue(foods.TryGetValue("A", out f) && f.Name.Equals("Apple"));
            Assert.IsTrue(foods.TryGetValue("C", out f) && f.Name.Equals("Cucumber"));
        }

        [TestMethod]
        public virtual void CompiledFoods_TestGetAllFoodHierarchiesMatched() {
            _rawDataProvider.SetDataTables(
                (ScopingType.Foods, @"FoodTranslationTests\FoodTranslationFoods"),
                (ScopingType.FoodHierarchies, @"FoodsTests\FoodHierarchies")
            );

            var foods = _getFoodsDelegate.Invoke();

            Assert.AreEqual(4, foods.Count);

            Assert.IsTrue(foods.TryGetValue("A", out Food apple));
            Assert.IsTrue(foods.TryGetValue("F", out Food flour));
            Assert.IsTrue(foods.TryGetValue("W", out Food wheat));
            Assert.IsTrue(foods.TryGetValue("AP", out Food pie));

            Assert.AreEqual(0, apple.Children.Count);
            Assert.AreEqual(1, flour.Children.Count);
            Assert.AreEqual(2, pie.Children.Count);
            Assert.AreEqual(0, wheat.Children.Count);
            Assert.AreEqual(flour, wheat.Parent);
            Assert.AreEqual(pie, flour.Parent);
            Assert.AreEqual(pie, apple.Parent);
            Assert.IsNull(pie.Parent);
        }

        [TestMethod]
        public virtual void CompiledFoods_TestGetAllFoodHierarchiesMatchedFilter() {
            _rawDataProvider.SetDataTables(
                (ScopingType.Foods, @"FoodTranslationTests\FoodTranslationFoods"),
                (ScopingType.FoodHierarchies, @"FoodsTests\FoodHierarchies")
            );
            _rawDataProvider.SetFilterCodes(ScopingType.Foods, new[] { "AP", "F" });
            var foods = _getFoodsDelegate.Invoke();

            Assert.AreEqual(2, foods.Count);

            Assert.IsTrue(foods.TryGetValue("F", out Food flour));
            Assert.IsTrue(foods.TryGetValue("AP", out Food pie));

            Assert.AreEqual(0, flour.Children.Count);
            Assert.AreEqual(1, pie.Children.Count);
            Assert.AreEqual(pie, flour.Parent);
            Assert.IsNull(pie.Parent);
        }

        [TestMethod]
        public virtual void CompiledFoods_TestGetAllFoodHierarchiesUnmatched() {
            _rawDataProvider.SetDataTables(
                (ScopingType.FoodHierarchies, @"FoodsTests\FoodHierarchies")
            );

            var foods = _getFoodsDelegate.Invoke();
            Assert.AreEqual(4, foods.Count);
        }

        [TestMethod]
        public virtual void CompiledFoods_TestGetAllFoodConsumptionQuantificationsMatched() {
            _rawDataProvider.SetDataTables(
                (ScopingType.Foods, @"FoodTranslationTests\FoodTranslationFoods"),
                (ScopingType.FoodConsumptionQuantifications, @"FoodsTests\FoodConsumptionQuantifications")
            );

            var foods = _getFoodsDelegate.Invoke();

            Assert.AreEqual(4, foods.Count);
            Assert.IsTrue(foods.TryGetValue("A", out Food apple) && apple.Name.Equals("Apple"));
            Assert.IsTrue(foods.TryGetValue("F", out Food flour) && flour.Name.Equals("Flour"));
            Assert.IsTrue(foods.TryGetValue("W", out Food wheat) && wheat.Name.Equals("Wheat"));
            Assert.IsTrue(foods.TryGetValue("AP", out Food pie) && pie.Name.Equals("Apple Pie"));

            Assert.AreEqual(2, apple.FoodConsumptionQuantifications.Count);
            Assert.AreEqual(73.4, apple.FoodConsumptionQuantifications["piece"].UnitWeight);
            Assert.AreEqual(14.3, apple.FoodConsumptionQuantifications["piece"].UnitWeightUncertainty);
            Assert.AreEqual(10, apple.FoodConsumptionQuantifications["piece"].AmountUncertainty);
            Assert.AreEqual(24, apple.FoodConsumptionQuantifications["slice"].UnitWeight);
            Assert.AreEqual(10.5, apple.FoodConsumptionQuantifications["slice"].UnitWeightUncertainty);
            Assert.AreEqual(5, apple.FoodConsumptionQuantifications["slice"].AmountUncertainty);

            Assert.AreEqual(0, wheat.FoodConsumptionQuantifications.Count);

            Assert.AreEqual(1, flour.FoodConsumptionQuantifications.Count);
            Assert.AreEqual(32.39, flour.FoodConsumptionQuantifications["cup"].UnitWeight);
            Assert.AreEqual(8.3, flour.FoodConsumptionQuantifications["cup"].UnitWeightUncertainty);
            Assert.AreEqual(12.12, flour.FoodConsumptionQuantifications["cup"].AmountUncertainty);

            Assert.AreEqual(2, pie.FoodConsumptionQuantifications.Count);
            Assert.AreEqual(400, pie.FoodConsumptionQuantifications["piece"].UnitWeight);
            Assert.AreEqual(25, pie.FoodConsumptionQuantifications["piece"].UnitWeightUncertainty);
            Assert.AreEqual(0, pie.FoodConsumptionQuantifications["piece"].AmountUncertainty);
            Assert.AreEqual(121, pie.FoodConsumptionQuantifications["slice"].UnitWeight);
            Assert.AreEqual(11, pie.FoodConsumptionQuantifications["slice"].UnitWeightUncertainty);
            Assert.AreEqual(11, pie.FoodConsumptionQuantifications["slice"].AmountUncertainty);

        }

        [TestMethod]
        public virtual void CompiledFoods_TestGetAllFoodConsumptionQuantificationsUnmatched() {
            _rawDataProvider.SetDataTables(
                (ScopingType.FoodConsumptionQuantifications, @"FoodsTests\FoodConsumptionQuantifications")
            );
            var foods = _getFoodsDelegate.Invoke();

            Assert.AreEqual(3, foods.Count);
            Food f;
            Assert.IsTrue(foods.TryGetValue("A", out f) && f.Name == "A");
            Assert.IsTrue(foods.TryGetValue("F", out f) && f.Name == "F");
            Assert.IsTrue(foods.TryGetValue("AP", out f) && f.Name == "AP");
        }

        [TestMethod]
        public virtual void CompiledFoods_TestGetAllFoodPropertiesMatched() {
            _rawDataProvider.SetDataTables(
                (ScopingType.Foods, @"FoodsTests\FoodsSimple"),
                (ScopingType.FoodProperties, @"FoodsTests\FoodsSimpleProperties")
            );
            var foods = _getFoodsDelegate.Invoke();

            Assert.AreEqual(3, foods.Count);

            Assert.IsTrue(foods.TryGetValue("A", out Food apple));
            Assert.IsTrue(foods.TryGetValue("B", out Food banana));
            Assert.IsTrue(foods.TryGetValue("C", out Food cucumber));

            Assert.AreEqual(40, apple.GetDefaultUnitWeight(UnitWeightValueType.UnitWeightRac)?.Value);
            Assert.IsNull(apple.MarketShare);

            Assert.AreEqual(60, banana.GetDefaultUnitWeight(UnitWeightValueType.UnitWeightRac)?.Value);
            Assert.IsNull(banana.MarketShare);

            Assert.IsNull(cucumber.Properties);
            Assert.IsNull(cucumber.GetDefaultUnitWeight(UnitWeightValueType.UnitWeightRac));
        }

        [TestMethod]
        public virtual void CompiledFoods_TestGetAllFoodPropertiesUnmatched() {
            _rawDataProvider.SetDataTables(
                (ScopingType.FoodProperties, @"FoodsTests\FoodsSimpleProperties")
            );
            var foods = _getFoodsDelegate.Invoke();

            Assert.AreEqual(3, foods.Count);

            Food f;
            Assert.IsTrue(foods.TryGetValue("A", out f) && f.Name == "A");
            Assert.IsTrue(foods.TryGetValue("B", out f) && f.Name == "B");
            Assert.IsTrue(foods.TryGetValue("C", out f) && f.Name == "C");
        }

        [TestMethod]
        public virtual void CompiledFoods_TestGetAllFoodsFromConsumptions() {
            _rawDataProvider.SetDataTables(
                (ScopingType.FoodSurveys, @"FoodsTests\FoodSurveysSimple"),
                (ScopingType.Consumptions, @"FoodsTests\FoodConsumptionsSimple"),
                (ScopingType.DietaryIndividuals, @"FoodsTests\IndividualsSimple")
            );
            var consumptions = _compiledDataManager.GetAllFoodConsumptions();
            var foods = _getFoodsDelegate.Invoke();

            Assert.AreEqual(3, foods.Count);
            Food f;
            Assert.IsTrue(foods.TryGetValue("A", out f) && f.Name.Equals("A"));
            Assert.IsTrue(foods.TryGetValue("B", out f) && f.Name.Equals("B"));
            Assert.IsTrue(foods.TryGetValue("C", out f) && f.Name.Equals("C"));
        }

        [TestMethod]
        public virtual void CompiledFoods_TestGetAllFoodsFromConsumptionsFiltered() {
            _rawDataProvider.SetDataTables(
                (ScopingType.FoodSurveys, @"FoodsTests\FoodSurveysSimple"),
                (ScopingType.Consumptions, @"FoodsTests\FoodConsumptionsSimple"),
                (ScopingType.DietaryIndividuals, @"FoodsTests\IndividualsSimple")
            );
            _rawDataProvider.SetFilterCodes(ScopingType.Foods, new[] { "B" });

            _compiledDataManager.GetAllFoodConsumptions();
            var foods = _getFoodsDelegate.Invoke();

            Assert.AreEqual(1, foods.Count);
            Food f;
            Assert.IsTrue(foods.TryGetValue("B", out f) && f.Name.Equals("B"));
        }

        [TestMethod]
        public virtual void CompiledFoods_TestGetAllFoodsFromProcessingFactors() {
            _rawDataProvider.SetDataTables(
                (ScopingType.ProcessingFactors, @"FoodsTests\ProcessingFactorsSimple")
            );

            _compiledDataManager.GetAllProcessingFactors();
            var foods = _getFoodsDelegate.Invoke();

            Assert.AreEqual(3, foods.Count);
            Food f;
            Assert.IsTrue(foods.TryGetValue("A", out f) && f.Name.Equals("A"));
            Assert.IsTrue(foods.TryGetValue("B", out f) && f.Name.Equals("B"));
            Assert.IsTrue(foods.TryGetValue("C", out f) && f.Name.Equals("C"));
        }

        [TestMethod]
        public virtual void CompiledFoods_TestGetAllFoodsFromProcessingFactorsFiltered() {
            _rawDataProvider.SetDataTables(
                (ScopingType.ProcessingFactors, @"FoodsTests\ProcessingFactorsSimple")
            );
            _rawDataProvider.SetFilterCodes(ScopingType.Foods, new[] { "A", "B" });

            _compiledDataManager.GetAllProcessingFactors();
            var foods = _getFoodsDelegate.Invoke();

            Assert.AreEqual(2, foods.Count);
            Food f;
            Assert.IsTrue(foods.TryGetValue("A", out f) && f.Name.Equals("A"));
            Assert.IsTrue(foods.TryGetValue("B", out f) && f.Name.Equals("B"));
        }

        [TestMethod]
        public virtual void CompiledFoods_TestGetAllFoodsWithSameFoodsFromConsumptionsAndProcessingFactors() {
            _rawDataProvider.SetDataTables(
                (ScopingType.FoodSurveys, @"FoodsTests\FoodSurveysSimple"),
                (ScopingType.DietaryIndividuals, @"FoodsTests\IndividualsSimple"),
                (ScopingType.Consumptions, @"FoodsTests\FoodConsumptionsSimple"),
                (ScopingType.ProcessingFactors, @"FoodsTests\ProcessingFactorsSimple")
            );

            _compiledDataManager.GetAllFoodConsumptions();
            _compiledDataManager.GetAllProcessingFactors();
            var foods = _getFoodsDelegate.Invoke();

            Assert.AreEqual(3, foods.Count);
            Food f;
            Assert.IsTrue(foods.TryGetValue("A", out f) && f.Name.Equals("A"));
            Assert.IsTrue(foods.TryGetValue("B", out f) && f.Name.Equals("B"));
            Assert.IsTrue(foods.TryGetValue("C", out f) && f.Name.Equals("C"));
        }

        [TestMethod]
        public virtual void CompiledFoods_TestGetAllFoodEx2FoodsAndFacets() {
            _rawDataProvider.SetDataTables(
                (ScopingType.FacetDescriptors, @"FoodsTests\FoodEx2FacetDescriptors"),
                (ScopingType.Foods, @"FoodsTests\FoodEx2Foods"),
                (ScopingType.Facets, @"FoodsTests\FoodEx2Facets")
            );

            var foods = _getFoodsDelegate.Invoke();

            Assert.AreEqual(4, foods.Count);

            Assert.IsTrue(foods.TryGetValue("A0001", out Food afood));
            Assert.IsTrue(foods.TryGetValue("A0001#F01.DESCB", out Food bfood));
            Assert.IsTrue(foods.TryGetValue("C0001#F02.DESCA$F03.DESCC", out Food cfood));
            Assert.IsTrue(foods.TryGetValue("C0001", out Food cparent));

            Assert.AreEqual("Afood", afood.Name);
            Assert.AreEqual("Afood - Descriptor B", bfood.Name);
            Assert.AreEqual("C0001 - Descriptor A - Descriptor C", cfood.Name);
            Assert.AreEqual("C0001", cparent.Name);

            Assert.AreEqual(0, afood.FoodFacets.Count);
            Assert.AreEqual(1, bfood.FoodFacets.Count);
            Assert.AreEqual(2, cfood.FoodFacets.Count);

            Assert.AreEqual(afood, bfood.Parent);
            Assert.AreEqual(cparent, cfood.Parent);

            //not added to children (?)
            Assert.AreEqual(0, afood.Children.Count);
            Assert.AreEqual(0, cparent.Children.Count);
        }
    }
}
