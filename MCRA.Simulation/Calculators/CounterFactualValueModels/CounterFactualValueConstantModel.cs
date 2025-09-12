using MCRA.Data.Compiled.Objects;

namespace MCRA.Simulation.Calculators.CounterFactualValueModels {

    public sealed class CounterFactualValueConstantModel(
        ExposureResponseFunction erf
    ) : CounterFactualValueModelBase<CounterFactualValueModelParametrisation>(erf) {
        protected override CounterFactualValueModelParametrisation getParametrisation(
            double factor,
            double? upper
        ) {
            var result = new CounterFactualValueModelParametrisation() {
                Factor = factor
            };
            return result;
        }
    }
}
