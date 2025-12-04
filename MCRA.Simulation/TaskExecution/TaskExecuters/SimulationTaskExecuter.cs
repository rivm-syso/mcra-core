using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Xml;
using MCRA.Data.Management;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.OutputGeneration.Helpers;
using MCRA.Simulation.OutputManagement;
using System.Globalization;
using MCRA.General;

namespace MCRA.Simulation.TaskExecution.TaskExecuters {

    public class SimulationTaskExecuter : TaskExecuterBase {

        public bool KeepTemporaryFiles { get; set; } = false;

        private readonly IOutputManager _outputManager;

        private readonly ITaskLoader _taskLoader;

        public SimulationTaskExecuter(ITaskLoader taskLoader,
            IOutputManager outputManager,
            string log4netConfigFile)
            : base(log4netConfigFile)
        {
            _taskLoader = taskLoader;
            _outputManager = outputManager;
        }

        /// <summary>
        /// Initializes, runs, and summarizes the output of the specified simulation task.
        /// </summary>
        /// <param name="task"></param>
        /// <param name="progressReport"></param>
        public override TaskExecutionResult Run(
            ITask task,
            CompositeProgressState progressReport
        ) {
            var startTime = DateTime.Now;

            // Set thread culture info to invariant, for rendering doubles with decimal dot.
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

            DirectoryInfo tempDataFolder = null;
            try {
                var localProgress = progressReport.NewProgressState(1);
                localProgress.Update("Initializing");

                // Initialize log file
                setupLogFile(task.id);
                _log.Info($"Task {task.id} started.");

                // Create output and get output id
                var output = _outputManager.CreateOutput(task.id);
                tempDataFolder = new DirectoryInfo(_outputManager.GetOutputTempFolder(output));
                var reportSectionManager = _outputManager.CreateSectionManager(output);
                var outputRawDataWriter = _outputManager.CreateRawDataWriter(output);

                localProgress.Update("Loading action and data source configuration", 2);

                (var project, var compiledDataManager) = _taskLoader.Load(task);

                // Create the action mapping and runner
                var actionMapping = ActionMappingFactory.Create(project, project.ActionType);
                var actionRunner = new ActionRunner(project);

                // Create summary toc
                var summaryToc = new SummaryToc(reportSectionManager);

                // Summarize settings
                localProgress.Update("Summarizing settings and data sources", 20);
                actionRunner.SummarizeSettings(actionMapping, summaryToc);

                // Summarize data sources
                var settingsHeader = summaryToc.GetSubSectionHeader(OutputConstants.ActionSettingsSectionGuid);
                var dataSourceVersions = project.ProjectDataSourceVersions.SelectMany(v => v.Value);
                actionRunner.SummarizeDataSources(actionMapping, dataSourceVersions, settingsHeader);

                // Run the action
                localProgress.Update("Running action", 40);
                var subsetManager = new SubsetManager(compiledDataManager, project, tempDataFolder);
                var runProgress = progressReport.NewCompositeState(59);
                actionRunner.Run(actionMapping, subsetManager, summaryToc, outputRawDataWriter, runProgress);

                // Render the output (html) recursively from the summaryToc
                localProgress.Update("Rendering and saving output", 60);
                var outputWritingProgress = progressReport.NewCompositeState(40);
                summaryToc.SaveSummarySectionsRecursive(outputWritingProgress);
                outputWritingProgress.MarkCompleted();

                // Render the short output summary for standard actions if applicable
                if (!string.IsNullOrEmpty(project.ShortOutputTemplate)) {
                    var builder = new ReportBuilder(summaryToc);
                    var resolvedHtml = builder.ResolveReportTemplate(project.ShortOutputTemplate);
                    output.OutputSummary = XmlSerialization.CompressString(resolvedHtml);
                }

                // Update output and save to task and project
                output.SectionHeaderData = XmlSerialization.ToCompressedXml(summaryToc);
                output.DateCreated = DateTime.Now;
                _outputManager.SaveOutput(output, reportSectionManager);

                if (KeepTemporaryFiles) {
                    //save settings xml of project after the run
                    var finalSettingsXml = XmlSerialization.ToXml(project);
                    File.WriteAllText(Path.Combine(tempDataFolder.FullName, "_FinalProjectSettings.xml"), finalSettingsXml);
                }

                localProgress.Update("Task completed", 100);
                _log.Info($"Task {task.id} completed.");
            } catch (OperationCanceledException) {
                throw;
            } catch (Exception ex) {
                var innerMessage = !string.IsNullOrEmpty(ex.InnerException?.Message)
                    ? $" - {ex.InnerException?.Message}"
                    : string.Empty;
                _log.Debug($"Task {task.id} failed. Error: {ex.Message}{innerMessage}");
                _log.Info($"Error: {ex.Message}{innerMessage}");
                _log.Info($"Trace: {ex.StackTrace}");
                throw new Exception($"{ex.Message}{innerMessage}");
            } finally {
                try {
                    if (!KeepTemporaryFiles) {
                        tempDataFolder?.Delete(true);
                    }
                    _log.Info($"Running time: {DateTime.Now - startTime}");
                } catch (Exception ex) {
                    _log.Info($"Temp data folder removal failed:\n{ex}");
                }
            }

            return new TaskExecutionResult();
        }
    }
}
