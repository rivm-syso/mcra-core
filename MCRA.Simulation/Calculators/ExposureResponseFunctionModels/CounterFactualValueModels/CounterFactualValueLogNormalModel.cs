using MCRA.Data.Compiled.Objects;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.ExposureResponseFunctionModels.CounterFactualValueModels {

    public sealed class CounterFactualValueLogNormalModel(
        ExposureResponseFunction erf
    ) : CounterFactualValueDistributionModel<LogNormalDistribution>(erf), ICounterFactualValueModel {
        protected override LogNormalDistribution getDistribution(ExposureResponseFunction erf) {
            if (!erf.CFVUncertaintyUpper.HasValue) {
                var msg = $"Missing upper counterfactualvalue lognormal uncertainty distribution for ERF {erf.Code}.";
                throw new Exception(msg);
            }
            var distribution = LogNormalDistribution.FromMeanAndUpper(erf.CounterFactualValue, erf.CFVUncertaintyUpper.Value);
            return distribution;
        }
    }
}
