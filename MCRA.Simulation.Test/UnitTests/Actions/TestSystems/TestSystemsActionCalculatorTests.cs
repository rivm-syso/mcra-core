using MCRA.Data.Compiled;
using MCRA.Data.Compiled.Objects;
using MCRA.Data.Management;
using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Actions.TestSystems;
using MCRA.Simulation.Test.Mock;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.Actions {
    /// <summary>
    /// Runs the TestSystems action
    /// </summary>
    [TestClass]
    public class TestSystemsActionCalculatorTests : ActionCalculatorTestsBase {

        /// <summary>
        /// Runs the TestSystems action: load data and summarize method
        /// </summary>
        [TestMethod]
        public void TestSystemsActionCalculator_Test() {
            var testSystems = new List<TestSystem>() {
                new() {
                    Code = "system",
                    Organ = "Liver",
                    Description= "Description",
                    ExposureRoute = ExposureRoute.Undefined,
                    GuidelineStudy = "GuidelineStudy",
                    Name = "Name",
                    Reference = "Reference",
                    Species = "Species",
                    Strain = "Strain",
                    TestSystemType = TestSystemType.CellLine
                }
            };
            var compiledData = new CompiledData() {
                AllTestSystems = testSystems.ToDictionary(c => c.Code),
            };

            var dataManager = new MockCompiledDataManager(compiledData);
            var project = new ProjectDto();
            var subsetManager = new SubsetManager(dataManager, project);
            var data = new ActionData();
            var calculator = new TestSystemsActionCalculator(project);
            TestLoadAndSummarizeNominal(calculator, data, subsetManager, "TestLoad");
            Assert.IsNotNull(data.TestSystems);
        }
    }
}