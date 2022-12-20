using System.Runtime.Serialization;

namespace MCRA.General {

    /// <summary>
    /// Data contract for sending progress reports of tasks send to the job scheduler.
    /// </summary>
    [DataContract]
    public sealed class MCRATaskReport {

        /// <summary>
        /// The id of the task.
        /// </summary>
        [DataMember]
        public string taskGuid { get; set; }

        /// <summary>
        /// The id of the task.
        /// </summary>
        [DataMember]
        public int idTask { get; set; }

        /// <summary>
        /// The id of the project to which the task belongs.
        /// </summary>
        [DataMember]
        public int idProject { get; set; }

        /// <summary>
        /// The current status of the task.
        /// </summary>
        [DataMember]
        public MCRATaskStatus Status { get; set; }

        /// <summary>
        /// The current progress of the task.
        /// </summary>
        [DataMember]
        public double Progress { get; set; }

        /// <summary>
        /// The current activity of the task.
        /// </summary>
        [DataMember]
        public string CurrentActivity { get; set; }

        /// <summary>
        /// If applicable, the error message thrown by the task.
        /// </summary>
        [DataMember]
        public string Error { get; set; }

        /// <summary>
        /// Returns true if the task is finished, which is either if it ran to completion,
        /// was canceled, of faulted.
        /// </summary>
        /// <returns></returns>
        public bool TaskFinished() {
            return Status == MCRATaskStatus.Canceled
                || Status == MCRATaskStatus.Faulted
                || Status == MCRATaskStatus.RanToCompletion;
        }
    }
}
