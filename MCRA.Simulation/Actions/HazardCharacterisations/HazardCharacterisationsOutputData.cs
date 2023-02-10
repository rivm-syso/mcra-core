
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Action;
using MCRA.Simulation.Calculators.HazardCharacterisationCalculation;

namespace MCRA.Simulation.Actions.HazardCharacterisations {
    public class HazardCharacterisationsOutputData : IModuleOutputData {
        public TargetUnit HazardCharacterisationsUnit { get; set; }
        public IDictionary<Compound, IHazardCharacterisationModel> HazardCharacterisations { get; set; }
        public IModuleOutputData Copy() {
            return new HazardCharacterisationsOutputData() {
                HazardCharacterisationsUnit = HazardCharacterisationsUnit,
                HazardCharacterisations = HazardCharacterisations,
            };
        }
    }
}

