namespace MCRA.Simulation.Calculators.PbkModelCalculation.SbmlModelCalculation;

public sealed class RoadRunnerSimulationJob {
    public string JobId { get; set; }
    public Dictionary<string, double> Parameters { get; set; } = new();
    public List<RoadRunnerExposureEvent> Events { get; set; } = new();
    public List<string> OutputStates { get; set; } = new();
    public List<string> OutputTimeSeries { get; set; } = new();
    public int Duration { get; set; }
    public int Steps { get; set; }
    public bool ApplyBodyWeightScaling { get; set; }
}
