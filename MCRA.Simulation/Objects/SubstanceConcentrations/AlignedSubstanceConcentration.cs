using MCRA.Data.Compiled.Interfaces;
using MCRA.Data.Compiled.Objects;

namespace MCRA.Simulation.Objects.SubstanceConcentrations {
    /// <summary>
    /// Represents the concentration of a specific substance measured in a sample, where the concentration 
    /// value has been aligned to a common unit.
    /// </summary>
    public class AlignedSubstanceConcentration : ISubstanceConcentration {
        public string SampleId { get; init; }
        public Compound Substance { get; init; }
        public double Concentration { get; init; }
    }
}
