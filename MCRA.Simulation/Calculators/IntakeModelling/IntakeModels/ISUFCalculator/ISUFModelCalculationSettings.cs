using MCRA.General.Action.Settings;
using MCRA.Simulation.Calculators.IntakeModelling.IntakeModels.ISUFCalculator;

namespace MCRA.Simulation.Calculators.IntakeModelling.IntakeModels {
    public class ISUFModelCalculationSettings : IISUFModelCalculationSettings {

        private IntakeModelSettings _isufModelSettings;

        public ISUFModelCalculationSettings(IntakeModelSettings isufModelSettings) {
            _isufModelSettings = isufModelSettings;
        }
        public int GridPrecision => _isufModelSettings.GridPrecision;

        public int NumberOfIterations => _isufModelSettings.NumberOfIterations;

        public bool IsSplineFit => _isufModelSettings.SplineFit;
    }
}
