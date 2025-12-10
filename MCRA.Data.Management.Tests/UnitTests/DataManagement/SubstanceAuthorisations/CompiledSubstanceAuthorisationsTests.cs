using MCRA.Data.Management.Tests.UnitTests.DataManagement;
using MCRA.General;

namespace MCRA.Data.Management.Test.UnitTests.DataManagement {
    [TestClass]
    public class CompiledSubstanceAuthorisationsTests : CompiledTestsBase {
        [TestMethod]
        [DataRow(ManagerType.CompiledDataManager)]
        [DataRow(ManagerType.SubsetManager)]
        public void CompiledSubstanceAuthorisations_TestSimple(ManagerType managerType) {
            RawDataProvider.SetDataTables(
                (ScopingType.SubstanceAuthorisations, @"AuthorisedUsesTests/AuthorisedUsesSimple")
            );

            var records = GetAllSubstanceAuthorisations(managerType);

            var compoundCodes = records.Select(f => f.Substance.Code).Distinct();
            var foodCodes = records.Select(f => f.Food.Code).Distinct();

            CollectionAssert.AreEquivalent(new[] { "A", "B", "C", "D", "E", "F" }, compoundCodes.ToList());
            CollectionAssert.AreEquivalent(new[] { "f1", "f2", "f3", "f4", "f5", "f6" }, foodCodes.ToList());
        }

        [TestMethod]
        [DataRow(ManagerType.CompiledDataManager)]
        [DataRow(ManagerType.SubsetManager)]
        public void CompiledSubstanceAuthorisations_TestSimpleFoodsFilter(ManagerType managerType) {
            RawDataProvider.SetDataTables(
                (ScopingType.SubstanceAuthorisations, @"AuthorisedUsesTests/AuthorisedUsesSimple")
            );
            RawDataProvider.SetFilterCodes(ScopingType.Foods, ["f1"]);

            var records = GetAllSubstanceAuthorisations(managerType);
            Assert.HasCount(1, records);
            var f = records.First();

            Assert.AreEqual("B", f.Substance.Code);
            Assert.AreEqual("f1", f.Food.Code);
        }

        [TestMethod]
        [DataRow(ManagerType.CompiledDataManager)]
        [DataRow(ManagerType.SubsetManager)]
        public void CompiledSubstanceAuthorisations_TestSimpleCompoundsFilter(ManagerType managerType) {
            RawDataProvider.SetDataTables(
                (ScopingType.SubstanceAuthorisations, @"AuthorisedUsesTests/AuthorisedUsesSimple")
            );
            RawDataProvider.SetFilterCodes(ScopingType.Compounds, ["B", "C"]);

            var records = GetAllSubstanceAuthorisations(managerType);

            var compoundCodes = records.Select(f => f.Substance.Code).Distinct();
            var foodCodes = records.Select(f => f.Food.Code).Distinct();

            CollectionAssert.AreEquivalent(new[] { "B", "C" }, compoundCodes.ToList());
            CollectionAssert.AreEquivalent(new[] { "f1", "f2", "f3", "f4", "f5" }, foodCodes.ToList());
        }

        [TestMethod]
        [DataRow(ManagerType.CompiledDataManager)]
        [DataRow(ManagerType.SubsetManager)]
        public void CompiledSubstanceAuthorisations_TestFilterFoodsAndCompoundsSimple(ManagerType managerType) {
            RawDataProvider.SetDataTables(
                (ScopingType.SubstanceAuthorisations, @"AuthorisedUsesTests/AuthorisedUsesSimple")
            );
            RawDataProvider.SetFilterCodes(ScopingType.Foods, ["f1", "f4"]);
            RawDataProvider.SetFilterCodes(ScopingType.Compounds, ["B", "C"]);

            var records = GetAllSubstanceAuthorisations(managerType);

            var compoundCodes = records.Select(f => f.Substance.Code).Distinct();
            var foodCodes = records.Select(f => f.Food.Code).Distinct();

            CollectionAssert.AreEquivalent(new[] { "B", "C" }, compoundCodes.ToList());
            CollectionAssert.AreEquivalent(new[] { "f1", "f4" }, foodCodes.ToList());
        }
    }
}
