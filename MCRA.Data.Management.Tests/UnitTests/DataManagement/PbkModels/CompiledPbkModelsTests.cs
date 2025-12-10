using MCRA.Data.Management.Tests.UnitTests.DataManagement;
using MCRA.General;

namespace MCRA.Data.Management.Test.UnitTests.DataManagement {
    [TestClass]
    public class CompiledPbkModelsTests : CompiledTestsBase {

        [TestMethod]
        [DataRow(ManagerType.CompiledDataManager)]
        [DataRow(ManagerType.SubsetManager)]
        public void CompiledKineticModelInstancesSimpleTest(ManagerType managerType) {
            RawDataProvider.SetDataTables(
                (ScopingType.KineticModelInstances, @"KineticModelsTests/KineticModelInstancesSimple")
            );

            var models = GetAllPbkModels(managerType);
            var modelIds = models.Select(m => m.IdModelInstance).ToList();

            CollectionAssert.AreEqual(new[] { "km01", "km02", "km03", "km04", "km05", "km06", "km07", "km08", "km09", "km10" }, modelIds);

            Assert.AreEqual(0, models.Sum(a => a.KineticModelInstanceParameters?.Count ?? 0));
        }

        [TestMethod]
        [DataRow(ManagerType.CompiledDataManager)]
        [DataRow(ManagerType.SubsetManager)]
        public void CompiledKineticModelInstancesSimpleSubstanceFilterTest(ManagerType managerType) {
            RawDataProvider.SetDataTables(
                (ScopingType.KineticModelInstances, @"KineticModelsTests/KineticModelInstancesSimple")
            );
            RawDataProvider.SetFilterCodes(ScopingType.Compounds, ["B", "C"]);

            var models = GetAllPbkModels(managerType);
            var modelIds = models.Select(m => m.IdModelInstance).ToList();

            CollectionAssert.AreEqual(new[] { "km02", "km03", "km07", "km08" }, modelIds);

            Assert.AreEqual(0, models.Sum(a => a.KineticModelInstanceParameters?.Count ?? 0));
        }
    }
}
