using System;

namespace MCRA.General {

    /// <summary>
    /// Data contract for sending progress reports of tasks send to the job scheduler.
    /// </summary>
    public interface IOutput {

        /// <summary>
        /// The id of the output.
        /// </summary>
        int id { get; }

        /// <summary>
        /// The id of the task.
        /// </summary>
        int idTask { get; }

        /// <summary>
        /// Description of the output
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Start of execution of task
        /// </summary>
        DateTime StartExecution { get; }

        /// <summary>
        /// Build date/time of version of MCRA
        /// </summary>
        DateTime BuildDate { get; }

        /// <summary>
        /// Version of MCRA this output was generated with
        /// </summary>
        string BuildVersion { get; }

        /// <summary>
        /// Created date of the output
        /// </summary>
        DateTime DateCreated { get; set; }

        /// <summary>
        /// Section header data (TOC) as a byte array of compressed xml
        /// </summary>
        byte[] SectionHeaderData { get; set; }

        /// <summary>
        /// Output summary data as compressed HTML string
        /// </summary>
        byte[] OutputSummary { get; set; }
    }
}
