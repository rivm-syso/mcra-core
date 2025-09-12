using MCRA.Data.Compiled.Objects;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.CounterFactualValueModels {

    public sealed class CounterFactualValueLogNormalModel(
        ExposureResponseFunction erf
    ) : CounterFactualValueDistributionModel<LogNormalDistribution>(erf) {
        protected override LogNormalDistribution getDistributionFromNominalAndUpper(double factor, double upper) {
            var distribution = LogNormalDistribution.FromMeanAndUpper(factor, upper);
            return distribution;
        }
    }
}
