namespace MCRA.Utils.Sbml.BatchSimulation.OutOfProcess.Dto {

    /// <summary>
    /// The result (or per-job error) of a single job within a batch "simulate" response.
    /// </summary>
    public sealed class WorkerJobResultDto {
        public string JobId { get; set; }
        public SbmlSimulationResultDto Result { get; set; }
        public string Error { get; set; }
    }
}
