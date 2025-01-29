using MCRA.General;

namespace MCRA.Data.Compiled.Objects {
    public sealed class SoilConcentrationDistribution {
        public string idSample { get; set; }
        public Compound Substance { get; set; }
        public double Concentration { get; set; }
        public ConcentrationUnit ConcentrationUnit { get; set; } = ConcentrationUnit.ugPerg;
    }
}
