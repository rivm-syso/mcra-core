using MCRA.Data.Compiled.Objects;
using MCRA.General;

namespace MCRA.Simulation.Calculators.ConsumerProductExposureFractionCalculation {
    public sealed class ConsumerProductExposureFractionSGs {
        public ConsumerProduct Product { get; set; }
        public Compound Substance { get; set; }
        public ExposureRoute Route { get; set; }
        public double? ExposureFraction { get; set; }
        public ICollection<ConsumerProductExposureFraction> CPEFSubgroups { get; set; }
    }
}
