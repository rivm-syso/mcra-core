using MCRA.Data.Management.Tests.UnitTests.DataManagement;
using MCRA.General;

namespace MCRA.Data.Management.Test.UnitTests.DataManagement {
    [TestClass]
    public class CompiledDoseResponseModelsTests : CompiledTestsBase {

        [TestMethod]
        [DataRow(ManagerType.CompiledDataManager)]
        [DataRow(ManagerType.SubsetManager)]
        public void CompiledDoseResponseModels_TestSimple(ManagerType managerType) {
            RawDataProvider.SetDataTables(
                (ScopingType.DoseResponseModels, @"DoseResponseTests/DoseResponseModelsSimple"),
                (ScopingType.Responses, @"DoseResponseTests/ResponsesSimple")
            );

            // Only experiments with all matching codes are loaded (matching response codes are mandatory)
            var models = GetAllDoseResponseModels(managerType);
            Assert.HasCount(2, models);

            // Substances are loaded from valid experiments, so only 4 in this case
            var substances = GetAllCompounds(managerType);
            Assert.HasCount(5, substances);

            var responses = GetAllResponses(managerType);
            Assert.HasCount(3, responses);
        }

        [TestMethod]
        [DataRow(ManagerType.CompiledDataManager)]
        [DataRow(ManagerType.SubsetManager)]
        public void CompiledDoseResponseModels_TestBenchmarkDoses(ManagerType managerType) {
            RawDataProvider.SetDataTables(
                (ScopingType.DoseResponseModels, @"DoseResponseTests/DoseResponseModelsSimple"),
                (ScopingType.DoseResponseModelBenchmarkDoses, @"DoseResponseTests/DoseResponseModelBenchmarkDosesSimple"),
                (ScopingType.DoseResponseModelBenchmarkDosesUncertain, @"DoseResponseTests/DoseResponseModelBenchmarkDosesUncertainSimple"),
                (ScopingType.Responses, @"DoseResponseTests/ResponsesSimple")
            );

            // Only experiments with all matching codes are loaded (matching response codes are mandatory)
            var models = GetAllDoseResponseModels(managerType);
            Assert.HasCount(2, models);

            // Check benchmark doses and benchmark dose uncertainty values
            Assert.AreEqual(7, models.SelectMany(r => r.DoseResponseModelBenchmarkDoses.Values).Count());

            var uncertaintySets = models.FirstOrDefault(r => r.IdDoseResponseModel == "model1").DoseResponseModelBenchmarkDoses.Values.SelectMany(r => r.DoseResponseModelBenchmarkDoseUncertains);
            Assert.AreEqual(12, uncertaintySets.Count());
            Assert.AreEqual(3, uncertaintySets.Select(r => r.IdUncertaintySet).Distinct().Count());

            // Substances are loaded from valid experiments, so only 4 in this case
            var substances = GetAllCompounds(managerType);
            Assert.HasCount(5, substances);

            var responses = GetAllResponses(managerType);
            Assert.HasCount(3, responses);
        }

        [TestMethod]
        [DataRow(ManagerType.CompiledDataManager)]
        [DataRow(ManagerType.SubsetManager)]
        public void CompiledDoseResponseModelsFilterSubstancesResponsesTest(ManagerType managerType) {
            RawDataProvider.SetDataTables(
                (ScopingType.DoseResponseModels, @"DoseResponseTests/DoseResponseModelsForFiltering"),
                (ScopingType.Responses, @"DoseResponseTests/ResponsesSimple")
            );

            RawDataProvider.SetFilterCodes(ScopingType.Compounds, ["A", "B"]);
            RawDataProvider.SetFilterCodes(ScopingType.Responses, ["R2"]);

            var models = GetAllDoseResponseModels(managerType);

            Assert.HasCount(6, models);

            CollectionAssert.AreEquivalent(
                new[] { "drm15", "drm16", "drm19", "drm22", "drm23", "drm26" },
                models.Select(m => m.IdDoseResponseModel.ToLower()).ToList()
            );
        }

        [TestMethod]
        [DataRow(ManagerType.CompiledDataManager)]
        [DataRow(ManagerType.SubsetManager)]
        public void CompiledDoseResponseModels_TestFilterSubstancesResponsesExperiments(ManagerType managerType) {
            RawDataProvider.SetEmptyDataSource(SourceTableGroup.Compounds);
            RawDataProvider.SetDataTables(
                (ScopingType.DoseResponseModels, @"DoseResponseTests/DoseResponseModelsForFiltering"),
                (ScopingType.Responses, @"DoseResponseTests/ResponsesSimple")
            );
            RawDataProvider.SetFilterCodes(ScopingType.Compounds, ["a", "b"]);
            RawDataProvider.SetFilterCodes(ScopingType.Responses, ["R2"]);
            RawDataProvider.SetFilterCodes(ScopingType.DoseResponseExperiments, ["Exp1"]);

            var models = GetAllDoseResponseModels(managerType);

            Assert.HasCount(3, models);

            CollectionAssert.AreEquivalent(
                new[] { "drm15", "drm16", "drm19" },
                models.Select(m => m.IdDoseResponseModel.ToLower()).ToList()
            );

            var substances = GetAllCompounds(managerType);
            Assert.HasCount(2, substances);

            CollectionAssert.AreEquivalent(
                new[] { "a", "b" },
                substances.Keys.ToList()
            );
        }
    }
}
