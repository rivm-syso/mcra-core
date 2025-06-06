using MCRA.Data.Compiled.Objects;

namespace MCRA.Simulation.Objects.ConsumerProductCollection {
    public sealed class ConsumerProductCollection {
        public ConsumerProduct ConsumerProduct { get; set; }
        public Dictionary<Compound, List<ConsumerProductConcentration>> SubstanceSampleCollection { get; set; }

    }
}
