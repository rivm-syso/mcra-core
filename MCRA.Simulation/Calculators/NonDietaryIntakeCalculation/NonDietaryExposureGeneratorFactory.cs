namespace MCRA.Simulation.Calculators.NonDietaryIntakeCalculation {
    public  class NonDietaryExposureGeneratorFactory {
        private INonDietaryExposureGeneratorFactorySettings _settings;
        public NonDietaryExposureGeneratorFactory(INonDietaryExposureGeneratorFactorySettings settings) {
            _settings = settings;
        }

        public  NonDietaryExposureGenerator Create() {
            if (_settings.MatchSpecificIndividuals) {
                return new NonDietaryMatchedExposureGenerator();
            } else if (!_settings.MatchSpecificIndividuals && !_settings.IsCorrelationBetweenIndividuals) {
                return new NonDietaryUnmatchedExposureGenerator();
            } else {
                return new NonDietaryUnmatchedCorrelatedExposureGenerator();
            }
        }
    }
}
