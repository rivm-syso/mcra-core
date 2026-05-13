using MCRA.Data.Compiled.Interfaces;
using MCRA.General;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Data.Compiled.Objects {
    public sealed class ConsumerProductConcentration : ISubstanceConcentration {
        public ConsumerProduct Product { get; set; }
        public Compound Substance { get; set; }
        public double Concentration { get; set; }
        public ConsumerProductConcentrationUnit Unit { get; set; }
        public double? SamplingWeight { get; set; }
    }
}
