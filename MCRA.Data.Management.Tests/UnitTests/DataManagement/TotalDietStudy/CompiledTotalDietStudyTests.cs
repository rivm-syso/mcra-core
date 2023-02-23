using MCRA.Data.Compiled.Objects;
using MCRA.General;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Data.Management.Test.UnitTests.DataManagement {
    public class CompiledTotalDietStudyTests : CompiledTestsBase {
        protected Func<IList<TDSFoodSampleComposition>> _getItemsDelegate;

        [TestMethod]
        public void CompiledTotalDietStudy_TestSimple() {
            _rawDataProvider.SetDataTables(
                (ScopingType.TdsFoodSampleCompositions, @"TotalDietStudyTests\TDSFoodSampleCompositionsSimple")
            );

            var compositions = _getItemsDelegate.Invoke();

            Assert.AreEqual(15, compositions.Count);

            var codes = compositions.Select(c => c.Food.Code).Distinct().ToList();
            CollectionAssert.AreEquivalent(new[] { "f1", "f2", "f4", "f5", "f6" }, codes);

            codes = compositions.Select(c => c.TDSFood.Code).Distinct().ToList();
            CollectionAssert.AreEquivalent(new[] { "t1", "t2", "t3", "t4", "t5" }, codes);
        }

        [TestMethod]
        public void CompiledTotalDietStudy_TestFoodFilter() {
            _rawDataProvider.SetDataTables(
                (ScopingType.TdsFoodSampleCompositions, @"TotalDietStudyTests\TDSFoodSampleCompositionsSimple")
            );
            _rawDataProvider.SetFilterCodes(ScopingType.Foods, new[] { "f1", "f2", "t1", "t2" });

            var compositions = _getItemsDelegate.Invoke();

            Assert.AreEqual(3, compositions.Count);

            var codes = compositions.Select(c => c.Food.Code).Distinct().ToList();
            CollectionAssert.AreEquivalent(new[] { "f1", "f2" }, codes);

            codes = compositions.Select(c => c.TDSFood.Code).Distinct().ToList();
            CollectionAssert.AreEquivalent(new[] { "t1", "t2" }, codes);
        }
    }
}
