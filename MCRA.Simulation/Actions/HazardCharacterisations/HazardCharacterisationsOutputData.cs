using MCRA.Simulation.Action;
using MCRA.Simulation.Calculators.HazardCharacterisationCalculation;

namespace MCRA.Simulation.Actions.HazardCharacterisations {
    public class HazardCharacterisationsOutputData : IModuleOutputData {
        public ICollection<HazardCharacterisationModelCompoundsCollection> HazardCharacterisationModelsCollections { get; set; }
        public IModuleOutputData Copy() {
            return new HazardCharacterisationsOutputData() {
                HazardCharacterisationModelsCollections = HazardCharacterisationModelsCollections
            };
        }
    }
}
