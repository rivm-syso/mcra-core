using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Calculators.IntakeModelling.IntakeModels.ISUFCalculator;

namespace MCRA.Simulation.Calculators.IntakeModelling.IntakeModels {
    public class ISUFModelCalculationSettings : IISUFModelCalculationSettings {

        private readonly DietaryExposuresModuleConfig _dietaryConfig;

        public ISUFModelCalculationSettings(DietaryExposuresModuleConfig dietaryConfig) {
            _dietaryConfig = dietaryConfig;
        }
        public int GridPrecision => _dietaryConfig.IsufModelGridPrecision;

        public int NumberOfIterations => _dietaryConfig.IsufModelNumberOfIterations;

        public bool IsSplineFit => _dietaryConfig.IsufModelSplineFit;
    }
}
