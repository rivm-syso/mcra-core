using MCRA.General.Action.Settings.Dto;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Data.Management.Test.UnitTests.DataManagement {
    /// <summary>
    /// Runs all tests for compiled non-dietary exposure sources when using CompiledDataManager.GetAllNonDietaryExposureSources
    /// to retrieve the non-dietary exposure sources
    /// </summary>
    [TestClass]
    public class CompiledNonDietaryExposureSourcesTests : CompiledNonDietaryExposureSourceTests {
        [TestInitialize]
        public override void TestInitialize() {
            base.TestInitialize();
            _getNonDietaryExposureSourcesDelegate = () => _compiledDataManager.GetAllNonDietaryExposureSources();
        }
    }

    /// <summary>
    /// Runs all tests for compiled non-dietary exposure sources when using 
    /// SubsetManager.AllNonDietaryExposureSources (cast to a dictionary by 
    /// code) to retrieve the sources
    /// </summary>
    [TestClass]
    public class SubsetManagerAllNonDietaryExposureSourcesTests : CompiledNonDietaryExposureSourceTests {
        protected SubsetManager _subsetManager;

        [TestInitialize]
        public override void TestInitialize() {
            base.TestInitialize();
            _subsetManager = new SubsetManager(_compiledDataManager, new ProjectDto());
            _getNonDietaryExposureSourcesDelegate = () => _subsetManager.AllNonDietaryExposureSources.ToDictionary(f => f.Code, StringComparer.OrdinalIgnoreCase);
        }
    }
}
