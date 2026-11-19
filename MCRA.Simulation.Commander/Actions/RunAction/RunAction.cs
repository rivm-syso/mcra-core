using System.IO.Compression;
using System.Text;
using System.Xml;
using MCRA.Data.Management;
using MCRA.Data.Management.CompiledDataManagers;
using MCRA.Data.Management.RawDataManagers;
using MCRA.Data.Management.RawDataProviders;
using MCRA.Data.Raw;
using MCRA.General;
using MCRA.General.Action.Settings;
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
            Simulation.ConfigureSbmlRoadRunnerOptions(appSettings);

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

                // If an override is given for the amount of significant digits to print
                // for numerical values (e.g. use 5 for regression test output for easy file diffs)
                if (options.SignificantDigits > 0) {
                    CsvOutputWriterFactory.DefaultOptions.SignificantDigits = options.SignificantDigits;
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

                var isLoop = tryGetLoopConfiguration(actionFolder, options, out var loopDef);

                // var isLoop = project.LoopScopingTypes?.Count > 0;
                var task = new TaskData {
                    ActionType = project.ActionType,
                    SettingsXml = projectSettingsXml,
                    DataSourceConfiguration = dsConfig,
                    Type = isLoop ? MCRATaskType.LoopCalculation : MCRATaskType.Simulation
                };
                var outputFolderName = diOutput.FullName;

                if (isLoop) {
                    createLoopTaskItems(dataManagerFactory, dsConfig, project, task, loopDef);
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
                    WriteDataFiles = !options.SkipTables,
                    WriteAsSpreadsheet = options.FileType.Contains('x', StringComparison.OrdinalIgnoreCase),
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

        private static bool tryGetLoopConfiguration(string actionFolder, RunActionOptions options, out LoopSettings def) {
            def = null;

            // Fetch loop configuration file, if any, this overrides an existing file "_loopConfig.xml"
            if (!string.IsNullOrEmpty(options.LoopConfigPath) && !File.Exists(options.LoopConfigPath)) {
                throw new Exception($"Specified loop configuration file {options.LoopConfigPath} does not exist.");
            }

            var loopConfigFile = string.IsNullOrEmpty(options.LoopConfigPath)
                ? Path.Combine(actionFolder, "_LoopSettings.xml")
                : options.LoopConfigPath;

            if (File.Exists(loopConfigFile)) {
                try {
                    def = XmlSerialization.FromXmlFile<LoopSettings>(loopConfigFile);
                    return true;
                } catch (Exception ex) {
                    throw new Exception($"Specified loop configuration file {loopConfigFile} contains invalid XML:\n{ex.Message}");
                }
            }

            //try deserializing old style loop scoping from Action settings file
            loopConfigFile = Path.Combine(actionFolder, "_ActionSettings.xml");
            if (File.Exists(loopConfigFile)) {
                try {
                    var actionLoopSettings = XmlSerialization.FromXmlFile<ActionLoopSettings>(loopConfigFile);
                    if (actionLoopSettings.LoopScopingTypes.Count > 0) {
                        var xmlDoc = new XmlDocument();
                        var elem = xmlDoc.CreateElement(nameof(LoopSettings.LoopConfiguration));
                        elem.InnerXml = XmlSerialization.ToXml(actionLoopSettings.LoopScopingTypes.First());

                        def = new() {
                            LoopType = McraLoopType.Scoping,
                            LoopConfiguration = elem
                        };
                        return true;
                    }
                } catch (Exception ex) {
                    throw new Exception($"Specified loop configuration file {loopConfigFile} contains invalid XML:\n{ex.Message}");
                }
            }
            return false;
        }

        private void createLoopTaskItems(
            IRawDataManagerFactory rdmFactory,
            DataSourceConfiguration dsConfig,
            ProjectDto project,
            TaskData task,
            LoopSettings loopDef
        ) {
            var xmlString = loopDef.LoopConfiguration.OuterXml;

            switch (loopDef.LoopType) {
                case McraLoopType.Scoping:
                    var scopeConfig = XmlSerialization.FromXml<ScopeLoopConfig>(xmlString);
                    createScopeLoopItems(rdmFactory, dsConfig, project, task, scopeConfig);
                    break;
                case McraLoopType.DataSource:
                    var dsLoopConfig = XmlSerialization.FromXml<DataSourceLoopConfig>(xmlString);
                    createDataSourceLoopItems(dsConfig, project, task, dsLoopConfig);
                    break;
                case McraLoopType.Setting:
                    var settingsLoopConfig = XmlSerialization.FromXml<SettingLoopConfig>(xmlString);
                    createSettingLoopItems(project.ActionType, dsConfig, task, settingsLoopConfig);
                    break;
                case McraLoopType.Complex:
                    var complexLoopConfig = XmlSerialization.FromXml<ComplexLoopConfig>(xmlString);
                    createComplexLoopItems(dsConfig, project, task, complexLoopConfig);
                    break;
                default:
                    break;
            }
        }

        private static void createScopeLoopItems(
            IRawDataManagerFactory rdmFactory,
            DataSourceConfiguration dsConfig,
            ProjectDto project,
            TaskData task,
            ScopeLoopConfig scopeConfig
        ) {
            var scopingType = scopeConfig.ScopingType;
            var actionMapping = ActionMappingFactory.Create(project, project.ActionType);
            var tableGroupMappings = actionMapping.GetTableGroupMappings();
            var provider = new ActionRawDataProvider(project, tableGroupMappings, rdmFactory);
            var linkManager = new CompiledLinkManager(provider);
            var scopeEntities = linkManager.GetAllScopeEntities(scopingType);
            //Create the project XMLs for the loop scoping keys
            foreach (var entity in scopeEntities.Values) {
                var subTaskName = !string.IsNullOrEmpty(entity.Name) ? $"{entity.Name} ({entity.Code})" : entity.Code;
                var settings = XmlSerialization.FromXml<ProjectDto>(task.SettingsXml);
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

        private static void createDataSourceLoopItems(
            DataSourceConfiguration dsConfig,
            ProjectDto project,
            TaskData task,
            DataSourceLoopConfig loopConfig
        ) {
            var actionMapping = ActionMappingFactory.Create(project, project.ActionType);
            var loopSourceIds = dsConfig.GetRawDataSourceIds(loopConfig.SourceTableGroup);

            //Create the project XMLs for the data sources
            foreach (var dsId in loopSourceIds) {
                //exclude all data source versions except current id
                DataSourceMappingRecord loopDsmr = null;
                var newDsConfig = new DataSourceConfiguration();

                foreach (var dsmr in dsConfig.DataSourceMappingRecords) {
                    if (dsmr.SourceTableGroup == loopConfig.SourceTableGroup) {
                        //only add the one record to the new ds config
                        if (dsmr.IdRawDataSourceVersion == dsId) {
                            loopDsmr = dsmr;
                            newDsConfig.DataSourceMappingRecords.Add(dsmr);
                        }
                    } else {
                        newDsConfig.DataSourceMappingRecords.Add(dsmr);
                    }
                }
                var subTaskName = $"{dsId}-{loopDsmr.Name}";

                var subTask = new TaskData {
                    Description = subTaskName,
                    ActionType = project.ActionType,
                    DataSourceConfiguration = newDsConfig,
                    SettingsXml = task.SettingsXml
                };

                task.ChildTasks.Add(subTask);
            }
        }

        private static void createSettingLoopItems(
            ActionType actionType,
            DataSourceConfiguration dsConfig,
            TaskData task,
            SettingLoopConfig loopConfig
        ) {
            //Create the project XMLs for the data sources
            foreach (var settingValue in loopConfig.LoopValues) {
                var subTaskName = $"{loopConfig.SettingsItemType}={settingValue}";
                var settings = XmlSerialization.FromXml<ProjectDto>(task.SettingsXml);

                settings.SetRawValue(actionType, loopConfig.SettingsItemType, settingValue);

                var subTask = new TaskData {
                    Description = subTaskName,
                    ActionType = actionType,
                    DataSourceConfiguration = dsConfig,
                    SettingsXml = XmlSerialization.ToXml(settings)
                };

                task.ChildTasks.Add(subTask);
            }
        }

        private static void createComplexLoopItems(
            DataSourceConfiguration dsConfig,
            ProjectDto project,
            TaskData task,
            ComplexLoopConfig loopConfig
        ) {
            var actionMapping = ActionMappingFactory.Create(project, project.ActionType);
            //Create the project XMLs for the data sources
            foreach (var loopItem in loopConfig.LoopItems) {
                var subTaskName = $"{loopItem.DisplayName}";
                var settings = XmlSerialization.FromXml<ProjectDto>(task.SettingsXml);

                foreach(var modConfig in loopItem.ModuleConfigurations) {
                    var modSettings = settings.GetModuleConfiguration(modConfig.ActionType);
                    modSettings.Apply(modConfig);
                }

                foreach(var scopeFilter in loopItem.ScopeKeysFilters) {
                    settings.SetFilterCodes(scopeFilter.ScopingType, scopeFilter.SelectedCodes);
                }

                var loopDs = dsConfig;
                if(loopItem.DataSources.Count > 0) {
                    var loopDsDict = loopItem.DataSources
                        .GroupBy(d => d.SourceTableGroup)
                        .ToDictionary(g => g.Key, g => g.Select(d => d.Name).ToHashSet());
                    loopDs = new DataSourceConfiguration();

                    foreach (var dsmr in dsConfig.DataSourceMappingRecords) {
                        if(loopDsDict.TryGetValue(dsmr.SourceTableGroup, out var dsNames)) {
                            if(dsNames.Contains(dsmr.Name)) {
                                loopDs.DataSourceMappingRecords.Add(dsmr);
                            }
                        } else {
                            loopDs.DataSourceMappingRecords.Add(dsmr);
                        }
                    }
                }

                var subTask = new TaskData {
                    Description = subTaskName,
                    ActionType = project.ActionType,
                    DataSourceConfiguration = loopDs,
                    SettingsXml = XmlSerialization.ToXml(settings)
                };

                task.ChildTasks.Add(subTask);
            }
        }

        /// <summary>
        /// Supports two options:
        /// 1) if no input path is specified, it uses the current directory, when you launch mcra.exe inside an action folder.
        /// 2) otherwise, it uses the specified inputpath from the cmd line argument.
        /// </summary>
        private static string DetermineInputPath(RunActionOptions options) {
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
            return rawDataManagerType switch {
                RawDataManagerType.Csv => new CsvRawDataManagerFactory(diBaseDataFolder.FullName),
                _ => throw new Exception($"No data manager available for manager type {rawDataManagerType}."),
            };
        }
    }
}
