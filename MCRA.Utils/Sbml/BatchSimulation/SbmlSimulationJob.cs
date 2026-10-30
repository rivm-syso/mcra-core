using MCRA.Utils.Sbml.Simulation;

namespace MCRA.Utils.Sbml.BatchSimulation;

public sealed class SbmlSimulationJob {
    public string JobId { get; set; }
    public Dictionary<string, double> Parameters { get; set; } = [];
    public List<SbmlSimulationEvent> Events { get; set; } = [];
    public List<string> OutputStates { get; set; } = [];
    public List<string> OutputTimeSeries { get; set; } = [];
    public int Duration { get; set; }
    public int Steps { get; set; }
}
