using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace MCRA.Data.Management.Test.UnitTests.DataManagement {
    /// <summary>
    /// Runs all tests for compiled PointsOfDeparture when using CompiledDataManager.GetAllPointOfDeparture
    /// to retrieve the PointsOfDeparture
    /// </summary>
    [TestClass]
    public class CompiledDataManagerMolecularDockingModelsTests: CompiledMolecularDockingModelsTests {
        [TestInitialize]
        public override void TestInitialize() {
            base.TestInitialize();
            _getItemsDelegate = () => _compiledDataManager.GetAllMolecularDockingModels();
        }
    }

    /// <summary>
    /// Runs all tests for PointsOfDeparture when using SubsetManager.AllPointsOfDeparture
    /// to retrieve the PointsOfDeparture
    /// </summary>
    [TestClass]
    public class SubsetManagerMolecularDockingModelsTests : CompiledMolecularDockingModelsTests {
        protected SubsetManager _subsetManager;

        [TestInitialize]
        public override void TestInitialize() {
            base.TestInitialize();
            _subsetManager = new SubsetManager(_compiledDataManager, new General.Action.Settings.Dto.ProjectDto());
            _getItemsDelegate = () => _subsetManager.AllMolecularDockingModels.ToDictionary(m => m.Code, StringComparer.OrdinalIgnoreCase);
        }
    }
}
