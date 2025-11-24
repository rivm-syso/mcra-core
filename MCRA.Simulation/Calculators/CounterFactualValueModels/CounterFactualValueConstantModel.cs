using MCRA.Data.Compiled.Objects;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.CounterFactualValueModels {

    public sealed class CounterFactualValueConstantModel : ICounterFactualValueModel {

        public ExposureResponseFunction ExposureResponseFunction { get; set; }

        public CounterFactualValueConstantModel(ExposureResponseFunction erf) {
            ExposureResponseFunction = erf;
        }

        public void CalculateParameters() {
            // Nothing to do here
        }

        public void ResampleModelParameters(IRandom random) {
            // Nothing to do here
        }

        public double GetCounterFactualValue() {
            return ExposureResponseFunction.CounterFactualValue;
        }
    }
}
