using MCRA.Data.Management.Tests.UnitTests.DataManagement;
using MCRA.General;

namespace MCRA.Data.Management.Test.UnitTests.DataManagement {
    [TestClass]
    public class CompiledSubstancesTests : CompiledTestsBase {

        [TestMethod]
        [DataRow(ManagerType.CompiledDataManager)]
        [DataRow(ManagerType.SubsetManager)]
        [DataRow(ManagerType.SubsetManager, true)]
        public void CompiledSubstances_TestGetAllSubstances(ManagerType managerType, bool useDictionary = false) {
            RawDataProvider.SetDataTables(
                (ScopingType.Compounds, @"SubstancesTests/SubstancesSimple")
            );

            var substances = GetAllCompounds(managerType, useDictionary);

            Assert.HasCount(5, substances);
            Assert.IsTrue(substances.TryGetValue("A", out var c) && c.Name.Equals("SubstanceA"));
            Assert.IsTrue(substances.TryGetValue("B", out c) && c.Name.Equals("SubstanceB"));
            Assert.IsTrue(substances.TryGetValue("C", out c) && c.Name.Equals("SubstanceC"));
            Assert.IsTrue(substances.TryGetValue("D", out c) && c.Name.Equals("SubstanceD"));
            Assert.IsTrue(substances.TryGetValue("E", out c) && c.Name.Equals("SubstanceE"));
        }

        [TestMethod]
        [DataRow(ManagerType.CompiledDataManager)]
        [DataRow(ManagerType.SubsetManager)]
        [DataRow(ManagerType.SubsetManager, true)]
        public void CompiledSubstances_TestGetAllSubstancesFiltered(ManagerType managerType, bool useDictionary = false) {
            RawDataProvider.SetDataTables(
                (ScopingType.Compounds, @"SubstancesTests/SubstancesSimple")
            );
            RawDataProvider.SetFilterCodes(ScopingType.Compounds, ["B", "D"]);

            var substances = GetAllCompounds(managerType, useDictionary);

            Assert.HasCount(2, substances);
            Assert.IsTrue(substances.TryGetValue("B", out var c) && c.Name.Equals("SubstanceB"));
            Assert.IsTrue(substances.TryGetValue("D", out c) && c.Name.Equals("SubstanceD"));
        }
    }
}
