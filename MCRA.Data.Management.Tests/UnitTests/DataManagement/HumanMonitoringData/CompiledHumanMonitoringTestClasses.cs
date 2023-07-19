using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Data.Management.Test.UnitTests.DataManagement {
    /// <summary>
    /// Runs all tests for compiled PointsOfDeparture when using CompiledDataManager.GetAllPointOfDeparture
    /// to retrieve the PointsOfDeparture
    /// </summary>
    [TestClass]
    public class CompiledDataManagerHumanMonitoringTests : CompiledHumanMonitoringTests {
        [TestInitialize]
        public override void TestInitialize() {
            base.TestInitialize();
            _getAnalyticalMethodsDelegate = () => _compiledDataManager.GetAllHumanMonitoringAnalyticalMethods();
            _getIndividualsDelegate = () => _compiledDataManager.GetAllHumanMonitoringIndividuals();
            _getSurveysDelegate = () => _compiledDataManager.GetAllHumanMonitoringSurveys();
            _getSamplesDelegate = () => _compiledDataManager.GetAllHumanMonitoringSamples();
        }
    }

    /// <summary>
    /// Runs all tests for PointsOfDeparture when using SubsetManager.AllPointsOfDeparture
    /// to retrieve the PointsOfDeparture
    /// </summary>
    [TestClass]
    public class SubsetManagerHumanMonitoringTests : CompiledHumanMonitoringTests {
        protected SubsetManager _subsetManager;

        [TestInitialize]
        public override void TestInitialize() {
            base.TestInitialize();
            _subsetManager = new SubsetManager(_compiledDataManager, new General.Action.Settings.ProjectDto());
            _getAnalyticalMethodsDelegate = () => _subsetManager.AllHumanMonitoringAnalyticalMethods;
            _getIndividualsDelegate = () => _subsetManager.AllHumanMonitoringIndividuals.ToDictionary(s => s.Code);
            _getSurveysDelegate = () => _subsetManager.AllHumanMonitoringSurveys.ToDictionary(s => s.Code);
            _getSamplesDelegate = () => _subsetManager.AllHumanMonitoringSamples.ToDictionary(s => s.Code);
        }
    }
}
