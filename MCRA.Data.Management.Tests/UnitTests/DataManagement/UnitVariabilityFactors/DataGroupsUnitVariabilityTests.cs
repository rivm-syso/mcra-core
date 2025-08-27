using MCRA.Data.Compiled.Wrappers;
using MCRA.General;

namespace MCRA.Data.Management.Test.UnitTests.DataManagement {
    [TestClass]
    public class DataGroupsUnitVariabilityTests:CompiledTestsBase {

        /// <summary>
        /// Tests correct loading of the unit variability factors by counting the number of
        /// unit variability factors loaded from the data and counting the unit variability
        /// factors linked to each food, substance and processingtype.
        /// </summary>
        [TestMethod]
        public void UnitVariabilityFactorsDataTest1() {
            _rawDataProvider.SetDataGroupsFromFolder(
                1,
                "_DataGroupsTest",
                [SourceTableGroup.UnitVariabilityFactors, SourceTableGroup.Foods, SourceTableGroup.Compounds]);

            var unitVariabilityFactors = _compiledDataManager.GetAllUnitVariabilityFactors();
            Assert.AreEqual(5, unitVariabilityFactors.Count);

            var foods = _compiledDataManager.GetAllFoods();
            var foodApple = foods["APPLE"];
            var foodBananas = foods["BANANAS"];
            var foodPineapple = foods["PINEAPPLE"];

            var compounds = _compiledDataManager.GetAllCompounds();
            var compoundA = compounds["CompoundA"];
            var compoundB = compounds["CompoundB"];

            var processingTypes = _compiledDataManager.GetAllProcessingTypes().Values;
            var processingTypeJuicing = processingTypes.Single(pt => pt.Name == "Juicing");
            var processingTypePeeling = processingTypes.Single(pt => pt.Name == "Peeling");

            Assert.AreEqual(3, unitVariabilityFactors.Count(r => r.Food == foodApple));
            Assert.AreEqual(1, unitVariabilityFactors.Count(r => r.Food == foodBananas));
            Assert.AreEqual(1, unitVariabilityFactors.Count(r => r.Food == foodPineapple));

        }

        /// <summary>
        /// Tests correct loading of the unit variability factors. Tests correct values of the
        /// factor, coefficient, and units in composite sample fields.
        /// </summary>
        [TestMethod]
        public void UnitVariabilityFactorsDataTest2() {
            _rawDataProvider.SetDataGroupsFromFolder(
                1,
                "_DataGroupsTest",
                [SourceTableGroup.UnitVariabilityFactors, SourceTableGroup.Foods, SourceTableGroup.Compounds]
            );
            var unitVariabilityFactors = _compiledDataManager.GetAllUnitVariabilityFactors()
                    .GroupBy(uv => uv.Food)
                    .Select(g => new FoodUnitVariabilityInfo(g.Key, g.ToList()))
                    .ToDictionary(r => r.Food);

            var foods = _compiledDataManager.GetAllFoods();
            var foodApple = foods["APPLE"];
            var foodBananas = foods["BANANAS"];
            var foodPineapple = foods["PINEAPPLE"];

            var compounds = _compiledDataManager.GetAllCompounds();
            var compoundA = compounds["CompoundA"];
            var compoundB = compounds["CompoundB"];

            var processingTypes = _compiledDataManager.GetAllProcessingTypes();
            var processingTypeJuicing = processingTypes.Single(pt => pt.Value.Name == "Juicing").Value;
            var processingTypePeeling = processingTypes.Single(pt => pt.Value.Name == "Peeling").Value;

            Assert.AreEqual(3, unitVariabilityFactors[foodApple].UnitVariabilityFactors.Count);
            Assert.AreEqual(1, unitVariabilityFactors[foodBananas].UnitVariabilityFactors.Count);
            Assert.AreEqual(1, unitVariabilityFactors[foodPineapple].UnitVariabilityFactors.Count);

            // Apple
            var processingTypeAppleCompoundAPeeling = unitVariabilityFactors[foodApple].GetOrCeateMostSpecificVariabilityFactor(compoundA, processingTypePeeling, 0, 0);
            var processingTypeAppleCompoundBPeeling = unitVariabilityFactors[foodApple].GetOrCeateMostSpecificVariabilityFactor(compoundB, processingTypePeeling, 0, 0);
            var processingTypeAppleCompoundAJuicing = unitVariabilityFactors[foodApple].GetOrCeateMostSpecificVariabilityFactor(compoundA, processingTypeJuicing, 0, 0);
            var processingTypeAppleCompoundBJuicing = unitVariabilityFactors[foodApple].GetOrCeateMostSpecificVariabilityFactor(compoundB, processingTypeJuicing, 0, 0);

            // Factors
            Assert.AreEqual(5, processingTypeAppleCompoundAPeeling.Factor);
            Assert.IsNull(processingTypeAppleCompoundBPeeling.Factor);
            Assert.AreEqual(7, processingTypeAppleCompoundAJuicing.Factor);
            Assert.IsNull(processingTypeAppleCompoundBJuicing.Factor);

            // Coefficients
            Assert.IsNull(processingTypeAppleCompoundAPeeling.Coefficient);
            Assert.AreEqual(1.86, processingTypeAppleCompoundBPeeling.Coefficient);
            Assert.AreEqual(2.2, processingTypeAppleCompoundAJuicing.Coefficient);
            Assert.AreEqual(1.86, processingTypeAppleCompoundBJuicing.Coefficient);

            // Coefficients
            Assert.IsNull(processingTypeAppleCompoundAPeeling.Coefficient);
            Assert.AreEqual(1.86, processingTypeAppleCompoundBPeeling.Coefficient);
            Assert.AreEqual(2.2, processingTypeAppleCompoundAJuicing.Coefficient);
            Assert.AreEqual(1.86, processingTypeAppleCompoundBJuicing.Coefficient);

            // Units in composite sample
            Assert.AreEqual(24, processingTypeAppleCompoundAPeeling.UnitsInCompositeSample);
            Assert.AreEqual(12, processingTypeAppleCompoundBPeeling.UnitsInCompositeSample);
            Assert.AreEqual(100, processingTypeAppleCompoundAJuicing.UnitsInCompositeSample);
            Assert.AreEqual(12, processingTypeAppleCompoundBJuicing.UnitsInCompositeSample);

        }

    }
}
