using MCRA.Data.Management.Tests.UnitTests.DataManagement;
using MCRA.General;

namespace MCRA.Data.Management.Test.UnitTests.DataManagement {
    [TestClass]
    public class CompiledMolecularDockingModelsTests : CompiledTestsBase {

        [TestMethod]
        [DataRow(ManagerType.CompiledDataManager)]
        [DataRow(ManagerType.SubsetManager)]
        public void CompiledMolecularDockingModelsSimpleTest(ManagerType managerType) {
            RawDataProvider.SetDataTables(
                (ScopingType.MolecularDockingModels, @"MolecularDockingModelsTests/MolecularDockingModelsSimple")
            );

            var allDockingModels = GetAllMolecularDockingModels(managerType);

            CollectionAssert.AreEqual(new[] { "MD1", "MD2", "MD3" }, allDockingModels.Keys.ToList());
        }


        [TestMethod]
        [DataRow(ManagerType.CompiledDataManager)]
        [DataRow(ManagerType.SubsetManager)]
        public void CompiledMolecularDockingModelsSimpleEffectsFilterTest(ManagerType managerType) {
            RawDataProvider.SetDataTables(
                (ScopingType.MolecularDockingModels, @"MolecularDockingModelsTests/MolecularDockingModelsSimple")
            );
            RawDataProvider.SetFilterCodes(ScopingType.Effects, ["Eff1"]);

            var allDockingModels = CompiledDataManager.GetAllMolecularDockingModels();

            CollectionAssert.AreEqual(new[] { "MD1", "MD3" }, allDockingModels.Keys.ToList());

        }

        [TestMethod]
        [DataRow(ManagerType.CompiledDataManager)]
        [DataRow(ManagerType.SubsetManager)]
        public void CompiledMolecularBindingEnergiesSimpleTest(ManagerType managerType) {
            RawDataProvider.SetDataTables(
                (ScopingType.MolecularDockingModels, @"MolecularDockingModelsTests/MolecularDockingModelsSimple"),
                (ScopingType.MolecularBindingEnergies, @"MolecularDockingModelsTests/MolecularBindingEnergiesSimple")
            );

            var allDockingModels = CompiledDataManager.GetAllMolecularDockingModels();

            CollectionAssert.AreEqual(new[] { "MD1", "MD2", "MD3" }, allDockingModels.Keys.ToList());

            var compoundCodes = allDockingModels["MD1"].BindingEnergies.Keys.Select(c => c.Code).ToList();
            CollectionAssert.AreEquivalent(new[] { "A", "B", "C" }, compoundCodes);

            compoundCodes = allDockingModels["MD2"].BindingEnergies.Keys.Select(c => c.Code).ToList();
            CollectionAssert.AreEquivalent(new[] { "A", "D" }, compoundCodes);

            compoundCodes = allDockingModels["MD3"].BindingEnergies.Keys.Select(c => c.Code).ToList();
            CollectionAssert.AreEquivalent(new[] { "B", "C" }, compoundCodes);
        }

        [TestMethod]
        [DataRow(ManagerType.CompiledDataManager)]
        [DataRow(ManagerType.SubsetManager)]
        public void CompiledMolecularBindingEnergiesFilterEffectsAndCompoundsSimpleTest(ManagerType managerType) {
            RawDataProvider.SetDataTables(
                (ScopingType.MolecularDockingModels, @"MolecularDockingModelsTests/MolecularDockingModelsSimple"),
                (ScopingType.MolecularBindingEnergies, @"MolecularDockingModelsTests/MolecularBindingEnergiesSimple")
            );
            RawDataProvider.SetFilterCodes(ScopingType.Effects, ["Eff1"]);
            RawDataProvider.SetFilterCodes(ScopingType.Compounds, ["A", "B", "D"]);

            var allDockingModels = CompiledDataManager.GetAllMolecularDockingModels();

            CollectionAssert.AreEqual(new[] { "MD1", "MD3" }, allDockingModels.Keys.ToList());

            var compoundCodes = allDockingModels["MD1"].BindingEnergies.Keys.Select(c => c.Code).ToList();
            CollectionAssert.AreEquivalent(new[] { "A", "B" }, compoundCodes);

            compoundCodes = allDockingModels["MD3"].BindingEnergies.Keys.Select(c => c.Code).ToList();
            CollectionAssert.AreEquivalent(new[] { "B" }, compoundCodes);
        }
    }
}
