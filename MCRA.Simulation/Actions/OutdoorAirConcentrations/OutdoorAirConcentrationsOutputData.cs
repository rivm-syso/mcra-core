
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Action;

namespace MCRA.Simulation.Actions.OutdoorAirConcentrations {
    public class OutdoorAirConcentrationsOutputData : IModuleOutputData {
        public IList<OutdoorAirConcentration> AirConcentrations { get; set; }
        public AirConcentrationUnit AirConcentrationUnit { get; set; }
        public IModuleOutputData Copy() {
            return new OutdoorAirConcentrationsOutputData() {
                AirConcentrations = AirConcentrations,
                AirConcentrationUnit = AirConcentrationUnit
            };
        }
    }
}

