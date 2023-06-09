using MCRA.Utils.ProgressReporting;
using MCRA.General;

namespace MCRA.Simulation.TaskExecution {
    public interface ITaskExecuter {
        TaskExecutionResult Run(ITask task, CompositeProgressState progressReport);
    }
}