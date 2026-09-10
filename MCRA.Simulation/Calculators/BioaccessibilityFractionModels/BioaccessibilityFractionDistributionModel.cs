using MCRA.Data.Compiled.Objects;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.BioaccessibilityFractionModels {
    public class BioaccessibilityFractionDistributionModel : IBioaccessibilityFractionModel {

        public Compound Substance { get; private set; }
        public Distribution Distribution { get; private set; }

        public BioaccessibilityFractionDistributionModel(
            Compound substance,
            Distribution distribution
        ) {
            Substance = substance;
            Distribution = distribution;
        }

        public double Draw(IRandom random) {
            return Distribution.Draw(random);
        }
    }
}
