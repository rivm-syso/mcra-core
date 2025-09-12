using MCRA.Data.Compiled.Objects;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.CounterFactualValueModels {

    public sealed class CounterFactualValueUniformModel(
        ExposureResponseFunction erf
    ) : CounterFactualValueDistributionModel<UniformDistribution>(erf) {
        protected override UniformDistribution getDistributionFromNominalAndUpper(double factor, double upper) {
            var distribution = UniformDistribution.FromMeanAndUpper(factor, upper);
            return distribution;
        }
    }
}
