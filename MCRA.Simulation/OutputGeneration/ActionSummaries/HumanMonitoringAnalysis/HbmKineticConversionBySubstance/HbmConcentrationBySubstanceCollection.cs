using MCRA.Data.Compiled.Objects;
using MCRA.General;

namespace MCRA.Simulation.OutputGeneration;

public sealed class HbmConcentrationBySubstanceCollection {
    public ExposureTarget ExposureTarget { get; set; }
    public TargetUnit TargetUnit { get; set; }
    public int Id { get; set; }
    public List<(Compound Substance, double Concentration)> ConcentrationsBySubstance { get; set; }
}
