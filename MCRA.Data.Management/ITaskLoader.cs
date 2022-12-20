using MCRA.General;
using MCRA.General.Action.Settings.Dto;
using System.Collections.Generic;

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
        /// Load compiled data manager from output raw-data.
        /// </summary>
        /// <param name="idOutput"></param>
        /// <returns></returns>
        ICompiledDataManager GetOutputCompiledDataManager(int idOutput);

    }
}
