using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.BodIndicatorModels {

    public class BodIndicatorDistributionModel<T> : IBodIndicatorModel where T : Distribution {

        private double _draw;

        private readonly BurdenOfDisease _bod;
        private readonly Population _population;

        public T Distribution { get; protected set; }

        public Population Population => _population;

        public Effect Effect => _bod.Effect;

        public BodIndicator BodIndicator => _bod.BodIndicator;

        public BodIndicatorDistributionModel(T distribution, BurdenOfDisease bod, Population population) {
            _bod = bod;
            _population = population;
            _draw = bod.Value; // Initialise with nominal value
            Distribution = distribution;
        }

        public double GetBodIndicatorValue() {
            return _draw;
        }

        public void ResampleModelParameters(IRandom random) {
            _draw = Distribution.Draw(random);
        }
    }
}
