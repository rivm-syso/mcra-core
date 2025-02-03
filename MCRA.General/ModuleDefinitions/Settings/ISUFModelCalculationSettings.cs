using MCRA.General.ModuleDefinitions.Interfaces;

namespace MCRA.General.ModuleDefinitions.Settings {
    public class ISUFModelCalculationSettings : IISUFModelCalculationSettings {

        private readonly int _gridPrecision;
        private readonly int _numberOfIterations;
        private readonly bool _isSplineFit;

        public ISUFModelCalculationSettings(
            int gridPrecision,
            int numberOfIterations,
            bool isSplineFit
        ) {
            _gridPrecision = gridPrecision;
            _numberOfIterations = numberOfIterations;
            _isSplineFit = isSplineFit;
        }

        public int GridPrecision => _gridPrecision;

        public int NumberOfIterations => _numberOfIterations;

        public bool IsSplineFit => _isSplineFit;
    }
}
