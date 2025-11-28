using MCRA.Data.Compiled.Objects;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.ExposureResponseFunctionModels.CounterFactualValueModels {

    public sealed class CounterFactualValueUniformModel(
        ExposureResponseFunction erf
    ) : CounterFactualValueDistributionModel<UniformDistribution>(erf), ICounterFactualValueModel {
        protected override UniformDistribution getDistribution(ExposureResponseFunction erf) {
            if (!erf.CFVUncertaintyUpper.HasValue) {
                var msg = $"Missing upper counterfactualvalue uniform uncertainty distribution for ERF {erf.Code}";
                throw new Exception(msg);
            }
            var distribution = UniformDistribution.FromMeanAndUpper(erf.CounterFactualValue, erf.CFVUncertaintyUpper.Value);
            return distribution;
        }
    }
}
