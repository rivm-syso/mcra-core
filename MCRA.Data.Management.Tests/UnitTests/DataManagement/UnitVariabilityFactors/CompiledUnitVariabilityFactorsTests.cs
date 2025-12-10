using MCRA.Data.Management.Tests.UnitTests.DataManagement;
using MCRA.General;

namespace MCRA.Data.Management.Test.UnitTests.DataManagement {
    [TestClass]
    public class CompiledUnitVariabilityFactorsTests : CompiledTestsBase {
        [TestMethod]
        [DataRow(ManagerType.CompiledDataManager)]
        [DataRow(ManagerType.SubsetManager)]
        public void CompiledUnitVariabilityFactorsSimpleTest(ManagerType managerType) {
            RawDataProvider.SetDataTables(
                (ScopingType.UnitVariabilityFactors, @"UnitVariabilityFactorsTests/UnitVariabilityFactorsSimple")
            );

            var factors = GetAllUnitVariabilityFactors(managerType);

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
        [DataRow(ManagerType.CompiledDataManager)]
        [DataRow(ManagerType.SubsetManager)]
        public void CompiledUnitVariabilityFactorsSimpleProcessingTypesFilterTest(ManagerType managerType) {
            RawDataProvider.SetDataTables(
                (ScopingType.UnitVariabilityFactors, @"UnitVariabilityFactorsTests/UnitVariabilityFactorsSimple")
            );

            var factors = GetAllUnitVariabilityFactors(managerType);

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
        [DataRow(ManagerType.CompiledDataManager)]
        [DataRow(ManagerType.SubsetManager)]
        public void CompiledUnitVariabilityFactorsSimpleFoodsFilterTest(ManagerType managerType) {
            RawDataProvider.SetDataTables(
                (ScopingType.UnitVariabilityFactors, @"UnitVariabilityFactorsTests/UnitVariabilityFactorsSimple")
            );
            RawDataProvider.SetFilterCodes(ScopingType.Foods, ["f1"]);

            var factors = GetAllUnitVariabilityFactors(managerType);
            Assert.HasCount(1, factors);
            var f = factors.First();

            Assert.IsNull(f.Compound);
            Assert.IsNull(f.ProcessingType);
            Assert.AreEqual("f1", f.Food.Code);
        }

        [TestMethod]
        [DataRow(ManagerType.CompiledDataManager)]
        [DataRow(ManagerType.SubsetManager)]
        public void CompiledUnitVariabilityFactorsSimpleCompoundsFilterTest(ManagerType managerType) {
            RawDataProvider.SetDataTables(
                (ScopingType.UnitVariabilityFactors, @"UnitVariabilityFactorsTests/UnitVariabilityFactorsSimple")
            );
            RawDataProvider.SetFilterCodes(ScopingType.Compounds, ["B", "C"]);

            var factors = GetAllUnitVariabilityFactors(managerType);

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
        [DataRow(ManagerType.CompiledDataManager)]
        [DataRow(ManagerType.SubsetManager)]
        public void CompiledUnitVariabilityFactorsFilterFoodsAndCompoundsSimpleTest(ManagerType managerType) {
            RawDataProvider.SetDataTables(
                (ScopingType.UnitVariabilityFactors, @"UnitVariabilityFactorsTests/UnitVariabilityFactorsSimple")
            );
            RawDataProvider.SetFilterCodes(ScopingType.Foods, ["f1", "f4", "f7"]);
            RawDataProvider.SetFilterCodes(ScopingType.Compounds, ["B", "C"]);

            var factors = GetAllUnitVariabilityFactors(managerType);

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
