using MCRA.General;

namespace MCRA.Data.Compiled.Objects {
    public sealed class ConsumerProductConcentration {
        public ConsumerProduct Product { get; set; }
        public Compound Substance { get; set; }
        public double Concentration { get; set; }
        public ConcentrationUnit Unit { get; set; } = ConcentrationUnit.ugPerKg;
        public double? SamplingWeight { get; set; }
    }
}
