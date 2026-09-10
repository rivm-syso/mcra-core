using MCRA.Data.Compiled.Objects;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.BioaccessibilityFractionModels {
    public interface IBioaccessibilityFractionModel {
        public Compound Substance { get; }
        double Draw(IRandom random);
    }
}
