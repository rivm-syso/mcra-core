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

        // TODO: this was added for the consumer product concentrations, why only for consumer products and not for soil, dust, etc?
        public double? SamplingWeight { get; set; }

    }
}
