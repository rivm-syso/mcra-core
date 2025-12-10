using MCRA.Data.Management.Tests.UnitTests.DataManagement;
using MCRA.General;

namespace MCRA.Data.Management.Test.UnitTests.DataManagement {
    [TestClass]
    public class CompiledDietaryExposuresTests : CompiledTestsBase {
        [TestMethod]
        [DataRow(ManagerType.CompiledDataManager)]
        [DataRow(ManagerType.SubsetManager)]
        public void CompiledDietaryExposures_TestExposuresOnly(ManagerType managerType) {
            RawDataProvider.SetDataTables(
                (ScopingType.DietaryExposureModels, @"DietaryExposuresTests/DietaryExposureModels"),
                (ScopingType.DietaryExposurePercentiles, @"DietaryExposuresTests/DietaryExposurePercentiles"),
                (ScopingType.DietaryExposurePercentilesUncertain, @"DietaryExposuresTests/DietaryExposurePercentilesUncertain")
            );

            // Only experiments with all matching codes are loaded (matching response codes are mandatory)
            var models = GetAllDietaryExposureModels(managerType);
            Assert.HasCount(2, models);

            // Count 4 percentiles
            Assert.IsTrue(models.Values.All(r => r.DietaryExposurePercentiles.Count == 4));

            // Count 5 uncertainty sets
            Assert.IsTrue(models.Values.All(r => r.DietaryExposurePercentiles.All(p => p.Value.ExposureUncertainties.Count == 5)));

            // Substances are loaded from valid experiments, so only 4 in this case
            var substances = GetAllCompounds(managerType);
            Assert.HasCount(1, substances);
        }
    }
}
