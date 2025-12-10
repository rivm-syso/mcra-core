using MCRA.Data.Management.Tests.UnitTests.DataManagement;
using MCRA.General;

namespace MCRA.Data.Management.Test.UnitTests.DataManagement {
    [TestClass]
    public class CompiledTotalDietStudyTests : CompiledTestsBase {
        [TestMethod]
        [DataRow(ManagerType.CompiledDataManager)]
        [DataRow(ManagerType.SubsetManager)]
        public void CompiledTotalDietStudy_TestSimple(ManagerType managerType) {
            RawDataProvider.SetDataTables(
                (ScopingType.TdsFoodSampleCompositions, @"TotalDietStudyTests/TDSFoodSampleCompositionsSimple")
            );

            var compositions = GetAllTDSFoodSampleCompositions(managerType);

            Assert.HasCount(15, compositions);

            var codes = compositions.Select(c => c.Food.Code).Distinct().ToList();
            CollectionAssert.AreEquivalent(new[] { "f1", "f2", "f4", "f5", "f6" }, codes);

            codes = compositions.Select(c => c.TDSFood.Code).Distinct().ToList();
            CollectionAssert.AreEquivalent(new[] { "t1", "t2", "t3", "t4", "t5" }, codes);
        }

        [TestMethod]
        [DataRow(ManagerType.CompiledDataManager)]
        [DataRow(ManagerType.SubsetManager)]
        public void CompiledTotalDietStudy_TestFoodFilter(ManagerType managerType) {
            RawDataProvider.SetDataTables(
                (ScopingType.TdsFoodSampleCompositions, @"TotalDietStudyTests/TDSFoodSampleCompositionsSimple")
            );
            RawDataProvider.SetFilterCodes(ScopingType.Foods, ["f1", "f2", "t1", "t2"]);

            var compositions = GetAllTDSFoodSampleCompositions(managerType);

            Assert.HasCount(3, compositions);

            var codes = compositions.Select(c => c.Food.Code).Distinct().ToList();
            CollectionAssert.AreEquivalent(new[] { "f1", "f2" }, codes);

            codes = compositions.Select(c => c.TDSFood.Code).Distinct().ToList();
            CollectionAssert.AreEquivalent(new[] { "t1", "t2" }, codes);
        }
    }
}
