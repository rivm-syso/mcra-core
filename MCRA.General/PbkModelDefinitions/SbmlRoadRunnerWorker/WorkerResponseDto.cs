namespace MCRA.General.PbkModelDefinitions.SbmlRoadRunnerWorker {
    /// <summary>
    /// Response envelope for the out-of-process RoadRunner worker's "/initialize" and "/simulate"
    /// endpoints. For "/simulate" (<c>Type == "result"</c>), <see cref="Results"/> holds one entry per
    /// job in the submitted <see cref="SimulationJobBatch"/>, in the same order.
    /// </summary>
    public sealed class WorkerResponseDto {
        public string Type { get; set; }
        public int ProtocolVersion { get; set; }
        /// <summary>Used for "error" responses that relate to a single job rather than the whole batch.</summary>
        public string JobId { get; set; }
        public List<WorkerJobResultDto> Results { get; set; }
        public string Error { get; set; }
    }
}
