using MCRA.Data.Management;
using MCRA.Data.Management.CompiledDataManagers;
using MCRA.Data.Raw;
using MCRA.General;
using MCRA.General.Action.Serialization;
using MCRA.General.Action.Settings.Dto;
using System;
using System.Collections.Generic;

namespace MCRA.Simulation.TaskExecution {
    public class GenericTaskLoader : ITaskLoader {

        private readonly Func<ProjectDto, DataSourceConfiguration, IRawDataProvider> _rawDataProviderFactory;

        public GenericTaskLoader(Func<ProjectDto, DataSourceConfiguration, IRawDataProvider> rawDataProviderFactory) {
            _rawDataProviderFactory = rawDataProviderFactory;
        }

        /// <summary>
        /// Creates a project and compiled data manager instance for the specified task.
        /// </summary>
        /// <param name="actionTypeDefault"></param>
        /// <param name="dsConfig"></param>
        /// <param name="projectXml"></param>
        /// <returns></returns>
        public (ProjectDto, ICompiledDataManager) Load(ITask task) {
            // Create project instance from the serialized (compressed)
            // XML that was saved with the task and set action type from task
            var dsConfig = task.DataSourceConfiguration;
            var projectSettings = ProjectSettingsSerializer.ImportFromXmlString(task.SettingsXml, dsConfig, false, out _);
            if (task.ActionType != ActionType.Unknown) {
                projectSettings.ActionType = task.ActionType;
            }
            projectSettings.ProjectDataSourceVersions = dsConfig.ToVersionsDictionary();

            // Create compiled data manager
            var compiledDataManager = new CompiledDataManager(_rawDataProviderFactory(projectSettings, dsConfig));
            return (projectSettings, compiledDataManager);
        }

        /// <summary>
        /// Collect outputs of loop task.
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        public List<IOutput> CollectSubTaskOutputs(ITask task) => new List<IOutput>();

        /// <summary>
        /// Load compiled data manager from output raw-data.
        /// </summary>
        /// <param name="idOutput"></param>
        /// <returns></returns>
        public ICompiledDataManager GetOutputCompiledDataManager(int idOutput) => null;
    }
}
