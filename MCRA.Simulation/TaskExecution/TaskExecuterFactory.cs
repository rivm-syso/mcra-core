using MCRA.Data.Management;
using MCRA.General;
using MCRA.Simulation.OutputManagement;
using MCRA.Simulation.TaskExecution.TaskExecuters;
using System;

namespace MCRA.Simulation.TaskExecution {

    /// <summary>
    /// Creates a job executer instance for the given task type.
    /// </summary>
    public static class TaskExecuterFactory {
        public static TaskExecuterBase CreateTaskExecuter(
            MCRATaskType taskType,
            ITaskLoader taskLoader,
            IOutputManager outputManager,
            string log4netConfigFile
        ) {
            switch (taskType) {
                case MCRATaskType.Simulation:
                    return new SimulationTaskExecuter(
                        taskLoader,
                        outputManager,
                        log4netConfigFile
                    );
                case MCRATaskType.LoopCalculation:
                    return new LoopCalculationTaskExecuter(
                        taskLoader,
                        outputManager,
                        log4netConfigFile
                    );
                default:
                    throw new Exception($"Cannot start jobs of type {taskType}");
            }
        }
    }
}
