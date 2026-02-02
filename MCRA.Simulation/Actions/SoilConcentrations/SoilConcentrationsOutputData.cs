using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Action;

namespace MCRA.Simulation.Actions.SoilConcentrations {
    public class SoilConcentrationsOutputData : IModuleOutputData {
        public IList<SubstanceConcentration> SoilConcentrations { get; set; }
        public ConcentrationUnit SoilConcentrationUnit { get; set; }
        public IModuleOutputData Copy() {
            return new SoilConcentrationsOutputData() {
                SoilConcentrations = SoilConcentrations,
                SoilConcentrationUnit = SoilConcentrationUnit
            };
        }
    }
}

