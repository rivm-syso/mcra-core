using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Action;
using MCRA.Simulation.Objects.SubstanceConcentrations;

namespace MCRA.Simulation.Actions.SoilConcentrations {
    public class SoilConcentrationsOutputData : IModuleOutputData {
        public SubstanceConcentrationsCollection SoilConcentrationCollection { get; set; }
        public IModuleOutputData Copy() {
            return new SoilConcentrationsOutputData() {
                SoilConcentrationCollection = SoilConcentrationCollection
            };
        }
    }
}

