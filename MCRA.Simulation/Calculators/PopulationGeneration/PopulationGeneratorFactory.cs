using MCRA.General;

namespace MCRA.Simulation.Calculators.PopulationGeneration {
    public  class PopulationGeneratorFactory {
        private IPopulationGeneratorFactorySettings _settings;

        public PopulationGeneratorFactory(IPopulationGeneratorFactorySettings settings) {
            _settings = settings;
        }
        public PopulationGeneratorBase Create() {
            if (_settings.ExposureType == ExposureType.Acute) {
                return new AcutePopulationGenerator(_settings.IsSurveySampling, _settings.NumberOfSimulatedIndividualDays);
            } else {
                return new ChronicPopulationGenerator();
            }
        }
    }
}
