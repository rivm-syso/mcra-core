using MCRA.Utils.Sbml.Simulation;

namespace MCRA.Utils.Sbml.BatchSimulation;

public sealed class SbmlSimulationJobResult {
    public string JobId { get; set; }
    public SbmlSimulationResult Result { get; set; }
    public string Error { get; set; }
}
