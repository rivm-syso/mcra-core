namespace MCRA.General.PbkModelDefinitions.SbmlRoadRunnerWorker {
    /// <summary>The result (or per-job error) of a single job within a batch "/simulate" response.</summary>
    public sealed class WorkerJobResultDto {
        public string JobId { get; set; }
        public WorkerSimulationResultDto Result { get; set; }
        public string Error { get; set; }
    }
}
