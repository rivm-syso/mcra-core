using MCRA.Data.Compiled.Objects;
using MCRA.General;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Data.Management.Test.UnitTests.DataManagement {
    public class CompiledMolecularDockingModelsTests : CompiledTestsBase {

        protected Func<IDictionary<string, MolecularDockingModel>> _getItemsDelegate;

        [TestMethod]
        public void CompiledMolecularDockingModelsSimpleTest() {
            _rawDataProvider.SetDataTables(
                (ScopingType.MolecularDockingModels, @"MolecularDockingModelsTests\MolecularDockingModelsSimple")
            );

            var allDockingModels = _getItemsDelegate.Invoke();

            CollectionAssert.AreEqual(new[] { "MD1", "MD2", "MD3" }, allDockingModels.Keys.ToList());
        }


        [TestMethod]
        public void CompiledMolecularDockingModelsSimpleEffectsFilterTest() {
            _rawDataProvider.SetDataTables(
                (ScopingType.MolecularDockingModels, @"MolecularDockingModelsTests\MolecularDockingModelsSimple")
            );
            _rawDataProvider.SetFilterCodes(ScopingType.Effects, new[] { "Eff1" });

            var allDockingModels = _compiledDataManager.GetAllMolecularDockingModels();

            CollectionAssert.AreEqual(new[] { "MD1", "MD3" }, allDockingModels.Keys.ToList());

        }

        [TestMethod]
        public void CompiledMolecularBindingEnergiesSimpleTest() {
            _rawDataProvider.SetDataTables(
                (ScopingType.MolecularDockingModels, @"MolecularDockingModelsTests\MolecularDockingModelsSimple"),
                (ScopingType.MolecularBindingEnergies, @"MolecularDockingModelsTests\MolecularBindingEnergiesSimple")
            );

            var allDockingModels = _compiledDataManager.GetAllMolecularDockingModels();

            CollectionAssert.AreEqual(new[] { "MD1", "MD2", "MD3" }, allDockingModels.Keys.ToList());

            var compoundCodes = allDockingModels["MD1"].BindingEnergies.Keys.Select(c => c.Code).ToList();
            CollectionAssert.AreEquivalent(new[] { "A", "B", "C" }, compoundCodes);

            compoundCodes = allDockingModels["MD2"].BindingEnergies.Keys.Select(c => c.Code).ToList();
            CollectionAssert.AreEquivalent(new[] { "A", "D" }, compoundCodes);

            compoundCodes = allDockingModels["MD3"].BindingEnergies.Keys.Select(c => c.Code).ToList();
            CollectionAssert.AreEquivalent(new[] { "B", "C" }, compoundCodes);
        }

        [TestMethod]
        public void CompiledMolecularBindingEnergiesFilterEffectsAndCompoundsSimpleTest() {
            _rawDataProvider.SetDataTables(
                (ScopingType.MolecularDockingModels, @"MolecularDockingModelsTests\MolecularDockingModelsSimple"),
                (ScopingType.MolecularBindingEnergies, @"MolecularDockingModelsTests\MolecularBindingEnergiesSimple")
            );
            _rawDataProvider.SetFilterCodes(ScopingType.Effects, new[] { "Eff1" });
            _rawDataProvider.SetFilterCodes(ScopingType.Compounds, new[] { "A", "B", "D" });

            var allDockingModels = _compiledDataManager.GetAllMolecularDockingModels();

            CollectionAssert.AreEqual(new[] { "MD1", "MD3" }, allDockingModels.Keys.ToList());

            var compoundCodes = allDockingModels["MD1"].BindingEnergies.Keys.Select(c => c.Code).ToList();
            CollectionAssert.AreEquivalent(new[] { "A", "B" }, compoundCodes);

            compoundCodes = allDockingModels["MD3"].BindingEnergies.Keys.Select(c => c.Code).ToList();
            CollectionAssert.AreEquivalent(new[] { "B" }, compoundCodes);
        }
    }
}
