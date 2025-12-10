using MCRA.Data.Management.Tests.UnitTests.DataManagement;
using MCRA.General;

namespace MCRA.Data.Management.Test.UnitTests.DataManagement {
    [TestClass]
    public class CompiledSubstanceConversionsTests : CompiledTestsBase {
        [TestMethod]
        [DataRow(ManagerType.CompiledDataManager)]
        [DataRow(ManagerType.SubsetManager)]
        public void CompiledSubstanceConversions_TestSimple(ManagerType managerType) {
            RawDataProvider.SetDataTables(
                (ScopingType.Compounds, @"SubstancesTests/SubstancesSimple"),
                (ScopingType.SubstanceConversions, @"ResidueDefinitionsTests/ResidueDefinitionsSimple")
            );

            var allDefinitions = GetAllSubstanceConversions(managerType);

            Assert.HasCount(2, allDefinitions);
        }

        [TestMethod]
        [DataRow(ManagerType.CompiledDataManager)]
        [DataRow(ManagerType.SubsetManager)]
        public void CompiledSubstanceConversions_TestEffectsFilter(ManagerType managerType) {
            RawDataProvider.SetDataTables(
                (ScopingType.Compounds, @"SubstancesTests/SubstancesSimple"),
                (ScopingType.SubstanceConversions, @"ResidueDefinitionsTests/ResidueDefinitionsSimple")
            );
            RawDataProvider.SetFilterCodes(ScopingType.Compounds, ["A", "B"]);

            var allDefinitions = GetAllSubstanceConversions(managerType);

            Assert.HasCount(1, allDefinitions);
        }
    }
}
