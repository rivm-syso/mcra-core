using MCRA.Data.Compiled.Objects;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.CounterFactualValueModels {

    public class CounterFactualValueDistributionModelParametrisation<T>
        : CounterFactualValueModelParametrisation where T : Distribution {
        public T Distribution { get; set; }
    }

    public abstract class CounterFactualValueDistributionModel<T>(
        ExposureResponseFunction erf
    ) : CounterFactualValueModelBase<CounterFactualValueDistributionModelParametrisation<T>>(
        erf
    ) where T : Distribution {

        public override void ResampleModelParameters(IRandom random) {
            var rnd = new McraRandomGenerator(random.Next());
            foreach (var parametrisation in ModelParametrisations) {
                // Correlated draw for all parametrisations
                parametrisation.Factor = parametrisation.Distribution.Draw(rnd);
                rnd.Reset();
            }
        }

        protected abstract T getDistributionFromNominalAndUpper(double factor, double upper);

        protected override CounterFactualValueDistributionModelParametrisation<T> getParametrisation(
            double factor,
            double? upper
        ) {
            if (!upper.HasValue) {
                var msg = $"Missing uncertainty upper value for counter factual value {ExposureResponseFunction.Code}";
                throw new Exception(msg);
            }
            try {
                var distribution = getDistributionFromNominalAndUpper(factor, upper.Value);
                var result = new CounterFactualValueDistributionModelParametrisation<T>() {
                    Distribution = distribution,
                    Factor = factor
                };
                return result;
            } catch (Exception ex) {
                var msg = $"Incorrect specification of counter factual value uncertainty distribution: {ex.Message}";
                throw new Exception(msg);
            }
        }
    }
}
