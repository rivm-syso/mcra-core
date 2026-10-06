namespace MCRA.General.PbkModelDefinitions.SbmlRoadRunnerWorker {
    /// <summary>
    /// A single simulation job (e.g. for one individual) within a <see cref="SimulationJobBatch"/>.
    /// </summary>
    public sealed class SimulationJobDto {
        public string JobId { get; set; }
        public Dictionary<string, double> Parameters { get; set; }
        public List<ExposureEventDto> Events { get; set; } = new();
        public List<string> OutputStates { get; set; } = new();
        public List<string> OutputTimeSeries { get; set; } = new();
        public int Duration { get; set; }
        public int Steps { get; set; }
        public bool ApplyBodyWeightScaling { get; set; }
    }
}
