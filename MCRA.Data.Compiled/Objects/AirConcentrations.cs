using MCRA.Data.Compiled.Interfaces;
using MCRA.General;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Data.Compiled.Objects {
    public sealed class AirConcentration : ISubstanceConcentration {
        public string idSample { get; set; }
        public Compound Substance { get; set; }
        public string Location { get; set; }
        public double Concentration { get; set; }
        public AirConcentrationUnit Unit { get; set; } = AirConcentrationUnit.ugPerm3;

        public string UnitTypeString => Unit.GetShortDisplayName();
    }
}
