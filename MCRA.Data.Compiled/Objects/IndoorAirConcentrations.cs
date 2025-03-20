using MCRA.General;

namespace MCRA.Data.Compiled.Objects {
    public sealed class IndoorAirConcentration {
        public string idSample { get; set; }
        public Compound Substance { get; set; }
        public string Location { get; set; }
        public double Concentration { get; set; }
        public AirConcentrationUnit AirConcentrationUnit { get; set; } = AirConcentrationUnit.ugPerm3;
    }
}
