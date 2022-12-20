using MCRA.General;
using MCRA.General.Action.Serialization;
using MCRA.General.Action.Settings.Dto;
using System.Collections.Generic;

namespace MCRA.Data.Management {
    public class TaskData : ITask {
        public int id { get; set; }

        public int? idOutput { get; set; }

        public MCRATaskStatus Status { get; set; }

        public ActionType ActionType { get; set; }

        public ICollection<ITask> ChildTasks { get; set; } = new HashSet<ITask>();

        public string SettingsXml { get; set; }

        public DataSourceConfiguration DataSourceConfiguration { get; set; }

        /// <summary>
        /// Extract task's original settings and data to a <see cref="ProjectDto"/> object
        /// </summary>
        /// <param name="task">Task to extract settings from</param>
        /// <returns></returns>
        public static ProjectDto GetProjectSettings(ITask task) {
            // Create project instance from the serialized (compressed)
            // XML that was saved with the task and set action type from task
            var dsConfig = task.DataSourceConfiguration;
            var projectSettings = ProjectSettingsSerializer.ImportFromXmlString(task.SettingsXml, dsConfig, false, out _);
            if (task.ActionType != ActionType.Unknown) {
                projectSettings.ActionType = task.ActionType;
            }
            projectSettings.ProjectDataSourceVersions = dsConfig.ToVersionsDictionary();
            return projectSettings;
        }
    }
}
