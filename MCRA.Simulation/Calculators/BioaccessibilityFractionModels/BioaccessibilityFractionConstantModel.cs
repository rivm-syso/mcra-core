using MCRA.Data.Compiled.Objects;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.BioaccessibilityFractionModels {
    public class BioaccessibilityFractionConstantModel : IBioaccessibilityFractionModel {

        public Compound Substance { get; private set; }
        public double Value { get; private set; }

        public BioaccessibilityFractionConstantModel(
            Compound substance,
            double value
        ) {
            Substance = substance;
            Value = value;
        }

        public double Draw(IRandom random) {
            return Value;
        }
    }
}
