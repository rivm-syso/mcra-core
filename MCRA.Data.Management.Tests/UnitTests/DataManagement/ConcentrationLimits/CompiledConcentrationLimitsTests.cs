using MCRA.Data.Compiled.Objects;
using MCRA.Data.Management.Tests.UnitTests.DataManagement;
using MCRA.General;
using MCRA.General.Action.Settings;

namespace MCRA.Data.Management.Test.UnitTests.DataManagement {
    [TestClass]
    public class CompiledConcentrationLimitsTests : CompiledTestsBase {

        [TestMethod]
        [DataRow(ManagerType.CompiledDataManager)]
        [DataRow(ManagerType.SubsetManager)]
        public void CompiledConcentrationLimits_TestSimple(ManagerType managerType) {
            RawDataProvider.SetDataTables(
                (ScopingType.ConcentrationLimits, @"MaximumResidueLimitsTests/MaximumResidueLimitsSimple")
            );

            var limits = GetAllMaximumConcentrationLimits(managerType);

            var compoundCodes = limits.Select(f => f.Compound.Code).Distinct();
            var foodCodes = limits.Select(f => f.Food.Code).Distinct();

            CollectionAssert.AreEquivalent(new[] { "A", "B", "C", "D", "E", "F" }, compoundCodes.ToList());
            CollectionAssert.AreEquivalent(new[] { "f1", "f2", "f3", "f4", "f5", "f6" }, foodCodes.ToList());
        }

        [TestMethod]
        [DataRow(ManagerType.CompiledDataManager)]
        [DataRow(ManagerType.SubsetManager)]
        public void CompiledConcentrationLimits_TestSimpleFoodsFilter(ManagerType managerType) {
            RawDataProvider.SetDataTables(
                (ScopingType.ConcentrationLimits, @"MaximumResidueLimitsTests/MaximumResidueLimitsSimple")
            );

            //set a filter scope on Foods
            RawDataProvider.SetFilterCodes(ScopingType.Foods, ["f1"]);

            var limits = GetAllMaximumConcentrationLimits(managerType);
            Assert.HasCount(1, limits);
            var f = limits.First();

            Assert.AreEqual("B", f.Compound.Code);
            Assert.AreEqual("f1", f.Food.Code);
        }

        [TestMethod]
        [DataRow(ManagerType.CompiledDataManager)]
        [DataRow(ManagerType.SubsetManager)]
        public void CompiledConcentrationLimits_TestSimpleCompoundsFilter(ManagerType managerType) {
            RawDataProvider.SetDataTables(
                (ScopingType.ConcentrationLimits, @"MaximumResidueLimitsTests/MaximumResidueLimitsSimple")
            );

            //set a filter scope on compounds
            RawDataProvider.SetFilterCodes(ScopingType.Compounds, ["B", "C"]);

            var limits = GetAllMaximumConcentrationLimits(managerType);

            var compoundCodes = limits.Select(f => f.Compound.Code).Distinct();
            var foodCodes = limits.Select(f => f.Food.Code).Distinct();

            CollectionAssert.AreEquivalent(new[] { "B", "C" }, compoundCodes.ToList());
            CollectionAssert.AreEquivalent(new[] { "f1", "f2", "f3", "f4", "f5" }, foodCodes.ToList());
        }

        [TestMethod]
        [DataRow(ManagerType.CompiledDataManager)]
        [DataRow(ManagerType.SubsetManager)]
        public void CompiledConcentrationLimits_TestFilterFoodsAndCompoundsSimple(ManagerType managerType) {
            RawDataProvider.SetDataTables(
                (ScopingType.ConcentrationLimits, @"MaximumResidueLimitsTests/MaximumResidueLimitsSimple")
            );

            //set a filter scope on Foods
            RawDataProvider.SetFilterCodes(ScopingType.Foods, ["f1", "f4"]);
            //set a filter scope on compounds
            RawDataProvider.SetFilterCodes(ScopingType.Compounds, ["B", "C"]);

            var limits = GetAllMaximumConcentrationLimits(managerType);

            var compoundCodes = limits.Select(f => f.Compound.Code).Distinct();
            var foodCodes = limits.Select(f => f.Food.Code).Distinct();

            CollectionAssert.AreEquivalent(new[] { "B", "C" }, compoundCodes.ToList());
            CollectionAssert.AreEquivalent(new[] { "f1", "f4" }, foodCodes.ToList());
        }
    }
}
