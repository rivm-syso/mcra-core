using MCRA.Data.Compiled.Objects;
using MCRA.General;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Data.Management.Test.UnitTests.DataManagement {
    public class CompiledUnitVariabilityFactorsTests : CompiledTestsBase {
        protected Func<ICollection<UnitVariabilityFactor>> _getItemsDelegate;

        [TestMethod]
        public void CompiledUnitVariabilityFactorsSimpleTest() {
            _rawDataProvider.SetDataTables(
                (ScopingType.UnitVariabilityFactors, @"UnitVariabilityFactorsTests/UnitVariabilityFactorsSimple")
            );

            var factors = _getItemsDelegate.Invoke();

            Assert.AreEqual(5, factors.Count(f => f.Compound == null));
            Assert.AreEqual(10, factors.Count(f => f.Compound != null));
            Assert.AreEqual(4, factors.Count(f => f.ProcessingType == null));
            Assert.AreEqual(11, factors.Count(f => f.ProcessingType != null));

            var compoundCodes = factors.Where(f => f.Compound != null).Select(f => f.Compound.Code).Distinct();
            var proctypeCodes = factors.Where(f => f.ProcessingType != null).Select(f => f.ProcessingType.Code).Distinct();
            var foodCodes = factors.Select(f => f.Food.Code).Distinct();

            CollectionAssert.AreEquivalent(new[] { "A", "B", "C", "D", "E", "F" }, compoundCodes.ToList());
            CollectionAssert.AreEquivalent(new[] { "t1", "t2", "t3", "t4", "t5" }, proctypeCodes.ToList());
            CollectionAssert.AreEquivalent(new[] { "f1", "f2", "f3", "f4", "f5", "f6", "f7", "f8" }, foodCodes.ToList());
        }

        [TestMethod]
        public void CompiledUnitVariabilityFactorsSimpleProcessingTypesFilterTest() {
            _rawDataProvider.SetDataTables(
                (ScopingType.UnitVariabilityFactors, @"UnitVariabilityFactorsTests/UnitVariabilityFactorsSimple")
            );

            var factors = _getItemsDelegate.Invoke();

            Assert.AreEqual(5, factors.Count(f => f.Compound == null));
            Assert.AreEqual(10, factors.Count(f => f.Compound != null));
            Assert.AreEqual(4, factors.Count(f => f.ProcessingType == null));
            Assert.AreEqual(11, factors.Count(f => f.ProcessingType != null));

            var compoundCodes = factors.Where(f => f.Compound != null).Select(f => f.Compound.Code).Distinct();
            var proctypeCodes = factors.Where(f => f.ProcessingType != null).Select(f => f.ProcessingType.Code).Distinct();
            var foodCodes = factors.Select(f => f.Food.Code).Distinct();

            CollectionAssert.AreEquivalent(new[] { "A", "B", "C", "D", "E", "F" }, compoundCodes.ToList());
            CollectionAssert.AreEquivalent(new[] { "t1", "t2", "t3", "t4", "t5" }, proctypeCodes.ToList());
            CollectionAssert.AreEquivalent(new[] { "f1", "f2", "f3", "f4", "f5", "f6", "f7", "f8" }, foodCodes.ToList());
        }

        [TestMethod]
        public void CompiledUnitVariabilityFactorsSimpleFoodsFilterTest() {
            _rawDataProvider.SetDataTables(
                (ScopingType.UnitVariabilityFactors, @"UnitVariabilityFactorsTests/UnitVariabilityFactorsSimple")
            );
            _rawDataProvider.SetFilterCodes(ScopingType.Foods, ["f1"]);

            var factors = _getItemsDelegate.Invoke();
            Assert.AreEqual(1, factors.Count);
            var f = factors.First();

            Assert.IsNull(f.Compound);
            Assert.IsNull(f.ProcessingType);
            Assert.AreEqual("f1", f.Food.Code);
        }

        [TestMethod]
        public void CompiledUnitVariabilityFactorsSimpleCompoundsFilterTest() {
            _rawDataProvider.SetDataTables(
                (ScopingType.UnitVariabilityFactors, @"UnitVariabilityFactorsTests/UnitVariabilityFactorsSimple")
            );
            _rawDataProvider.SetFilterCodes(ScopingType.Compounds, ["B", "C"]);

            var factors = _getItemsDelegate.Invoke();

            Assert.AreEqual(5, factors.Count(f => f.Compound == null));
            Assert.AreEqual(4, factors.Count(f => f.Compound != null));
            Assert.AreEqual(2, factors.Count(f => f.ProcessingType == null));
            Assert.AreEqual(7, factors.Count(f => f.ProcessingType != null));

            var compoundCodes = factors.Where(f => f.Compound != null).Select(f => f.Compound.Code).Distinct();
            var proctypeCodes = factors.Where(f => f.ProcessingType != null).Select(f => f.ProcessingType.Code).Distinct();
            var foodCodes = factors.Select(f => f.Food.Code).Distinct();

            CollectionAssert.AreEquivalent(new[] { "B", "C" }, compoundCodes.ToList());
            CollectionAssert.AreEquivalent(new[] { "t1", "t2", "t3", "t4", "t5" }, proctypeCodes.ToList());
            CollectionAssert.AreEquivalent(new[] { "f1", "f2", "f3", "f4", "f5", "f6", "f7" }, foodCodes.ToList());
        }

        [TestMethod]
        public void CompiledUnitVariabilityFactorsFilterFoodsAndCompoundsSimpleTest() {
            _rawDataProvider.SetDataTables(
                (ScopingType.UnitVariabilityFactors, @"UnitVariabilityFactorsTests/UnitVariabilityFactorsSimple")
            );
            _rawDataProvider.SetFilterCodes(ScopingType.Foods, ["f1", "f4", "f7"]);
            _rawDataProvider.SetFilterCodes(ScopingType.Compounds, ["B", "C"]);

            var factors = _getItemsDelegate.Invoke();

            Assert.AreEqual(3, factors.Count(f => f.Compound == null));
            Assert.AreEqual(1, factors.Count(f => f.Compound != null));
            Assert.AreEqual(2, factors.Count(f => f.ProcessingType == null));
            Assert.AreEqual(2, factors.Count(f => f.ProcessingType != null));

            var compoundCodes = factors.Where(f => f.Compound != null).Select(f => f.Compound.Code).Distinct();
            var proctypeCodes = factors.Where(f => f.ProcessingType != null).Select(f => f.ProcessingType.Code).Distinct();
            var foodCodes = factors.Select(f => f.Food.Code).Distinct();

            CollectionAssert.AreEquivalent(new[] { "C" }, compoundCodes.ToList());
            CollectionAssert.AreEquivalent(new[] { "t4", "t5" }, proctypeCodes.ToList());
            CollectionAssert.AreEquivalent(new[] { "f1", "f4", "f7" }, foodCodes.ToList());
        }
    }
}
