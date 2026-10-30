namespace MCRA.Utils.Sbml.BatchSimulation.OutOfProcess.Dto {
    public sealed class SbmlSimulationInitializationDto {

        public string Type { get; set; }

        /// <summary>
        /// The original full path of the SBML model file, as it existed on the machine that sent
        /// this initialization request. The out-of-process worker does not necessarily share a file
        /// system with the sender (and the path may not exist there), so this value is informational
        /// only (e.g. for diagnostics/logging) and must not be used to read the model from disk; use
        /// <see cref="ModelFileContent"/> instead.
        /// </summary>
        public string ModelFileName { get; set; }

        /// <summary>
        /// The full XML content of the SBML model file identified by <see cref="ModelFileName"/>,
        /// transferred inline so the worker can load the model without needing file system access.
        /// </summary>
        public string ModelFileContent { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of steps to execute.
        /// </summary>
        /// <remarks>The default value is 1,000,000.</remarks>
        public int MaximumNumSteps { get; set; } = 1000000;

    }
}
