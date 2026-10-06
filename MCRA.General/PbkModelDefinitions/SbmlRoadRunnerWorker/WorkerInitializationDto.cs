namespace MCRA.General.PbkModelDefinitions.SbmlRoadRunnerWorker {
    public sealed class WorkerInitializationDto {
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
        public Dictionary<string, string> RouteInputMappings { get; set; } = new();
        public string BodyWeightParameterId { get; set; }
        public int MaximumNumSteps { get; set; } = 1000000;
        /// <summary>
        /// A serializable snapshot of the PBK model specification (<see
        /// cref="PbkModelSpecifications.Sbml.ISbmlPbkModelSpecification"/>)
        /// of the kinetic model being simulated, so the out-of-process worker has access to the same
        /// model metadata (e.g. parameter types, lifetime-model status) as the in-process runner.
        /// </summary>
        public SbmlPbkModelSpecificationDto PbkModelSpecification { get; set; }
    }
}
