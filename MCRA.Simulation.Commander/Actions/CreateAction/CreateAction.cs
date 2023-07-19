using MCRA.Data.Management.DataTemplateGeneration;
using MCRA.General;
using MCRA.General.Action.ActionSettingsManagement;
using MCRA.General.Action.Serialization;
using MCRA.General.Action.Settings;
using MCRA.General.FileDefinitions;
using MCRA.General.ModuleDefinitions;
using MCRA.Simulation.Action;
using MCRA.Utils.DataFileReading;
using MCRA.Utils.Xml;
using Microsoft.Extensions.Configuration;

namespace MCRA.Simulation.Commander.Actions.CreateAction {
    /// <summary>
    /// Creates an action folder for a given action type. The action folder contains the settings
    /// and empty data files, which serves as a start for the user to complete with its own data.
    /// </summary>
    public class CreateAction : ActionBase {

        public int Execute(CreateActionOptions options) {

            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", false, true)
                .AddJsonFile($"appsettings.{env}.json", true, true)
                .AddEnvironmentVariables();
            var appSettings = builder.Build();

            if (options.SupportedTypes) {
                WriteSupportedActionTypes();
                return 0;
            }

            try {
                if (!ValidateActionType(options.ActionType)) {
                    return 0;
                }

                DirectoryInfo actionFolder = CreateUniqueActionFolder(options.OutputPath, options.Name);
                ProjectDto action = CreateNewAction(actionFolder.Name, options.ActionType);
                CreateActionSettings(action, actionFolder);
                CreateDataFiles(action, actionFolder, out List<DataSourceMappingRecord> dataSourceMappingRecords);
                CreateDataAction(dataSourceMappingRecords, actionFolder);

                Console.WriteLine();
                Console.WriteLine($"MCRA created your new {action.ActionType} action in folder:\n{actionFolder.FullName}");
                Console.WriteLine();

                return 0;
            } catch (Exception ex) {
                Console.WriteLine();
                WriteErrorMessage($"MCRA create failed.");
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
            }
        }

        private bool ValidateActionType(string actionTypeString) {
            bool isValid = Enum.TryParse<ActionType>(actionTypeString, ignoreCase: true, out ActionType result);
            if (!isValid) {
                WriteErrorMessage($"The specified action type \'{actionTypeString}\' is not valid.");
                Console.WriteLine();
                WriteSupportedActionTypes();
            }

            return isValid;
        }

        private static void WriteSupportedActionTypes() {
            var oldColor = Console.ForegroundColor;
            Console.WriteLine("Supported action types are:");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine();
            Console.WriteLine(string.Join("\n", Enum.GetNames<ActionType>().Where(t => t != ActionType.Unknown.ToString())));
            Console.ForegroundColor = oldColor;
        }

        /// <summary>
        /// Creates a unique folder for a given action name. When the folder already exists,
        /// it tries to create a next sequence numbered folder according to the pattern
        /// name (2), name (3), name (4), etc.
        /// </summary>
        private DirectoryInfo CreateUniqueActionFolder(string outputPath, string actionName) {
            int count = 2;
            var folderName = Path.Combine(outputPath, actionName);
            while (Directory.Exists(folderName)) {
                folderName = Path.Combine(outputPath, $"{actionName} ({count++})");
            }

            var actionFolder = Directory.CreateDirectory(folderName);

            // Create Data subfolder
            Directory.CreateDirectory(Path.Combine(folderName, FileDefinitions.ActionDataFolderName));

            return actionFolder;
        }

        private ProjectDto CreateNewAction(string name, string actionTypeString) {
            ActionType actionType = Enum.Parse<ActionType>(actionTypeString, ignoreCase: true);
            var creationDate = DateTime.Now;
            var action = new ProjectDto { ActionType = actionType, Name = name, DateCreated = creationDate, DateModified = creationDate };

            var actionSettingsManager = ActionSettingsManagerFactory.Create(actionType);
            if (actionSettingsManager != null) {
                actionSettingsManager.InitializeAction(action);
            }

            return action;
        }

        private void CreateActionSettings(ProjectDto action, DirectoryInfo actionDirectoryInfo) {
            var xmlString = ProjectSettingsSerializer.ExportToXmlString(action, format: true);
            var settingsFile = Path.Combine(actionDirectoryInfo.FullName, FileDefinitions.ActionSettingsFileName);
            File.WriteAllText(settingsFile, xmlString);
        }

        private void CreateDataFiles(ProjectDto action, DirectoryInfo actionDirectoryInfo, out List<DataSourceMappingRecord> dataSourceMappingRecords) {
            dataSourceMappingRecords = new List<DataSourceMappingRecord>();
            int rawDataSourceVersionId = 1;

            var sourceTableGroupsForAction = GetSourceTableGroupsForAction(action);
            foreach (var sourceTableGroup in sourceTableGroupsForAction) {
                var excelDataFileName = CreateExcelFile(actionDirectoryInfo, sourceTableGroup);

                dataSourceMappingRecords.Add(new DataSourceMappingRecord {
                    IdRawDataSourceVersion = rawDataSourceVersionId++,
                    Checksum = DataSourceReaderBase.CalculateFileHashBase64(excelDataFileName),
                    Name = Path.GetFileName(excelDataFileName),
                    RawDataSourcePath = Path.GetFileName(excelDataFileName),
                    SourceTableGroup = sourceTableGroup
                });
            }
        }

        private string CreateExcelFile(DirectoryInfo actionDirectoryInfo, SourceTableGroup sourceTableGroup) {
            var excelDataFileName = Path.Combine(actionDirectoryInfo.FullName, FileDefinitions.ActionDataFolderName, sourceTableGroup.ToString());
            excelDataFileName = Path.ChangeExtension(excelDataFileName, "xlsx");
            var excelTemplateGenerator = new ExcelDatasetTemplateGenerator(excelDataFileName);
            excelTemplateGenerator.Create(sourceTableGroup);
            return excelDataFileName;
        }

        private List<SourceTableGroup> GetSourceTableGroupsForAction(ProjectDto action) {
            ActionMapping actionMapping = ActionMappingFactory.Create(action, action.ActionType);
            var sourceTableGroups = new List<SourceTableGroup>();

            var visibleModules = actionMapping.ModuleMappingsDictionary.Where(e => e.Value.IsVisible == true).ToList();
            foreach (var module in visibleModules) {
                var moduleDefinition = McraModuleDefinitions.Instance.ModuleDefinitions[module.Key];
                if (moduleDefinition.SourceTableGroup != SourceTableGroup.Unknown) {
                    sourceTableGroups.Add(moduleDefinition.SourceTableGroup);
                }
            }
            return sourceTableGroups;
        }

        private void CreateDataAction(List<DataSourceMappingRecord> dataSourceMappingRecords, DirectoryInfo actionDirectoryInfo) {
            var dataSourceConfiguration = new DataSourceConfiguration();
            dataSourceConfiguration.DataSourceMappingRecords = dataSourceMappingRecords;

            string xmlString = dataSourceConfiguration.ToXml(format: true);
            var actionDataFile = Path.Combine(actionDirectoryInfo.FullName, FileDefinitions.ActionDataFileName);
            File.WriteAllText(actionDataFile, xmlString);
        }

        private void WriteErrorMessage(string message) {
            var oldColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ForegroundColor = oldColor;
        }
    }
}
