using System.Collections.Generic;

namespace MCRA.General {

    public interface ITask {

        /// <summary>
        /// The id of the task.
        /// </summary>
        int id { get; }

        /// <summary>
        /// The output id
        /// </summary>
        int? idOutput { get; }

        /// <summary>
        /// The current status of the task.
        /// </summary>
        MCRATaskStatus Status { get; }

        /// <summary>
        /// The action type of the task
        /// </summary>
        ActionType ActionType { get; }

        /// <summary>
        /// Any subtasks of this task
        /// </summary>
        ICollection<ITask> ChildTasks { get; }

        /// <summary>
        /// The raw settings XML string
        /// </summary>
        string SettingsXml { get; }

        /// <summary>
        /// The data source configuration
        /// </summary>
        DataSourceConfiguration DataSourceConfiguration { get; }

    }
}
