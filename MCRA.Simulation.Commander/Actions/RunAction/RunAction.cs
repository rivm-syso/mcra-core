using System.IO.Compression;
using System.Text;
using MCRA.Data.Management;
using MCRA.Data.Management.CompiledDataManagers;
using MCRA.Data.Management.RawDataManagers;
using MCRA.Data.Management.RawDataProviders;
using MCRA.Data.Raw;
using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.General.KineticModelDefinitions;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.OutputGeneration.Helpers;
using MCRA.Simulation.OutputManagement;
using MCRA.Simulation.TaskExecution;
using MCRA.Simulation.TaskExecution.TaskExecuters;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.R.REngines;
using MCRA.Utils.Xml;
using Microsoft.Extensions.Configuration;

namespace MCRA.Simulation.Commander.Actions.RunAction {
    public class RunAction : ActionBase {

        public int Execute(RunActionOptions options) {
            awaitDebugger(options);

            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", false, true)
                .AddJsonFile($"appsettings.{env}.json", true, true)
                .AddJsonFile("appsettings.user.json", optional: true)
                .AddEnvironmentVariables();
            var appSettings = builder.Build();

            var timestamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");

            DirectoryInfo diBaseDataFolder = null;
            DirectoryInfo diOutput = null;
            bool isActionFolder = false;
            string actionFolder = null;
            string outputFolder = null;
            DirectoryInfo zipUnpackFolder = null;
            try {
                var inputPath = DetermineInputPath(options);
                isActionFolder = Directory.Exists(inputPath);
                var isZipFile = File.Exists(inputPath) && Path.GetExtension(inputPath).Equals(".zip");
                if (!isZipFile && !isActionFolder) {
                    Console.WriteLine($"The specified input path '{inputPath}' is not recognized as a valid zip file or folder.");
                    return 1;
                }

                // This executables dir
                var exeDirInfo = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);

                // Set REngine static paths
                RDotNetEngine.R_HomePath = appSettings.GetValue<string>("RHomePath");

                // Set python path
                Environment.SetEnvironmentVariable("PYTHONNET_PYDLL", appSettings.GetValue<string>("PythonPath"));

                // Get output folder name
                var outDirName = $"Out-{exeDirInfo.Name}";
                if (!options.OverwriteOutput) {
                    // No overwrite, so generate a separate folder for output: add timestamp
                    outDirName += $"-{timestamp}";
                }
                // If a random seed override is provided, add it to the folder name
                if (options.RandomSeed.HasValue) {
                    outDirName += $"-r{options.RandomSeed}";
                }

                // Get output folder
                var outputBaseFolder = !string.IsNullOrEmpty(options.OutputPath)
                    ? options.OutputPath
                    : appSettings.GetValue<string>("ProjectOutputBaseFolder");

                if (isActionFolder) {
                    // Input is a folder
                    var actionFolderName = Path.GetFileName(inputPath);
                    actionFolder = inputPath;
                    outputFolder = Path.IsPathRooted(outputBaseFolder)
                                   ? Path.Combine(outputBaseFolder, actionFolderName, outDirName)
                                   : Path.Combine(inputPath, outputBaseFolder, outDirName);
                } else {
                    // Input is a zip file: extract to users temp directory
                    (actionFolder, outputFolder, zipUnpackFolder) = ExtractZipFile(inputPath, outDirName, outputBaseFolder);
                }

                // Initialize output folder
                diOutput = new DirectoryInfo(outputFolder);
                var diMetadata = new DirectoryInfo(Path.Combine(outputFolder, "Metadata"));
                if (options.OverwriteOutput && diOutput.Exists) {
                    diOutput.Delete(true);
                    diOutput.Refresh();
                }
                diMetadata.Create();

                CopyOriginalSettingsFile(actionFolder, diMetadata);

                diBaseDataFolder = new DirectoryInfo(Path.Combine(outputFolder, "_ds"));
                diBaseDataFolder.Create();

                var versionFileName = Path.Combine(diMetadata.FullName, "MCRAVersion.txt");
                var versionInfo = "MCRA Simulation Commander\n" +
                                 $"Version:  {ThisAssembly.Git.BaseVersion.Major}." +
                                 $"{ThisAssembly.Git.BaseVersion.Minor}.{ThisAssembly.Git.BaseVersion.Patch}\n" +
                                 $"Revision: {ThisAssembly.Git.Commits}\n" +
                                 $"Tag:      {ThisAssembly.Git.BaseTag}\n" +
                                 $"Branch:   {ThisAssembly.Git.Branch}\n" +
                                 $"Commit:   {ThisAssembly.Git.Commit}\n" +
                                 $"Sha:      {ThisAssembly.Git.Sha}\n";

                //write MCRA version info to log file
                File.WriteAllText(versionFileName, versionInfo);

                // Create the raw data manager
                var dataManagerFactory = createRawDataManagerFactory(
                    options.RawDataManagerType,
                    diBaseDataFolder
                );
                diBaseDataFolder.Refresh();

                ProjectDto project;
                DataSourceConfiguration dsConfig;

                // Import action folder
                var importProgress = createProgressReport(options.SilentMode);
                if (!options.SilentMode) {
                    Console.WriteLine("Loading action...");
                }
                using (var dataManager = dataManagerFactory.CreateRawDataManager()) {
                    using (var im = new FileImportManager(dataManager)) {
                        (project, dsConfig) = im.ImportAction(actionFolder, importProgress);
                    }
                }
                importProgress.MarkCompleted();
                if (!options.SilentMode) {
                    printConsole("  Done!", true);
                }

                if (options.RandomSeed.HasValue) {
                    // Override project seed value with option value
                    project.ActionSettings.RandomSeed = options.RandomSeed.Value;
                }

                var projectSettingsXml = XmlSerialization.ToXml(project, true);
                var isLoop = project.LoopScopingTypes?.Count > 0;
                var task = new TaskData {
                    ActionType = project.ActionType,
                    SettingsXml = projectSettingsXml,
                    DataSourceConfiguration = dsConfig,
                    Type = isLoop ? MCRATaskType.LoopCalculation : MCRATaskType.Simulation
                };
                var outputFolderName = diOutput.FullName;
                // check whether we have a loop on our hands
                if (isLoop) {
                    if (project.LoopScopingTypes.Count > 1) {
                        throw new Exception("Loops over multiple entity types are currently not supported.");
                    }
                    var scopingType = project.LoopScopingTypes.Single();
                    var actionMapping = ActionMappingFactory.Create(project, project.ActionType);
                    var tableGroupMappings = actionMapping.GetTableGroupMappings();
                    var provider = new ActionRawDataProvider(project, tableGroupMappings, dataManagerFactory);
                    var linkManager = new CompiledLinkManager(provider);
                    var scopeEntities = linkManager.GetAllScopeEntities(scopingType);
                    //Create the project XMLs for the loop scoping keys
                    foreach (var entity in scopeEntities.Values) {
                        var subTaskName = !string.IsNullOrEmpty(entity.Name) ? $"{entity.Name} ({entity.Code})" : entity.Code;
                        var settings = XmlSerialization.FromXml<ProjectDto>(projectSettingsXml);
                        var loopEntityKeyFilter = settings.ScopeKeysFilters.FirstOrDefault(r => r.ScopingType == scopingType);
                        if (loopEntityKeyFilter == null) {
                            loopEntityKeyFilter = new ScopeKeysFilter() {
                                ScopingType = scopingType,
                                SelectedCodes = [entity.Code]
                            };
                            settings.ScopeKeysFilters.Add(loopEntityKeyFilter);
                        } else {
                            loopEntityKeyFilter.SelectedCodes = [entity.Code];
                        }

                        var subTask = new TaskData {
                            Description = subTaskName,
                            ActionType = project.ActionType,
                            DataSourceConfiguration = dsConfig,
                            SettingsXml = XmlSerialization.ToXml(settings)
                        };
                        task.ChildTasks.Add(subTask);
                    }
                }

                // Save project settings as created to xml, to be able to make a comparison of
                // settings between original, transformed and actual settings of this run.
                // Final project settings from the created project that is initialized
                // with the ProjectRunSettings.xml and serialized once more to get the
                // actual settings for the run:
                var createdActionSettingsFileName = Path.Combine(diMetadata.FullName, "ActionSimulatedSettings.xml");
                File.WriteAllText(createdActionSettingsFileName, projectSettingsXml);

                if (!options.SilentMode) {
                    Console.WriteLine("\nRunning action...");
                }
                // Create task loader
                var taskLoader = new GenericTaskLoader((settings, ds) => {
                    var actionMapping = ActionMappingFactory.Create(settings, settings.ActionType);
                    var tableGroupMappings = actionMapping.GetTableGroupMappings();
                    var provider = new ActionRawDataProvider(settings, tableGroupMappings, dataManagerFactory);
                    return provider;
                });

                // Create output manager
                var outputManager = new StoreLocalOutputManager(outputFolderName) {
                    WriteReport = !options.SkipReport,
                    WriteCsvFiles = !options.SkipTables,
                    WriteChartFiles = !options.SkipCharts
                };

                // Create task executer and run
                var executer = new SimulationTaskExecuter(
                    taskLoader,
                    outputManager,
                    "log4net.config"
                ) {
                    KeepTemporaryFiles = options.KeepTempFiles,
                };

                if (isLoop) {
                    var numTasks = task.ChildTasks.Count;
                    var counter = 0;
                    foreach (var subTask in task.ChildTasks) {
                        counter++;
                        if (!options.SilentMode) {
                            Console.WriteLine($"\nRunning subtask '{subTask.Description}' ({counter} of {numTasks})...");
                        }

                        var invalidChars = Path.GetInvalidFileNameChars();
                        var fileNameSb = new StringBuilder(subTask.Description);
                        invalidChars.ForAll(c => fileNameSb.Replace(c, '_'));
                        outputFolderName = Path.Combine(diOutput.FullName, "Reports", fileNameSb.ToString());
                        Directory.CreateDirectory(outputFolderName);
                        outputManager.SetOutputPath(outputFolderName);
                        // Run the task
                        var taskProgress = createProgressReport(options.SilentMode);
                        executer.Run(subTask, taskProgress);

                        taskProgress.MarkCompleted();
                    }
                    if (!options.SkipReport) {
                        //render top level report, TODO: refactor/use the loop executer mechanism
                        //to generate the combined report in the overview.
                        //For now: just generate a simple HTML page
                        var mainRepBuilder = new ReportBuilder(null);
                        var mainOutput = new OutputInfo {
                            Title = task.Description,
                        };
                        var mainHtml = mainRepBuilder.RenderCombinedReport(
                            mainOutput,
                            task.ChildTasks.Select(t => new OutputInfo { Title = t.Description }),
                            false,
                            diOutput.FullName
                        );
                        var mainHtmlFileName = Path.Combine(diOutput.FullName, "Report.html");
                        File.WriteAllText(mainHtmlFileName, mainHtml);
                    }
                } else {
                    // Run the task
                    var taskProgress = createProgressReport(options.SilentMode);
                    executer.Run(task, taskProgress);
                    taskProgress.MarkCompleted();
                }

                if (!options.SilentMode) {
                    printConsole("  Done!", true);
                }

                Console.WriteLine("\n\nMCRA run finished!");

                return 0;
            } catch (Exception ex) {
                Console.WriteLine();
                Console.WriteLine("MCRA run failed!");
                Console.WriteLine(ex.ToString());
                return 1;
            } finally {
                if (!options.KeepTempFiles) {
                    // Delete temporary data folder if it exists
                    try {
                        if (diBaseDataFolder != null && diBaseDataFolder.Exists) {
                            diBaseDataFolder?.Delete(true);
                        }
                        if (zipUnpackFolder != null && zipUnpackFolder.Exists) {
                            zipUnpackFolder.Delete(true);
                        }
                    } catch (Exception ex) {
                        Console.WriteLine(ex.ToString());
                    }
                }

                if (options.InteractiveMode) {
                    Console.SetCursorPosition(1, Console.CursorTop);
                    Console.Write(new string(' ', Console.WindowWidth));
                    Console.WriteLine();
                    Console.WriteLine("Press enter to exit...");
                    Console.ReadKey();
                }
            }
        }

        /// <summary>
        /// Supports two options:
        /// 1) if no input path is specified, it uses the current directory, when you launch mcra.exe inside an action folder.
        /// 2) otherwise, it uses the specified inputpath from the cmd line argument.
        /// </summary>
        private string DetermineInputPath(RunActionOptions options) {
            if (string.IsNullOrWhiteSpace(options.InputPath)) {
                return Directory.GetCurrentDirectory();
            } else {
                return Path.IsPathRooted(options.InputPath)
                    ? options.InputPath
                    : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, options.InputPath);
            }
        }

        /// <summary>
        /// Extracts a zipped action file to the user temp folder.
        /// </summary>
        /// <param name="zipFilePath">Full path name to the action zip file.</param>
        /// <param name="outDirName">End part of the output folder.</param>
        /// <param name="outputBaseFolder">Base name of the output folder.</param>
        /// <returns></returns>
        private static (string actionFolder, string outputFolder, DirectoryInfo zipUnpackFolder) ExtractZipFile(string zipFilePath, string outDirName, string outputBaseFolder) {
            var actionFolderName = Path.GetFileNameWithoutExtension(zipFilePath);
            var outputFolder = Path.IsPathRooted(outputBaseFolder)
                           ? Path.Combine(outputBaseFolder, actionFolderName, outDirName)
                           : Path.Combine(Directory.GetParent(zipFilePath).ToString(), outputBaseFolder, actionFolderName, outDirName);

            string actionFolder = string.Empty;
            DirectoryInfo zipUnpackFolder = null;
            using (var zip = ZipFile.OpenRead(zipFilePath)) {
                var zipUnpackFolderName = $"{actionFolderName}-{DateTime.Now:yyyyMMddHHmmss}-{Guid.NewGuid().ToString("N").Substring(0, 8)}";
                zipUnpackFolder = new DirectoryInfo(Path.Combine(Path.GetTempPath(), zipUnpackFolderName));
                actionFolder = zipUnpackFolder.FullName;
                zip.ExtractToDirectory(zipUnpackFolder.FullName, overwriteFiles: true);

                CheckAndCorrectIfFolderIsZipped(zipUnpackFolder, actionFolderName);
            }
            return (actionFolder, outputFolder, zipUnpackFolder);
        }

        /// <summary>
        /// Special case: the (unpacked) zipfile contained the action folder and inside that folder are the true settings
        ///               and data files. We move them upwards one level.
        /// </summary>
        private static void CheckAndCorrectIfFolderIsZipped(DirectoryInfo zipUnpackFolder, string actionFolderName) {
            var actionSubFolder = Path.Combine(zipUnpackFolder.FullName, actionFolderName);
            if (Directory.Exists(actionSubFolder)) {
                // Special case: zipfile contained the action folder and inside that folder are th true settings and data files.
                //               We move them one level upwards.
                string[] fileList = Directory.GetFiles(Path.Combine(zipUnpackFolder.FullName, actionFolderName), "*.*", SearchOption.AllDirectories);
                foreach (var file in fileList) {
                    string fileToMove = file;
                    var moveTo = Path.Combine(zipUnpackFolder.FullName, Path.GetRelativePath(actionSubFolder, file));

                    if (!Directory.Exists(Path.GetDirectoryName(moveTo))) {
                        Directory.CreateDirectory(Path.GetDirectoryName(moveTo));
                    }
                    File.Move(fileToMove, moveTo);
                }
            }
        }

        private static void CopyOriginalSettingsFile(string actionFolder, DirectoryInfo diOutput) {
            var actionFiles = Directory.EnumerateFiles(actionFolder);
            var originalActionSettingsFileName = Path.Combine(diOutput.FullName, "ActionOriginalSettings.xml");
            var validSettingsFileNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase) {
                "ProjectSettings.xml", "_ActionSettings.xml", "ActionSettings.xml"
            };
            foreach (var file in actionFiles) {
                if (validSettingsFileNames.Contains(Path.GetFileName(file))) {
                    File.Copy(file, originalActionSettingsFileName);
                    break;
                }
            }
        }

        private static IRawDataManagerFactory createRawDataManagerFactory(
            RawDataManagerType rawDataManagerType,
            DirectoryInfo diBaseDataFolder
        ) {
            switch (rawDataManagerType) {
                case RawDataManagerType.Csv:
                    return new CsvRawDataManagerFactory(diBaseDataFolder.FullName);
                default:
                    throw new Exception($"No data manager available for manager type {rawDataManagerType}.");
            }
        }
    }
}
