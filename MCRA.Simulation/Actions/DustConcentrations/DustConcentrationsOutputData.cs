
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Action;

namespace MCRA.Simulation.Actions.DustConcentrations {
    public class DustConcentrationsOutputData : IModuleOutputData {
        public IList<DustConcentration> DustConcentrations { get; set; }
        public ConcentrationUnit DustConcentrationUnit { get; set; }
        public IModuleOutputData Copy() {
            return new DustConcentrationsOutputData() {
                DustConcentrations = DustConcentrations,
                DustConcentrationUnit = DustConcentrationUnit
            };
        }
    }
}

