using MCRA.General;

namespace MCRA.Data.Compiled.Objects {
    public sealed class OutdoorAirConcentration {
        public string idSample { get; set; }
        public Compound Substance { get; set; }
        public string Location { get; set; }
        public double Concentration { get; set; }
        public AirConcentrationUnit Unit { get; set; } = AirConcentrationUnit.ugPerm3;
    }
}
