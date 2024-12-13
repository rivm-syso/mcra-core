using MCRA.General;

namespace MCRA.Simulation.Calculators.AdjustmentFactorCalculation {
    public class AdjustmentFactorModelFactory {

        private IAdjustmentFactorModelFactorySettings _settings;
        public AdjustmentFactorModelFactory(IAdjustmentFactorModelFactorySettings settings) {
            _settings = settings;
        }

        public  AdjustmentFactorModelBase Create(
        ) {

            return _settings.AdjustmentFactorDistributionMethod switch {
                AdjustmentFactorDistributionMethod.None => new AFFixedModel(1),
                AdjustmentFactorDistributionMethod.Fixed => new AFFixedModel(_settings.ParameterA),
                AdjustmentFactorDistributionMethod.LogNormal => new AFLognormalModel(_settings.ParameterA, _settings.ParameterB, _settings.ParameterC),
                AdjustmentFactorDistributionMethod.LogStudents_t => new AFLogStudentTModel(_settings.ParameterA, _settings.ParameterB, _settings.ParameterC, _settings.ParameterD),
                AdjustmentFactorDistributionMethod.Beta => new AFBetaModel(_settings.ParameterA, _settings.ParameterB, _settings.ParameterC, _settings.ParameterD),
                AdjustmentFactorDistributionMethod.Gamma => new AFGammaModel(_settings.ParameterA, _settings.ParameterB, _settings.ParameterC),
                _ => throw new Exception("Model not implemented"),
            };
        }
    }
}
