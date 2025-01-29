using MCRA.General.Action.Settings;
using MCRA.General.ModuleDefinitions.Interfaces;

namespace MCRA.General.ModuleDefinitions.Settings {
    public partial class ConcentrationModelsModuleConfig : IConcentrationModelCalculationSettings {
        public ICollection<ConcentrationModelTypeFoodSubstance> ConcentrationModelTypesPerFoodCompound => ConcentrationModelTypesFoodSubstance;
    }
}
