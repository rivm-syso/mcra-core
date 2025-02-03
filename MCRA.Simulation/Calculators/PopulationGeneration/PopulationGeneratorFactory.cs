using MCRA.General;

namespace MCRA.Simulation.Calculators.PopulationGeneration {
    public  class PopulationGeneratorFactory {

        private ExposureType _exposureType;
        private bool _isSurveySampling;
        private int _numberOfSimulatedIndividualDays;

        public PopulationGeneratorFactory(
            ExposureType exposureType,
            bool isSurveySampling,
            int numberOfSimulatedIndividualDays
        ) {
            _exposureType = exposureType;
            _isSurveySampling = isSurveySampling;
            _numberOfSimulatedIndividualDays = numberOfSimulatedIndividualDays;
        }
        public PopulationGeneratorBase Create() {
            if (_exposureType == ExposureType.Acute) {
                return new AcutePopulationGenerator(_isSurveySampling, _numberOfSimulatedIndividualDays);
            } else {
                return new ChronicPopulationGenerator();
            }
        }
    }
}
