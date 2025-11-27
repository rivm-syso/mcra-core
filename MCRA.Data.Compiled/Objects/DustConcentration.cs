using MCRA.General;

namespace MCRA.Data.Compiled.Objects {
    public sealed class DustConcentration {
        public string idSample { get; set; }
        public Compound Substance { get; set; }
        public double Concentration { get; set; }
        public ConcentrationUnit Unit { get; set; } = ConcentrationUnit.ugPerg;
    }
}
