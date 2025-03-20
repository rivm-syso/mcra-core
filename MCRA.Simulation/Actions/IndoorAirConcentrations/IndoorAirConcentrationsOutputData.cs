
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Action;

namespace MCRA.Simulation.Actions.IndoorAirConcentrations {
    public class IndoorAirConcentrationsOutputData : IModuleOutputData {
        public IList<IndoorAirConcentration> AirConcentrations { get; set; }
        public AirConcentrationUnit AirAirConcentrationUnit { get; set; }
        public IModuleOutputData Copy() {
            return new IndoorAirConcentrationsOutputData() {
                AirConcentrations = AirConcentrations,
                AirAirConcentrationUnit = AirAirConcentrationUnit
            };
        }
    }
}

