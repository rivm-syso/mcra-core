using MCRA.Data.Compiled.Objects;

namespace MCRA.Simulation.Calculators.ConsumerProductExposureCalculation {
    public sealed class ConsumerProductSampleSubstanceCollections {
        public ConsumerProduct ConsumerProduct { get; set; }
        public Dictionary<Compound, List<ConsumerProductConcentration>> SubstanceSampleCollection { get; set; }
    }
}
