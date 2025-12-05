using MCRA.Data.Compiled.Objects;
using MCRA.General;

namespace MCRA.Data.Management.Test.UnitTests.DataManagement {
    public class CompiledDoseResponseModelsTests : CompiledTestsBase {
        protected Func<IList<DoseResponseModel>> _getResponseModelsDelegate;
        protected Func<IDictionary<string, Response>> _getResponsesDelegate;
        protected Func<IDictionary<string, Compound>> _getSubstancesDelegate;

        [TestMethod]
        public void CompiledDoseResponseModels_TestSimple() {
            _rawDataProvider.SetDataTables(
                (ScopingType.DoseResponseModels, @"DoseResponseTests/DoseResponseModelsSimple"),
                (ScopingType.Responses, @"DoseResponseTests/ResponsesSimple")
            );

            // Only experiments with all matching codes are loaded (matching response codes are mandatory)
            var models = _getResponseModelsDelegate.Invoke();
            Assert.HasCount(2, models);

            // Substances are loaded from valid experiments, so only 4 in this case
            var substances = _getSubstancesDelegate.Invoke();
            Assert.HasCount(5, substances);

            var responses = _getResponsesDelegate.Invoke();
            Assert.HasCount(3, responses);
        }

        [TestMethod]
        public void CompiledDoseResponseModels_TestBenchmarkDoses() {
            _rawDataProvider.SetDataTables(
                (ScopingType.DoseResponseModels, @"DoseResponseTests/DoseResponseModelsSimple"),
                (ScopingType.DoseResponseModelBenchmarkDoses, @"DoseResponseTests/DoseResponseModelBenchmarkDosesSimple"),
                (ScopingType.DoseResponseModelBenchmarkDosesUncertain, @"DoseResponseTests/DoseResponseModelBenchmarkDosesUncertainSimple"),
                (ScopingType.Responses, @"DoseResponseTests/ResponsesSimple")
            );

            // Only experiments with all matching codes are loaded (matching response codes are mandatory)
            var models = _getResponseModelsDelegate.Invoke();
            Assert.HasCount(2, models);

            // Check benchmark doses and benchmark dose uncertainty values
            Assert.AreEqual(7, models.SelectMany(r => r.DoseResponseModelBenchmarkDoses.Values).Count());

            var uncertaintySets = models.FirstOrDefault(r => r.IdDoseResponseModel == "model1").DoseResponseModelBenchmarkDoses.Values.SelectMany(r => r.DoseResponseModelBenchmarkDoseUncertains);
            Assert.AreEqual(12, uncertaintySets.Count());
            Assert.AreEqual(3, uncertaintySets.Select(r => r.IdUncertaintySet).Distinct().Count());

            // Substances are loaded from valid experiments, so only 4 in this case
            var substances = _getSubstancesDelegate.Invoke();
            Assert.HasCount(5, substances);

            var responses = _getResponsesDelegate.Invoke();
            Assert.HasCount(3, responses);
        }

        [TestMethod]
        public void CompiledDoseResponseModelsFilterSubstancesResponsesTest() {
            _rawDataProvider.SetDataTables(
                (ScopingType.DoseResponseModels, @"DoseResponseTests/DoseResponseModelsForFiltering"),
                (ScopingType.Responses, @"DoseResponseTests/ResponsesSimple")
            );

            _rawDataProvider.SetFilterCodes(ScopingType.Compounds, ["A", "B"]);
            _rawDataProvider.SetFilterCodes(ScopingType.Responses, ["R2"]);

            var models = _getResponseModelsDelegate.Invoke();

            Assert.HasCount(6, models);

            CollectionAssert.AreEquivalent(
                new[] { "drm15", "drm16", "drm19", "drm22", "drm23", "drm26" },
                models.Select(m => m.IdDoseResponseModel.ToLower()).ToList()
            );
        }

        [TestMethod]
        public void CompiledDoseResponseModels_TestFilterSubstancesResponsesExperiments() {
            _rawDataProvider.SetEmptyDataSource(SourceTableGroup.Compounds);
            _rawDataProvider.SetDataTables(
                (ScopingType.DoseResponseModels, @"DoseResponseTests/DoseResponseModelsForFiltering"),
                (ScopingType.Responses, @"DoseResponseTests/ResponsesSimple")
            );
            _rawDataProvider.SetFilterCodes(ScopingType.Compounds, ["a", "b"]);
            _rawDataProvider.SetFilterCodes(ScopingType.Responses, ["R2"]);
            _rawDataProvider.SetFilterCodes(ScopingType.DoseResponseExperiments, ["Exp1"]);

            var models = _getResponseModelsDelegate.Invoke();

            Assert.HasCount(3, models);

            CollectionAssert.AreEquivalent(
                new[] { "drm15", "drm16", "drm19" },
                models.Select(m => m.IdDoseResponseModel.ToLower()).ToList()
            );

            var substances = _getSubstancesDelegate.Invoke();
            Assert.HasCount(2, substances);

            CollectionAssert.AreEquivalent(
                new[] { "a", "b" },
                substances.Keys.ToList()
            );
        }
    }
}
