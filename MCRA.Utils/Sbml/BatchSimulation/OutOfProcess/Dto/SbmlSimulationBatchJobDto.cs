namespace MCRA.Utils.Sbml.BatchSimulation.OutOfProcess.Dto {
    /// <summary>
    /// A batch of <see cref="SbmlSimulationJobDto"/>s submitted to the out-of-process RoadRunner worker in a
    /// single "/simulate" request, so that a whole set of individuals is run against the worker's
    /// already-loaded model in one call instead of one call per individual.
    /// </summary>
    public sealed class SbmlSimulationBatchJobDto {
        public string Type { get; set; }
        public List<SbmlSimulationJobDto> Jobs { get; set; } = [];
    }
}
