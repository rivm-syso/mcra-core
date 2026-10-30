namespace MCRA.Utils.Sbml.Simulation;

public sealed class SbmlSimulationResult {
    public double[] Time { get; set; }
    public Dictionary<string, double[]> OutputTimeSeries { get; set; }
    public Dictionary<string, double> OutputStates { get; set; }
}
