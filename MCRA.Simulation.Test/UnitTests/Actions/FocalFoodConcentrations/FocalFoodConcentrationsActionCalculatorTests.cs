using MCRA.Data.Management;
using MCRA.Data.Management.CompiledDataManagers;
using MCRA.Data.Management.RawDataProviders;
using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Actions.FocalFoodConcentrations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.Actions {
    /// <summary>
    /// Runs the FocalFoodConcentrations action.
    /// </summary>
    [TestClass]
    public class FocalFoodConcentrationsActionCalculatorTests : ActionCalculatorTestsBase {

        /// <summary>
        /// Runs the FocalFoods action: load data and summarize action result method.
        /// </summary>
        [TestMethod]
        public void FocalFoodConcentrationsActionCalculator_TestLoadAndSummarize() {
            var project = new ProjectDto();
            project.GetModuleConfiguration<FocalFoodConcentrationsModuleConfig>().FocalFoods.Add(new() { CodeFood = "APPLE" });

            var rawDataProvider = new CsvRawDataProvider(@"Resources\Csv\");
            rawDataProvider.SetDataGroupsFromFolder(1, "_DataGroupsTest", SourceTableGroup.Foods, SourceTableGroup.Compounds, SourceTableGroup.Concentrations);
            rawDataProvider.SetEmptyDataSource(1, SourceTableGroup.Foods, SourceTableGroup.Compounds, SourceTableGroup.FocalFoods);

            var compiledDataManager = new CompiledDataManager(rawDataProvider);

            var data = new ActionData() {
                AllFoods = compiledDataManager.GetAllFoods().Values,
                AllCompounds = compiledDataManager.GetAllCompounds().Values
            };

            var subsetManager = new SubsetManager(compiledDataManager, project);
            var calculator = new FocalFoodConcentrationsActionCalculator(project);
            var header = TestLoadAndSummarizeNominal(calculator, data, subsetManager, "TestLoadAndSummarize");

            Assert.AreEqual(5, data.FocalCommoditySamples.Count);

            WriteReport(header, "TestLoadAndSummarize.html");
        }

        /// <summary>
        /// Runs the FocalFoodConcentrations action: load data and summarize action result method.
        /// </summary>
        [TestMethod]
        public void FocalFoodConcentrationsActionCalculator_TestLoadAndSummarizeNoFocalCommoditySelection() {
            var project = new ProjectDto();

            var rawDataProvider = new CsvRawDataProvider(@"Resources\Csv\");
            rawDataProvider.SetDataGroupsFromFolder(1, "_DataGroupsTest", SourceTableGroup.Foods, SourceTableGroup.Compounds, SourceTableGroup.Concentrations);
            rawDataProvider.SetEmptyDataSource(1, SourceTableGroup.Foods, SourceTableGroup.Compounds, SourceTableGroup.FocalFoods);
            rawDataProvider.SetFilterCodes(ScopingType.Compounds, new[] { "CompoundA" });

            var compiledDataManager = new CompiledDataManager(rawDataProvider);

            var data = new ActionData() {
                AllCompounds = compiledDataManager.GetAllCompounds().Values
            };

            var subsetManager = new SubsetManager(compiledDataManager, project);
            var calculator = new FocalFoodConcentrationsActionCalculator(project);
            var header = TestLoadAndSummarizeNominal(calculator, data, subsetManager, "TestLoadAndSummarizeNoFocalCommoditySelection");

            Assert.AreEqual(0, data.FocalCommoditySamples.Count);

            WriteReport(header, "TestLoadAndSummarizeNoFocalCommoditySelection.html");
        }
    }
}