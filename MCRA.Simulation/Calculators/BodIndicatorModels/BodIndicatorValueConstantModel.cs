using MCRA.Data.Compiled.Objects;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.BodIndicatorModels {

    public sealed class BodIndicatorValueConstantModel : IBodIndicatorValueModel {

        public BurdenOfDisease BurdenOfDisease { get; set; }

        public BodIndicatorValueConstantModel(BurdenOfDisease bod) {
            BurdenOfDisease = bod;
        }

        public void CalculateParameters() {
            // Nothing to do here
        }

        public void ResampleModelParameters(IRandom random) {
            // Nothing to do here
        }

        public double GetBodIndicatorValue() {
            return BurdenOfDisease.Value;
        }
    }
}
