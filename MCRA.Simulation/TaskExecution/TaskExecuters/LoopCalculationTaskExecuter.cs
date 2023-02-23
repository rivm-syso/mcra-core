using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Xml;
using MCRA.Data.Management;
using MCRA.Simulation.Actions;
using MCRA.Simulation.Actions.ActionComparison;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.OutputManagement;
using System.Globalization;
using MCRA.General;

namespace MCRA.Simulation.TaskExecution.TaskExecuters {
    public class LoopCalculationTaskExecuter : TaskExecuterBase {

        private readonly IOutputManager _outputManager;

        private readonly ITaskLoader _taskLoader;

        public LoopCalculationTaskExecuter(
            ITaskLoader loopTaskLoader,
            IOutputManager outputManager,
             string log4netConfigFile)
            : base(log4netConfigFile)
        {
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
            var outputs = _taskLoader.CollectSubTaskOutputs(task);

            // Get the (multi-)task (including authorisation checks)
            if (outputs?.Any() ?? false) {

                // Get action type from task and create combined output calculator
                localProgress.Update("Creating combined output calculator", 5);
                var actionType = task.ActionType;
                var calculator = ActionCalculatorProvider.Create(actionType, null, false);

                // Collect combined data
                localProgress.Update("Collecting output data", 5);
                var collectedResults = new Dictionary<IOutput, IActionComparisonData>();
                foreach (var subTaskOutput in outputs) {
                    var compiledDataManager = _taskLoader.GetOutputCompiledDataManager(subTaskOutput.id);
                    if (compiledDataManager != null) {
                        var comparisonData = calculator.LoadActionComparisonData(
                                compiledDataManager,
                                subTaskOutput.id.ToString(),
                                subTaskOutput.Description
                            );
                        if (comparisonData != null) {
                            collectedResults.Add(subTaskOutput, comparisonData);
                        }
                    }
                }

                // Only create output if there are results
                if (collectedResults.Any()) {
                    // Create output
                    var output = _outputManager.CreateOutput(task.id);
                    var reportSectionManager = _outputManager.CreateSectionManager(output);

                    // Create summary toc and summarize combined data
                    localProgress.Update("Summarizing combined report", 90);
                    var summaryToc = new SummaryToc(reportSectionManager);
                    calculator.SummarizeComparison(collectedResults.Values, summaryToc);

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
