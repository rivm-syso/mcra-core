using MCRA.Data.Management.Tests.UnitTests.DataManagement;
using MCRA.General;

namespace MCRA.Data.Management.Test.UnitTests.DataManagement {
    [TestClass]
    public class CompiledEffectRepresentationsTests : CompiledTestsBase {
        [TestMethod]
        [DataRow(ManagerType.CompiledDataManager)]
        [DataRow(ManagerType.SubsetManager)]
        public void CompiledEffectRepresentations_TestOnly(ManagerType managerType) {
            RawDataProvider.SetDataTables(
                (ScopingType.EffectRepresentations, @"EffectRepresentationsTests/EffectRepresentationsSimple")
            );

            var representations = GetAllEffectRepresentations(managerType);

            Assert.IsEmpty(representations);
        }

        [TestMethod]
        [DataRow(ManagerType.CompiledDataManager)]
        [DataRow(ManagerType.SubsetManager)]
        public void CompiledEffectRepresentations_TestOnlyWithResponsesScope(ManagerType managerType) {
            RawDataProvider.SetEmptyDataSource(SourceTableGroup.Responses);
            RawDataProvider.SetDataTables(
                (ScopingType.EffectRepresentations, @"EffectRepresentationsTests/EffectRepresentationsSimple")
            );

            //set a filter scope on responses
            RawDataProvider.SetFilterCodes(ScopingType.Responses, ["R2"]);

            var representations = GetAllEffectRepresentations(managerType);

            Assert.IsEmpty(representations);
        }

        [TestMethod]
        [DataRow(ManagerType.CompiledDataManager)]
        [DataRow(ManagerType.SubsetManager)]
        public void CompiledEffectRepresentations_TestSimple(ManagerType managerType) {
            RawDataProvider.SetDataTables(
                (ScopingType.EffectRepresentations, @"EffectRepresentationsTests/EffectRepresentationsSimple"),
                (ScopingType.Responses, @"EffectRepresentationsTests/ResponsesSimple")
            );
            var representations = GetAllEffectRepresentations(managerType);

            Assert.HasCount(9, representations);

            var effects = representations.Select(r => r.Effect.Code).Distinct().ToList();
            CollectionAssert.AreEquivalent(new[] { "Eff1", "Eff2", "Eff3", "Eff4", "Eff5" }, effects);

            var responses = representations.Select(r => r.Response.Code).Distinct().ToList();
            CollectionAssert.AreEquivalent(new[] { "R1", "R2", "R3" }, responses);
        }

        [TestMethod]
        [DataRow(ManagerType.CompiledDataManager)]
        [DataRow(ManagerType.SubsetManager)]
        public void CompiledEffectRepresentations_TestFilterEffectsSimple(ManagerType managerType) {
            RawDataProvider.SetDataTables(
                (ScopingType.EffectRepresentations, @"EffectRepresentationsTests/EffectRepresentationsSimple"),
                (ScopingType.Responses, @"EffectRepresentationsTests/ResponsesSimple")
            );
            RawDataProvider.SetFilterCodes(ScopingType.Effects, ["Eff1"]);

            var representations = GetAllEffectRepresentations(managerType);

            Assert.HasCount(1, representations);

            var effects = representations.Select(r => r.Effect.Code).Distinct().ToList();
            CollectionAssert.AreEquivalent(new[] { "Eff1" }, effects);

            var responses = representations.Select(r => r.Response.Code).Distinct().ToList();
            CollectionAssert.AreEquivalent(new[] { "R1" }, responses);
        }

        [TestMethod]
        [DataRow(ManagerType.CompiledDataManager)]
        [DataRow(ManagerType.SubsetManager)]
        public void CompiledEffectRepresentations_FilterResponsesSimpleTest(ManagerType managerType) {
            RawDataProvider.SetDataTables(
                (ScopingType.EffectRepresentations, @"EffectRepresentationsTests/EffectRepresentationsSimple"),
                (ScopingType.Responses, @"EffectRepresentationsTests/ResponsesSimple")
            );
            RawDataProvider.SetFilterCodes(ScopingType.Responses, ["R2"]);

            var representations = GetAllEffectRepresentations(managerType);

            Assert.HasCount(3, representations);

            var effects = representations.Select(r => r.Effect.Code).Distinct().ToList();
            CollectionAssert.AreEquivalent(new[] { "Eff2", "Eff3", "Eff4" }, effects);

            var responses = representations.Select(r => r.Response.Code).Distinct().ToList();
            CollectionAssert.AreEquivalent(new[] { "R2" }, responses);
        }

        [TestMethod]
        [DataRow(ManagerType.CompiledDataManager)]
        [DataRow(ManagerType.SubsetManager)]
        public void CompiledEffectRepresentations_TestFilterTestsystemsSimple(ManagerType managerType) {
            RawDataProvider.SetDataTables(
                (ScopingType.EffectRepresentations, @"EffectRepresentationsTests/EffectRepresentationsSimple"),
                (ScopingType.Responses, @"EffectRepresentationsTests/ResponsesSimple"),
                (ScopingType.TestSystems, @"DoseResponseTests/TestSystemsSimple")
            );
            RawDataProvider.SetFilterCodes(ScopingType.TestSystems, ["sys1"]);

            var representations = GetAllEffectRepresentations(managerType);

            Assert.HasCount(8, representations);

            var effects = representations.Select(r => r.Effect.Code).Distinct().ToList();
            CollectionAssert.AreEquivalent(new[] { "Eff1", "Eff2", "Eff3", "Eff4", "Eff5" }, effects);

            var responses = representations.Select(r => r.Response.Code).Distinct().ToList();
            CollectionAssert.AreEquivalent(new[] { "R1", "R2" }, responses);
        }

        [TestMethod]
        [DataRow(ManagerType.CompiledDataManager)]
        [DataRow(ManagerType.SubsetManager)]
        public void CompiledEffectRepresentations_TestFilterAll(ManagerType managerType) {
            RawDataProvider.SetDataTables(
                (ScopingType.EffectRepresentations, @"EffectRepresentationsTests/EffectRepresentationsSimple"),
                (ScopingType.Responses, @"DoseResponseTests/ResponsesSimple"),
                (ScopingType.TestSystems, @"DoseResponseTests/TestSystemsSimple"),
                (ScopingType.Effects, @"EffectsTests/EffectsSimple")
            );
            RawDataProvider.SetFilterCodes(ScopingType.TestSystems, ["sys1"]);
            RawDataProvider.SetFilterCodes(ScopingType.Responses, ["R2"]);
            RawDataProvider.SetFilterCodes(ScopingType.Effects, ["Eff2", "Eff5"]);

            var representations = GetAllEffectRepresentations(managerType);

            Assert.HasCount(1, representations);

            var effects = representations.Select(r => r.Effect.Code).Distinct().ToList();
            CollectionAssert.AreEquivalent(new[] { "Eff2" }, effects);

            var responses = representations.Select(r => r.Response.Code).Distinct().ToList();
            CollectionAssert.AreEquivalent(new[] { "R2" }, responses);
        }
    }
}
