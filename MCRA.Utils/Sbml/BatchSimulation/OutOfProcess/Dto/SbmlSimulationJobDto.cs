namespace MCRA.Utils.Sbml.BatchSimulation.OutOfProcess.Dto {
    /// <summary>
    /// A single simulation job (e.g. for one individual) within a <see cref="SimulationJobBatchDto"/>.
    /// </summary>
    public sealed class SbmlSimulationJobDto {
        public string JobId { get; set; }
        public Dictionary<string, double> Parameters { get; set; }
        public List<SbmlSimulationEventDto> Events { get; set; } = [];
        public List<string> OutputStates { get; set; } = [];
        public List<string> OutputTimeSeries { get; set; } = [];
        public int Duration { get; set; }
        public int Steps { get; set; }
    }
}
