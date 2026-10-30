namespace MCRA.Utils.Sbml.BatchSimulation.OutOfProcess.Dto {
    public sealed class SbmlSimulationResultDto {
        public double[] Time { get; set; }
        public Dictionary<string, double[]> OutputTimeSeries { get; set; }
        public Dictionary<string, double> OutputStates { get; set; }
    }
}
