namespace MCRA.General.PbkModelDefinitions.SbmlRoadRunnerWorker {
    public sealed class WorkerSimulationResultDto {
        public double[] Time { get; set; }
        public Dictionary<string, double[]> OutputTimeSeries { get; set; }
        public Dictionary<string, double> OutputStates { get; set; }
    }
}
