using MCRA.Data.Compiled.Objects;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.BodIndicatorModels {

    public abstract class BodIndicatorValueDistributionModel<T> where T : Distribution {

        public BurdenOfDisease BurdenOfDisease { get; set; }

        public double Factor { get; protected set; }

        public T Distribution { get; protected set; }

        public BodIndicatorValueDistributionModel(BurdenOfDisease bod) {
            BurdenOfDisease = bod;
        }

        public void CalculateParameters() {
            Distribution = getDistribution(BurdenOfDisease);
            Factor = BurdenOfDisease.Value;
        }

        public void ResampleModelParameters(IRandom random) {
            Factor = Distribution.Draw(random);
        }

        /// <summary>
        /// Returns the currently active (drawn) BodIndicator value.
        /// </summary>
        public double GetBodIndicatorValue() {
            return Factor;
        }

        protected abstract T getDistribution(BurdenOfDisease bod);
    }
}
