using MCRA.General;

namespace MCRA.Simulation.Calculators.AdjustmentFactorCalculation {
    public class AdjustmentFactorModelFactory {

        private IAdjustmentFactorModelFactorySettings _settings;
        public AdjustmentFactorModelFactory(IAdjustmentFactorModelFactorySettings settings) {
            _settings = settings;
        }

        public  AdjustmentFactorModelBase Create(
        ) {

            switch (_settings.AdjustmentFactorDistributionMethod) {
                case AdjustmentFactorDistributionMethod.None:
                    return new AFFixedModel(1);
                case AdjustmentFactorDistributionMethod.Fixed:
                    return new AFFixedModel(_settings.ParameterA);
                case AdjustmentFactorDistributionMethod.LogNormal:
                    return new AFLognormalModel(_settings.ParameterA, _settings.ParameterB, _settings.ParameterC);
                case AdjustmentFactorDistributionMethod.LogStudents_t:
                    return new AFLogStudentTModel(_settings.ParameterA, _settings.ParameterB, _settings.ParameterC, _settings.ParameterD);
                case AdjustmentFactorDistributionMethod.Beta:
                    return new AFBetaModel(_settings.ParameterA, _settings.ParameterB, _settings.ParameterC, _settings.ParameterD);
                case AdjustmentFactorDistributionMethod.Gamma:
                    return new AFGammaModel(_settings.ParameterA, _settings.ParameterB, _settings.ParameterC);
                default:
                    throw new Exception("Model not implemented");
            }
        }
    }
}
