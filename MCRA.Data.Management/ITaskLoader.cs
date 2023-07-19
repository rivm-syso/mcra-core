using MCRA.General;
using MCRA.General.Action.Settings;

namespace MCRA.Data.Management {
    public interface ITaskLoader {

        /// <summary>
        /// Load project and compiled data manager for simulation task.
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        (ProjectDto, ICompiledDataManager) Load(ITask task);

        /// <summary>
        /// Collect outputs of loop task.
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        List<IOutput> CollectSubTaskOutputs(ITask task);

        /// <summary>
        /// Returns the compiled data managers for the raw data, per (sub)action, that was generated for specified output.
        /// </summary>
        /// <param name="idOutput">Identifier of an action output.</param>
        Dictionary<ActionType?, ICompiledDataManager> GetOutputCompiledDataManagers(int idOutput);
    }
}
