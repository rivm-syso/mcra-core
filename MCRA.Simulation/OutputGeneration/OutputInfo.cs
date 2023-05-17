namespace MCRA.Simulation.OutputGeneration {
    public class OutputInfo {

        /// <summary>
        /// Description of the output.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Description of the output.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// DateCreated
        /// </summary>
        public DateTime DateCreated { get; set; }

        /// <summary>
        /// The version of MCRA this output was generated with
        /// </summary>
        public string BuildVersion { get; set; }

        /// <summary>
        /// BuildDate
        /// </summary>
        public DateTime? BuildDate { get; set; }

        /// <summary>
        /// ExecutionTime
        /// </summary>
        public string ExecutionTime { get; set; }

        /// <summary>
        /// Table of contents
        /// </summary>
        public SummaryToc SummaryToc { get; set; }
    }
}
