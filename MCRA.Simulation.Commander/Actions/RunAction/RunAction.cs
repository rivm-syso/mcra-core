using MCRA.Data.Management;
using MCRA.Data.Management.RawDataManagers;
using MCRA.Data.Management.RawDataProviders;
using MCRA.Data.Raw;
using MCRA.General;
using MCRA.General.Action.Settings.Dto;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputManagement;
using MCRA.Simulation.TaskExecution;
using MCRA.Simulation.TaskExecution.TaskExecuters;
using MCRA.Utils.Csv;
using MCRA.Utils.R.REngines;
using MCRA.Utils.Xml;
using Microsoft.Extensions.Configuration;
using System.IO.Compression;

namespace MCRA.Simulation.Commander.Actions.RunAction {
    public class RunAction : ActionBase {

        public int Execute(RunActionOptions options) {
            awaitDebugger(options);

            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", false, true)
                .AddJsonFile($"appsettings.{env}.json", true, true)
                .AddEnvironmentVariables();
            var appSettings = builder.Build();

            var timestamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");

            DirectoryInfo diBaseDataFolder = null, diOutput = null;
            var isProjectFolder = false;
            string zipFileName = null, projectName = null, outputFolder = null;
            try {
                var inputPath = options.InputPath;
                isProjectFolder = Directory.Exists(inputPath);
                var isZipFile = File.Exists(inputPath) && Path.GetExtension(inputPath).Equals(".zip");
                if (!isZipFile && !isProjectFolder) {
                    Console.WriteLine($"The specified input path '{inputPath}' is not recognized as a valid zip file or folder.");
                    return 1;
                }

                // This executables dir
                var exeDirInfo = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);

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

                if (isProjectFolder) {
                    // Input is a folder; create zip file from folder
                    projectName = Path.GetFileName(inputPath);
                    zipFileName = $"{projectName}-{DateTime.Now:yyyyMMddHHmmss}-{Guid.NewGuid().ToString("N").Substring(0, 8)}.zip";
                    ZipFile.CreateFromDirectory(inputPath, zipFileName);
                    outputFolder = Path.IsPathRooted(outputBaseFolder)
                        ? Path.Combine(outputBaseFolder, $"{projectName}\\{outDirName}")
                        : Path.Combine(inputPath, $"{outputBaseFolder}\\{outDirName}");
                } else {
                    // Input is a zip file
                    zipFileName = inputPath;
                    projectName = Path.GetFileNameWithoutExtension(zipFileName);
                    outputFolder = Path.Combine(outputBaseFolder, $"{projectName}\\{outDirName}");
                }

                // Initialize output folder
                diOutput = new DirectoryInfo(outputFolder);
                if (options.OverwriteOutput && diOutput.Exists) {
                    diOutput.Delete(true);
                    diOutput.Refresh();
                }
                if (!diOutput.Exists) {
                    diOutput.Create();
                }

                // Copy original xml settings file for this run
                var originalActionSettingsFileName = Path.Combine(diOutput.FullName, "ProjectOriginalSettings.xml");
                using (var zip = ZipFile.Open(zipFileName, ZipArchiveMode.Read)) {
                    foreach (var e in zip.Entries) {
                        if (e.Name.Equals("ProjectSettings.xml", StringComparison.OrdinalIgnoreCase) ||
                            e.Name.Equals("_ActionSettings.xml", StringComparison.OrdinalIgnoreCase)
                        ) {
                            e.ExtractToFile(originalActionSettingsFileName);
                        }
                    }
                }

                var transformedActionSettingsFileName = Path.Combine(diOutput.FullName, "ProjectRunSettings.xml");

                // Final project settings from the created project that is initialized
                // with the ProjectRunSettings.xml and serialized once more to get the
                // actual settings for the run:
                var createdProjectSettingsFileName = Path.Combine(diOutput.FullName, "ProjectSimulatedSettings.xml");

                diBaseDataFolder = new DirectoryInfo(Path.Combine(outputFolder, "_ds"));
                if (!diBaseDataFolder.Exists) {
                    diBaseDataFolder.Create();
                }

                var versionFileName = Path.Combine(diOutput.FullName, "_MCRAVersion.txt");
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


                // Create progress report
                var progress = createProgressReport(options.SilentMode);

                // Create the raw data manager
                var dataManagerFactory = createRawDataManagerFactory(
                    options.RawDataManagerType,
                    diBaseDataFolder
                );
                diBaseDataFolder.Refresh();

                ProjectDto project;
                DataSourceConfiguration dsConfig;

                // Import zip file
                using (var dataManager = dataManagerFactory.CreateRawDataManager()) {
                    using (var im = new FileImportManager(dataManager)) {
                        (project, dsConfig) = im.ImportZipFile(zipFileName, progress);
                    }
                }
                if (options.RandomSeed.HasValue) {
                    // Override project seed value with option value
                    project.MonteCarloSettings.RandomSeed = options.RandomSeed.Value;
                }

                // Save project settings as created to xml, to be able to make a comparison of
                // settings between original, transformed and actual settings of this run.
                var projectSettingsXml = XmlSerialization.ToXml(project, true);
                File.WriteAllText(createdProjectSettingsFileName, projectSettingsXml);

                // Set REngine static paths
                RDotNetEngine.R_HomePath = appSettings.GetValue<string>("RHomePath");

                // Sset the CsvWriter configuration to use max 5 significant
                // digits for all floating point numbers
                CsvWriter.SignificantDigits = 5;

                if (!options.SilentMode) {
                    Console.WriteLine("\nStarting simulation...");
                }
                // Create task loader
                var taskLoader = new GenericTaskLoader((settings, ds) => {
                    var actionMapping = ActionMappingFactory.Create(settings, settings.ActionType);
                    var tableGroupMappings = actionMapping.GetTableGroupMappings();
                    var provider = new ActionRawDataProvider(settings, tableGroupMappings, dataManagerFactory);
                    return provider;
                });

                // Create output manager
                var outputManager = new StoreLocalOutputManager(diOutput.FullName) {
                    WriteReport = !options.SkipReport,
                    WriteCsvFiles = !options.SkipTables,
                    WriteChartFiles = !options.SkipCharts
                };

                // Create task
                var task = new TaskData {
                    ActionType = project.ActionType,
                    SettingsXml = projectSettingsXml,
                    DataSourceConfiguration = dsConfig
                };

                // Create task executer and run
                var executer = new SimulationTaskExecuter(
                    taskLoader,
                    outputManager,
                    "log4net.config"
                ) {
                    KeepTemporaryFiles = options.KeepTempFiles,
                };

                // Run the task
                executer.Run(task, progress);

                Console.WriteLine("\nMCRA run finished!");

                return 0;
            } catch (Exception ex) {
                Console.WriteLine();
                Console.WriteLine("MCRA run failed!");
                Console.WriteLine(ex.ToString());
                return 1;
            } finally {
#if DEBUG
                if (options.InteractiveMode) {
                    Console.SetCursorPosition(1, Console.CursorTop);
                    Console.Write(new string(' ', Console.WindowWidth));
                    Console.WriteLine();
                    Console.WriteLine("Press enter to exit...");
                    Console.ReadKey();
                }
#endif
                if (isProjectFolder) {
                    try {
                        // Remove temporary zip file created from folder
                        File.Delete(zipFileName);
                    } catch {
                        /* no action */
                    }
                }
                if (!options.KeepTempFiles && (diBaseDataFolder?.Exists ?? false)) {
                    // Delete temporary data folder if it exists
                    try {
                        diBaseDataFolder?.Delete(true);
                    } catch (Exception ex) {
                        Console.WriteLine(ex.ToString());
                    }
                }
            }
        }

        private static IRawDataManagerFactory createRawDataManagerFactory(
            RawDataManagerType rawDataManagerType,
            DirectoryInfo diBaseDataFolder
        ) {
            switch (rawDataManagerType) {
                case RawDataManagerType.Binary:
                    return new BinaryRawDataManagerFactory(diBaseDataFolder.FullName);
                case RawDataManagerType.Csv:
                    return new CsvRawDataManagerFactory(diBaseDataFolder.FullName);
                default:
                    throw new Exception($"No data manager available for manager type {rawDataManagerType}.");
            }
        }
    }
}
