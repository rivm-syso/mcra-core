using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.BodIndicatorModels {

    public class BodIndicatorConstantModel : IBodIndicatorModel {

        private readonly BurdenOfDisease _bod;

        public Population Population => _bod.Population;

        public Effect Effect => _bod.Effect;

        public BodIndicator BodIndicator => _bod.BodIndicator;

        public BodIndicatorConstantModel(BurdenOfDisease bod) {
            _bod = bod;
        }

        public double GetBodIndicatorValue() {
            return _bod.Value;
        }

        public void ResampleModelParameters(IRandom random) {
            // Do nothing
        }
    }
}
