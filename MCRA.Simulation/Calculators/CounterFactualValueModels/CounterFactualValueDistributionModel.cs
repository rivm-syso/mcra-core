using MCRA.Data.Compiled.Objects;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.CounterFactualValueModels {

    public abstract class CounterFactualValueDistributionModel<T> where T : Distribution {

        public ExposureResponseFunction ExposureResponseFunction { get; protected set; }

        public double Factor { get; protected set; }

        public T Distribution { get; protected set; }

        public CounterFactualValueDistributionModel(ExposureResponseFunction erf) {
            ExposureResponseFunction = erf;
        }

        public void CalculateParameters() {
            Distribution = getDistribution(ExposureResponseFunction);
            Factor = ExposureResponseFunction.CounterFactualValue;
        }

        public void ResampleModelParameters(IRandom random) {
            Factor = Distribution.Draw(random);
        }

        /// <summary>
        /// Returns the currently active (drwan) counterfactual value.
        /// </summary>
        public double GetCounterFactualValue() {
            return Factor;
        }

        protected abstract T getDistribution(ExposureResponseFunction erf);

    }
}
