using MCRA.Data.Management.Tests.UnitTests.DataManagement;
using MCRA.General;

namespace MCRA.Data.Management.Test.UnitTests.DataManagement {
    [TestClass]
    public class CompiledDoseResponseDataTests : CompiledTestsBase {
        [TestMethod]
        [DataRow(ManagerType.CompiledDataManager)]
        [DataRow(ManagerType.SubsetManager)]
        public void CompiledDoseResponseExperiments_TestSimple(ManagerType managerType) {
            RawDataProvider.SetDataTables(
                (ScopingType.DoseResponseExperiments, @"DoseResponseTests/ExperimentsSimple"),
                (ScopingType.Responses, @"DoseResponseTests/ResponsesSimple")
            );

            var experiments = GetAllDoseResponseExperiments(managerType);
            //only experiments with all matching codes are loaded
            //(matching response codes are mandatory)
            Assert.HasCount(2, experiments);

            var substances = GetAllCompounds(managerType);

            //Substances are loaded from valid experiments
            Assert.HasCount(5, substances);

            var responses = GetAllResponses(managerType);

            Assert.HasCount(3, responses);
        }

        [TestMethod]
        [DataRow(ManagerType.CompiledDataManager)]
        [DataRow(ManagerType.SubsetManager)]
        public void CompiledDoseResponseExperiments_TestFilterSubstancesResponses(ManagerType managerType) {
            RawDataProvider.SetDataTables(
                (ScopingType.DoseResponseExperiments, @"DoseResponseTests/ExperimentsForFiltering"),
                (ScopingType.Responses, @"DoseResponseTests/ResponsesSimple")
            );

            RawDataProvider.SetFilterCodes(ScopingType.Compounds, ["A", "B"]);
            RawDataProvider.SetFilterCodes(ScopingType.Responses, ["R2"]);

            var experiments = GetAllDoseResponseExperiments(managerType);

            Assert.HasCount(6, experiments);
            CollectionAssert.AreEquivalent(
                new[] { "x10", "x11", "x14", "x19", "x20", "x23" },
                experiments.Keys.ToList()
            );
        }

        [TestMethod]
        [DataRow(ManagerType.CompiledDataManager)]
        [DataRow(ManagerType.SubsetManager)]
        public void CompiledDoseResponseExperiments_TestFilterResponses(ManagerType managerType) {
            RawDataProvider.SetDataTables(
                (ScopingType.DoseResponseExperiments, @"DoseResponseTests/ExperimentsForFiltering"),
                (ScopingType.Responses, @"DoseResponseTests/ResponsesSimple")
            );

            RawDataProvider.SetFilterCodes(ScopingType.Responses, ["R2"]);

            var experiments = GetAllDoseResponseExperiments(managerType);

            Assert.HasCount(18, experiments);
            CollectionAssert.AreEquivalent(
                new[] { "x10", "x11", "x12", "x13", "x14", "x15", "x16", "x17", "x18", "x19", "x20", "x21", "x22", "x23", "x24", "x25", "x26", "x27" },
                experiments.Keys.ToList()
            );
        }

        [TestMethod]
        [DataRow(ManagerType.CompiledDataManager)]
        [DataRow(ManagerType.SubsetManager)]
        public void CompiledDoseResponseExperiments_TestFilterSubstances(ManagerType managerType) {
            RawDataProvider.SetDataTables(
                (ScopingType.DoseResponseExperiments, @"DoseResponseTests/ExperimentsForFiltering"),
                (ScopingType.Responses, @"DoseResponseTests/ResponsesSimple")
            );

            RawDataProvider.SetFilterCodes(ScopingType.Compounds, ["A", "B"]);

            var experiments = GetAllDoseResponseExperiments(managerType);

            Assert.HasCount(12, experiments);
            CollectionAssert.AreEquivalent(
                new[] { "x01", "x02", "x05", "x10", "x11", "x14", "x19", "x20", "x23", "x28", "x29", "x32" },
                experiments.Keys.ToList()
            );
        }
    }
}
