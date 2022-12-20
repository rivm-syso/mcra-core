namespace MCRA.General {
    /// <summary>
    /// Holds a simulation tasks that is to be executed.
    /// </summary>
    public class SimulationTask {

        /// <summary>
        /// The task id.
        /// </summary>
        public int idTask { get; set; }

        /// <summary>
        /// The id of the project for which this task is to be executed.
        /// </summary>
        public int idProject { get; set; }

        /// <summary>
        /// The type of the task.
        /// </summary>
        public MCRATaskType TaskType { get; set; }

        /// <summary>
        /// The task that is to be executed.
        /// </summary>
        public Task Task { get; set; }

        /// <summary>
        /// The cancel source that can be used to abort/cancel the task.
        /// </summary>
        public CancellationTokenSource CancelSource { get; set; }
    }
}
