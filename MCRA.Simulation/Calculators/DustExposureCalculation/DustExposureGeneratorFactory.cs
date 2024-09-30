namespace MCRA.Simulation.Calculators.DustExposureCalculation {
    public class DustExposureGeneratorFactory {
        private IDustExposureGeneratorFactorySettings _settings;
        public DustExposureGeneratorFactory(IDustExposureGeneratorFactorySettings settings) {
            _settings = settings;
        }

        public DustExposureGenerator Create() {
            if (_settings.MatchSpecificIndividuals) {
                return new DustMatchedExposureGenerator();
            } else if (!_settings.MatchSpecificIndividuals && !_settings.IsCorrelationBetweenIndividuals) {
                return new DustUnmatchedExposureGenerator();
            } else {
                throw new NotImplementedException();
                //return new DustUnmatchedCorrelatedExposureGenerator();
            }
        }
    }
}
