using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.BodIndicatorModels {

    public class BodIndicatorConstantModel : IBodIndicatorModel {

        private readonly BurdenOfDisease _bod;
        private readonly Population _population;

        public Population Population => _population;

        public Effect Effect => _bod.Effect;

        public BodIndicator BodIndicator => _bod.BodIndicator;

        public BodIndicatorConstantModel(BurdenOfDisease bod, Population population) {
            _bod = bod;
            _population = population;
        }

        public double GetBodIndicatorValue() {
            return _bod.Value;
        }

        public void ResampleModelParameters(IRandom random) {
            // Do nothing
        }
    }
}
