using System.Globalization;
using MCRA.Data.Management;
using MCRA.General;
using MCRA.General.Action.Serialization;
using MCRA.Simulation.Action;
using MCRA.Simulation.Actions;
using MCRA.Simulation.Actions.ActionComparison;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.OutputManagement;
using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Xml;

namespace MCRA.Simulation.TaskExecution.TaskExecuters {
    public class LoopCalculationTaskExecuter : TaskExecuterBase {

        private readonly IOutputManager _outputManager;
        private readonly ITaskLoader _taskLoader;

        public LoopCalculationTaskExecuter(
            ITaskLoader loopTaskLoader,
            IOutputManager outputManager,
             string log4netConfigFile)
            : base(log4netConfigFile) {
            _taskLoader = loopTaskLoader;
            _outputManager = outputManager;
        }

        /// <summary>
        /// Creates a combined output report for the sub-tasks of the specified
        /// multi-task.
        /// </summary>
        /// <param name="task"></param>
        /// <param name="progressReport"></param>
        /// <returns></returns>
        public override TaskExecutionResult Run(
            ITask task,
            ProgressReport progressReport
        ) {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

            var localProgress = progressReport.NewProgressState(100);
            localProgress.Update("Starting combined report task");

            localProgress.Update("Collecting outputs", 2);
            var subTasksOutputs = _taskLoader.CollectSubTaskOutputs(task);

            // Get the (multi-)task (including authorisation checks)
            if (subTasksOutputs?.Any() ?? false) {

                localProgress.Update("Creating combined output calculator", 5);

                // Collect one or more different action results, for each output
                var collectedComparisons = new Dictionary<ActionType, Dictionary<IOutput, IActionComparisonData>>();

                // Collect combined data
                localProgress.Update("Collecting output data", 5);
                foreach (var subTaskOutput in subTasksOutputs) {
                    var compiledDataManagers = _taskLoader.GetOutputCompiledDataManagers(subTaskOutput.id);
                    foreach (var compiledDataManager in compiledDataManagers) {

                        var actionType = compiledDataManager.Key.Value;
                        var calculator = ActionCalculatorProvider.Create(actionType, null, false);
                        var comparisonData = calculator.LoadActionComparisonData(
                                compiledDataManager.Value,
                                subTaskOutput.id.ToString(),
                                subTaskOutput.Description
                            );
                        if (comparisonData != null) {
                            if (!collectedComparisons.ContainsKey(actionType)) {
                                collectedComparisons.Add(actionType, new Dictionary<IOutput, IActionComparisonData>());
                            }

                            var collectedResults = collectedComparisons[actionType];
                            collectedResults.Add(subTaskOutput, comparisonData);
                        }
                    }
                }

                // Only create output if there are results
                if (collectedComparisons.Any()) {

                    // Create output
                    var output = _outputManager.CreateOutput(task.id);
                    var reportSectionManager = _outputManager.CreateSectionManager(output);

                    // Create summary toc and summarize combined data
                    localProgress.Update("Summarizing combined report", 90);
                    var summaryToc = new SummaryToc(reportSectionManager);

                    // Sort comparisons according to the module mappings, main module first
                    var projectSettings = ProjectSettingsSerializer.ImportFromXmlString(task.SettingsXml, task.DataSourceConfiguration, false, out _);
                    var moduleMappings = ActionMappingFactory.Create(projectSettings, task.ActionType).GetModuleMappings();
                    var sortedComparisons = collectedComparisons.OrderByDescending(kv => {
                        var moduleMapping = moduleMappings.FirstOrDefault(m => m.ActionType == kv.Key);
                        return moduleMapping != null ? moduleMapping.Order : 0;
                    }).Select(x => new { x.Key, ComparisonData = x.Value.Values });

                    foreach (var sortedComparison in sortedComparisons) {
                        var collectedResults = sortedComparison.ComparisonData;
                        var calculator = ActionCalculatorProvider.Create(sortedComparison.Key, null, false);

                        calculator.SummarizeComparison(collectedResults, summaryToc);
                    }

                    // Render the output (html) recursively from the summaryToc
                    localProgress.Update("Saving combined output report", 90);
                    var headerState = progressReport.NewCompositeState(40);
                    summaryToc.SaveSummarySectionsRecursive(headerState);

                    // Update output and save to task and project
                    localProgress.Update("Saving output", 95);
                    output.SectionHeaderData = XmlSerialization.ToCompressedXml(summaryToc);
                    output.DateCreated = DateTime.Now;
                    _outputManager.SaveOutput(output, reportSectionManager);
                }
            }

            localProgress.Update("Task completed", 100);

            return new TaskExecutionResult();
        }
    }
}
