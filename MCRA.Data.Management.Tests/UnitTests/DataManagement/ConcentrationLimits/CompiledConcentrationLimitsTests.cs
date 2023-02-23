using MCRA.Data.Compiled.Objects;
using MCRA.General;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Data.Management.Test.UnitTests.DataManagement {
    public class CompiledConcentrationLimitsTests : CompiledTestsBase {

        protected Func<ICollection<ConcentrationLimit>> _getItemsDelegate;

        [TestMethod]
        public void CompiledConcentrationLimits_TestSimple() {
            _rawDataProvider.SetDataTables(
                (ScopingType.ConcentrationLimits, @"MaximumResidueLimitsTests\MaximumResidueLimitsSimple")
            );

            var limits = _getItemsDelegate.Invoke();

            var compoundCodes = limits.Select(f => f.Compound.Code).Distinct();
            var foodCodes = limits.Select(f => f.Food.Code).Distinct();

            CollectionAssert.AreEquivalent(new[] { "A", "B", "C", "D", "E", "F" }, compoundCodes.ToList());
            CollectionAssert.AreEquivalent(new[] { "f1", "f2", "f3", "f4", "f5", "f6" }, foodCodes.ToList());
        }

        [TestMethod]
        public void CompiledConcentrationLimits_TestSimpleFoodsFilter() {
            _rawDataProvider.SetDataTables(
                (ScopingType.ConcentrationLimits, @"MaximumResidueLimitsTests\MaximumResidueLimitsSimple")
            );

            //set a filter scope on Foods
            _rawDataProvider.SetFilterCodes(ScopingType.Foods, new[] { "f1" });

            var limits = _getItemsDelegate.Invoke();
            Assert.AreEqual(1, limits.Count);
            var f = limits.First();

            Assert.AreEqual("B", f.Compound.Code);
            Assert.AreEqual("f1", f.Food.Code);
        }

        [TestMethod]
        public void CompiledConcentrationLimits_TestSimpleCompoundsFilter() {
            _rawDataProvider.SetDataTables(
                (ScopingType.ConcentrationLimits, @"MaximumResidueLimitsTests\MaximumResidueLimitsSimple")
            );

            //set a filter scope on compounds
            _rawDataProvider.SetFilterCodes(ScopingType.Compounds, new[] { "B", "C" });

            var limits = _getItemsDelegate.Invoke();

            var compoundCodes = limits.Select(f => f.Compound.Code).Distinct();
            var foodCodes = limits.Select(f => f.Food.Code).Distinct();

            CollectionAssert.AreEquivalent(new[] { "B", "C" }, compoundCodes.ToList());
            CollectionAssert.AreEquivalent(new[] { "f1", "f2", "f3", "f4", "f5" }, foodCodes.ToList());
        }

        [TestMethod]
        public void CompiledConcentrationLimits_TestFilterFoodsAndCompoundsSimple() {
            _rawDataProvider.SetDataTables(
                (ScopingType.ConcentrationLimits, @"MaximumResidueLimitsTests\MaximumResidueLimitsSimple")
            );

            //set a filter scope on Foods
            _rawDataProvider.SetFilterCodes(ScopingType.Foods, new[] { "f1", "f4" });
            //set a filter scope on compounds
            _rawDataProvider.SetFilterCodes(ScopingType.Compounds, new[] { "B", "C" });

            var limits = _getItemsDelegate.Invoke();

            var compoundCodes = limits.Select(f => f.Compound.Code).Distinct();
            var foodCodes = limits.Select(f => f.Food.Code).Distinct();

            CollectionAssert.AreEquivalent(new[] { "B", "C" }, compoundCodes.ToList());
            CollectionAssert.AreEquivalent(new[] { "f1", "f4" }, foodCodes.ToList());
        }
    }
}
