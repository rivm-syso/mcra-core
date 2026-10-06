namespace MCRA.General.PbkModelDefinitions.SbmlRoadRunnerWorker {
    /// <summary>
    /// A batch of <see cref="SimulationJobDto"/>s submitted to the out-of-process RoadRunner worker in a
    /// single "/simulate" request, so that a whole set of individuals is run against the worker's
    /// already-loaded model in one call instead of one call per individual.
    /// </summary>
    public sealed class SimulationJobBatch {
        public string Type { get; set; }
        public List<SimulationJobDto> Jobs { get; set; } = new();
    }
}
