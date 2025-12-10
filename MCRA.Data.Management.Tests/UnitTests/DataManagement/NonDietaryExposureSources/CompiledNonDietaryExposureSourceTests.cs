using MCRA.Data.Management.Tests.UnitTests.DataManagement;
using MCRA.General;

namespace MCRA.Data.Management.Test.UnitTests.DataManagement {

    /// <summary>
    /// Note: this is not marked as TestClass, the subclasses define the method to use
    /// to retrieve the non-dietary exposure sources (_getNonDietaryExposureSourcesDelegate)
    /// These tests are run multiple times (the subclasses) because of the redundancy in
    /// non-dietary exposure sources retrieval in the CompiledDataManager and SubsetManager.
    /// </summary>
    [TestClass]
    public class CompiledNonDietaryExposureSourceTests : CompiledTestsBase {

        [TestMethod]
        [DataRow(ManagerType.CompiledDataManager)]
        [DataRow(ManagerType.SubsetManager)]
        public virtual void CompiledNonDietaryExposureSources_TestGetAllNonDietaryExposureSources(ManagerType managerType) {
            RawDataProvider.SetDataTables(
                (ScopingType.NonDietaryExposureSources, @"NonDietaryExposureSourcesTests/NonDietaryExposureSourcesSimple")
            );

            var sources = GetAllNonDietaryExposureSources(managerType);

            Assert.HasCount(3, sources);

            Assert.IsTrue(sources.TryGetValue("A", out var s) && s.Name.Equals("Aftershave"));
            Assert.IsTrue(sources.TryGetValue("B", out s) && s.Name.Equals("Body lotion"));
            Assert.IsTrue(sources.TryGetValue("C", out s) && s.Name.Equals("Conditioner"));
        }

        [TestMethod]
        [DataRow(ManagerType.CompiledDataManager)]
        [DataRow(ManagerType.SubsetManager)]
        public virtual void CompiledNonDietaryExposureSources_TestGetAllNonDietaryExposureSourcesFiltered(ManagerType managerType) {
            RawDataProvider.SetDataTables(
                (ScopingType.NonDietaryExposureSources, @"NonDietaryExposureSourcesTests/NonDietaryExposureSourcesSimple")
            );
            RawDataProvider.SetFilterCodes(ScopingType.NonDietaryExposureSources, ["A", "C"]);

            var sources = GetAllNonDietaryExposureSources(managerType);
            Assert.HasCount(2, sources);

            Assert.IsTrue(sources.TryGetValue("A", out var s) && s.Name.Equals("Aftershave"));
            Assert.IsTrue(sources.TryGetValue("C", out s) && s.Name.Equals("Conditioner"));
        }
    }
}
