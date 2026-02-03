
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Action;

namespace MCRA.Simulation.Actions.IndoorAirConcentrations {
    public class IndoorAirConcentrationsOutputData : IModuleOutputData {
        public IList<AirConcentration> AirConcentrations { get; set; }
        public AirConcentrationUnit AirConcentrationUnit { get; set; }
        public IModuleOutputData Copy() {
            return new IndoorAirConcentrationsOutputData() {
                AirConcentrations = AirConcentrations,
                AirConcentrationUnit = AirConcentrationUnit
            };
        }
    }
}

