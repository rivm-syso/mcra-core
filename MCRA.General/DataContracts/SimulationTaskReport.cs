
namespace MCRA.General {
    /// <summary>
    /// Data contract for sending progress reports of simulation tasks.
    /// </summary>
    public class SimulationTaskReport {
        /// <summary>
        /// The task id.
        /// </summary>
        public int idTask { get; set; }

        /// <summary>
        /// The task status.
        /// </summary>
        public MCRATaskStatus Status { get; set; }

        /// <summary>
        /// The progress (in percentages).
        /// </summary>
        public double Progress { get; set; }

        /// <summary>
        /// The current activity of the task.
        /// </summary>
        public string CurrentActivity { get; set; }

        /// <summary>
        /// If applicable, the error of the task.
        /// </summary>
        public string Error { get; set; }
    }
}
