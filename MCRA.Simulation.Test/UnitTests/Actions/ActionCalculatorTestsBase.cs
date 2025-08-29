using MCRA.Data.Management;
using MCRA.Data.Management.RawDataWriters;
using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.General.SettingsDefinitions;
using MCRA.Simulation.Action;
using MCRA.Simulation.Action.UncertaintyFactorial;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.OutputGeneration.Helpers;
using MCRA.Simulation.OutputManagement;
using MCRA.Simulation.Test.Helpers;
using MCRA.Simulation.Test.Mock.MockActionData;
using MCRA.Simulation.Test.Mock.MockProject;
using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Test.UnitTests.Actions {

    /// <summary>
    /// Runs the ActionRunner
    /// </summary>
    [TestClass]
    public abstract class ActionCalculatorTestsBase {

        protected static string _reportOutputPath = Path.Combine(TestUtilities.TestOutputPath, "SummaryReports");

        /// <summary>
        /// Loads the action data, summarizes the result, and writes the generated
        /// report section to a file.
        /// </summary>
        /// <param name="calculator"></param>
        /// <param name="data"></param>
        /// <param name="subsetManager"></param>
        /// <param name="reportFileName"></param>
        /// <returns></returns>
        protected SummaryToc TestLoadAndSummarizeNominal(
            IActionCalculator calculator,
            ActionData data,
            SubsetManager subsetManager,
            string reportFileName
        ) {
            var mockProject = new MockProject(subsetManager.Project);
            //reset mockedSettings before run
            mockProject.ClearInvocations();

            var loadDataProgress = new CompositeProgressState();
            calculator.LoadData(data, subsetManager, loadDataProgress);
            Assert.AreEqual(100, loadDataProgress.Progress, 1e-2);

            var usedSettingsList = mockProject.AllInvocations;
            var header = new SummaryToc(new InMemorySectionManager());
            calculator.SummarizeActionResult(null, data, header, 0, new CompositeProgressState());
            mockProject.ClearInvocations();

            //run settings summarizer
            var settingsSummary = calculator.SummarizeSettings();

            // Collect invoked settings
            var invokedProjectSettingsList = mockProject.AllInvocations;

            // Collect summarized settings items
            var summarizedSettings = settingsSummary.GetSettingsSummaryRecordsRecursive()
                .Where(r => r is ActionSettingSummaryRecord)
                .Cast<ActionSettingSummaryRecord>()
                .Select(r => r.SettingsItemType)
                .Where(r => r != SettingsItemType.Undefined)
                .ToList();

            // Checklists for debug purposes
            var usedSettingsNotSummarized = usedSettingsList.Where(s => !invokedProjectSettingsList.Contains(s)).ToList();
            var summarizedSettingsNotUsed = invokedProjectSettingsList.Where(s => !usedSettingsList.Contains(s)).ToList();

            Assert.AreEqual(0, usedSettingsNotSummarized.Count + summarizedSettingsNotUsed.Count,
                $"\r\nUsed not summarized:\r\n-{string.Join("\r\n-", usedSettingsNotSummarized)}" +
                $"\r\nSummarized not used:\r\n+{string.Join("\r\n+", summarizedSettingsNotUsed)}");

            // Check for missing module settings
            var moduleSettings = calculator.ModuleDefinition.AllModuleSettings;
            var undefinedModuleSettings = summarizedSettings.Where(r => !moduleSettings.Contains(r)).ToList();
            Assert.IsTrue(!undefinedModuleSettings.Any(), $"The following settings were not defined in the module definition:\r\n-{string.Join("\r\n-", undefinedModuleSettings)}");

            if (!string.IsNullOrEmpty(reportFileName)) {
                WriteOutput(calculator, data, null, Path.GetFileNameWithoutExtension(reportFileName));
                WriteReport(header, reportFileName);
            }
            return header;
        }

        /// <summary>
        /// Runs the load and summarize uncertain methods of the action calculator and writes
        /// the report in case a report filename is specified.
        /// </summary>
        /// <param name="calculator"></param>
        /// <param name="data"></param>
        /// <param name="header"></param>
        /// <param name="factorialSet"></param>
        /// <param name="uncertaintySources"></param>
        /// <param name="reportFileName"></param>
        /// <param name="random"></param>
        protected void TestLoadAndSummarizeUncertainty(
            IActionCalculator calculator,
            ActionData data,
            SummaryToc header,
            IRandom random,
            UncertaintyFactorialSet factorialSet = null,
            Dictionary<UncertaintySource, IRandom> uncertaintySources = null,
            string reportFileName = null
        ) {
            uncertaintySources = uncertaintySources ?? createUncertaintySourceGenerators(random);
            factorialSet = factorialSet ?? new UncertaintyFactorialSet(uncertaintySources.Keys.ToArray());
            data = data.Copy();
            calculator.ResultsSummarized = false;

            // Load data uncertain
            var loadDataProgress = new CompositeProgressState();
            calculator.LoadDataUncertain(data, factorialSet, uncertaintySources, loadDataProgress);
            Assert.AreEqual(100, loadDataProgress.Progress, 1e-2);

            // Summarize uncertain
            var summarizeProgress = new CompositeProgressState();
            calculator.SummarizeActionResultUncertain(factorialSet, null, data, header, summarizeProgress);
            Assert.AreEqual(100, summarizeProgress.Progress, 1e-2);

            if (!string.IsNullOrEmpty(reportFileName)) {
                WriteOutput(calculator, data, null, Path.GetFileNameWithoutExtension(reportFileName));
                WriteReport(header, reportFileName);
            }
        }

        /// <summary>
        /// Runs the action, updates the simulation data, summarizes the result,
        /// and writes the generated report section to a file.
        /// </summary>
        /// <param name="project"></param>
        /// <param name="calculator"></param>
        /// <param name="data"></param>
        /// <param name="reportFileName"></param>
        /// <returns></returns>
        protected (SummaryToc, IActionResult) TestRunUpdateSummarizeNominal(
            ProjectDto project,
            IActionCalculator calculator,
            ActionData data,
            string reportFileName
        ) {
            var mockProject = new MockProject(project);
            var mockActionData = new MockActionData(data);

            //reset mockedSettings before run
            mockProject.ClearInvocations();

            var calculationProgress = new CompositeProgressState();
            var result = calculator.Run(mockActionData, calculationProgress);

            // Assert result is available
            Assert.IsNotNull(result);

            // Assert calculation progress is 100%
            Assert.AreEqual(100, calculationProgress.Progress, 1e-2);

            // Tablu list of not-yet resolved issues (this list should only get shorter!!!)
            var tabuList = new HashSet<(ActionType, ActionType)> {
                (ActionType.DietaryExposures, ActionType.Concentrations),
                (ActionType.DietaryExposures, ActionType.ModelledFoods),
                (ActionType.ExposureMixtures, ActionType.ActiveSubstances),
                (ActionType.OccurrenceFrequencies, ActionType.Concentrations),
                (ActionType.Risks, ActionType.ActiveSubstances),
                (ActionType.Risks, ActionType.ModelledFoods),
                (ActionType.SingleValueConsumptions, ActionType.DietaryExposures),
                (ActionType.SingleValueDietaryExposures, ActionType.ActiveSubstances),
                (ActionType.SingleValueRisks, ActionType.ActiveSubstances),
                (ActionType.SingleValueRisks, ActionType.RelativePotencyFactors),
                (ActionType.SingleValueRisks, ActionType.FocalFoodConcentrations),
                (ActionType.SingleValueRisks, ActionType.SubstanceConversions),
                (ActionType.SingleValueRisks, ActionType.DeterministicSubstanceConversionFactors),
                (ActionType.SingleValueRisks, ActionType.DietaryExposures),
                (ActionType.TargetExposures, ActionType.Concentrations),
                (ActionType.HazardCharacterisations, ActionType.DietaryExposures)
            };

            var usedInputModules = mockActionData.Modules;
            var specifiedInputModules = calculator.InputActionTypes.Select(r => r.ActionType).ToList();
            var missingInputRequirements = usedInputModules
                .Where(r => !tabuList.Contains((calculator.ActionType, r)))
                .Except(specifiedInputModules)
                .Except([calculator.ActionType]);
            var missingModulesString = string.Join(", ", missingInputRequirements.Select(r => r.ToString()));

            // TODO: the following assert will reveal a lot of inconsistencies;
            // uncomment this assertion once these inconsistencies have been addressed
            Assert.AreEqual(0, missingInputRequirements.Count(), $"Calculator uses data of modules {missingModulesString} while this is not specified in the module definition.");

            var header = new SummaryToc(new InMemorySectionManager());
            calculator.UpdateSimulationData(mockActionData, result);

            var summarizeProgress = new CompositeProgressState();
            calculator.SummarizeActionResult(result, mockActionData, header, 0, summarizeProgress);

            // Assert summarize progress is 100%
            Assert.AreEqual(100, summarizeProgress.Progress, 1e-2);

            var usedSettingsList = mockProject.AllInvocations;

            mockProject.ClearInvocations();
            //run settings summarizer and check
            _ = calculator.SummarizeSettings();

            var summarizedSettingsList = mockProject.AllInvocations;

            //checklists for debug purposes
            var usedSettingsNotSummarized = usedSettingsList.Where(s => !summarizedSettingsList.Contains(s)).ToList();
            var summarizedSettingsNotUsed = summarizedSettingsList.Where(s => !usedSettingsList.Contains(s)).ToList();

            Assert.AreEqual(0, usedSettingsNotSummarized.Count + summarizedSettingsNotUsed.Count,
                $"\r\nUsed not summarized:\r\n-{string.Join("\r\n-", usedSettingsNotSummarized)}" +
                $"\r\nSummarized not used:\r\n+{string.Join("\r\n+", summarizedSettingsNotUsed)}");

            if (!string.IsNullOrEmpty(reportFileName)) {
                WriteOutput(calculator, data, result, Path.GetFileNameWithoutExtension(reportFileName));
                WriteReport(header, reportFileName);
            }
            return (header, result);
        }

        /// <summary>
        /// Note properties: ResultsSummarized = false, SimulationDataUpdated = false
        /// </summary>
        /// <param name="calculator"></param>
        /// <param name="data"></param>
        /// <param name="header"></param>
        /// <param name="random"></param>
        /// <param name="factorialSet"></param>
        /// <param name="uncertaintySources"></param>
        /// <param name="idBootstrap"></param>
        /// <param name="reportFileName"></param>
        protected void TestRunUpdateSummarizeUncertainty(
            IActionCalculator calculator,
            ActionData data,
            SectionHeader header,
            IRandom random,
            UncertaintyFactorialSet factorialSet = null,
            Dictionary<UncertaintySource, IRandom> uncertaintySources = null,
            int idBootstrap = 1,
            string reportFileName = null
        ) {
            uncertaintySources = uncertaintySources ?? createUncertaintySourceGenerators(random);
            factorialSet = factorialSet ?? new UncertaintyFactorialSet(uncertaintySources.Keys.ToArray());
            calculator.ResultsComputed = false;

            var runUncertainProgress = new CompositeProgressState();
            var result = calculator.RunUncertain(data, factorialSet, uncertaintySources, header, runUncertainProgress);
            Assert.AreEqual(100, runUncertainProgress.Progress, 1e-2);

            calculator.ResultsSummarized = false;
            var summarizeUnccertainProgress = new CompositeProgressState();
            calculator.SummarizeActionResultUncertain(factorialSet, result, data, header, summarizeUnccertainProgress);
            Assert.AreEqual(100, summarizeUnccertainProgress.Progress, 1e-2);

            calculator.SimulationDataUpdated = false;
            calculator.UpdateSimulationDataUncertain(data, result);
            Assert.IsNotNull(result);
            if (!string.IsNullOrEmpty(reportFileName)) {
                WriteOutput(
                    calculator,
                    data,
                    result,
                    Path.GetFileNameWithoutExtension(reportFileName)
                );
                WriteReport((SummaryToc)header, reportFileName);
            }
        }

        protected void WriteReport(SummaryToc toc, string filename) {
            var reportBuilder = new ReportBuilder(toc);
            var html = reportBuilder.RenderReport(null, false, null);
            if (!string.IsNullOrEmpty(filename)) {
                filename = Path.HasExtension(filename) ? filename : $"{filename}.html";
                var outputPath = Path.Combine(_reportOutputPath, GetType().Name);
                if (!Directory.Exists(outputPath)) {
                    Directory.CreateDirectory(outputPath);
                }
                File.WriteAllText(Path.Combine(outputPath, filename), html);
            }
        }

        protected void WriteOutput(
            IActionCalculator calculator,
            ActionData data,
            IActionResult result,
            string testName
        ) {
            var outputPath = Path.Combine(_reportOutputPath, GetType().Name, $"{testName}-OutputData");
            var outputWriter = new CsvRawDataWriter(outputPath);
            calculator.WriteOutputData(outputWriter, data, result);
            outputWriter.Store();
        }

        Dictionary<UncertaintySource, IRandom> createUncertaintySourceGenerators(
            IRandom random,
            UncertaintySource[] uncertaintySources = null
        ) {
            uncertaintySources = uncertaintySources ?? Enum.GetValues(typeof(UncertaintySource)).Cast<UncertaintySource>().ToArray();
            var uncertaintySourceGenerators = uncertaintySources.ToDictionary(r => r, r => random);
            return uncertaintySourceGenerators;
        }
    }
}
