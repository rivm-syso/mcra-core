using MCRA.Data.Compiled.Objects;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.ExposureResponseFunctionModels.CounterFactualValueModels {

    public sealed class CounterFactualValueTriangularModel(
        ExposureResponseFunction erf
    ) : CounterFactualValueDistributionModel<TriangularDistribution>(erf), ICounterFactualValueModel {
        protected override TriangularDistribution getDistribution(ExposureResponseFunction erf) {
            if (!erf.CFVUncertaintyLower.HasValue) {
                var msg = $"Missing lower bound counterfactualvalue triangular uncertainty distribution for ERF {erf.Code}.";
                throw new Exception(msg);
            }
            if (!erf.CFVUncertaintyUpper.HasValue) {
                var msg = $"Missing upper bound counterfactualvalue triangular uncertainty distribution for ERF {erf.Code}.";
                throw new Exception(msg);
            }
            var distribution = TriangularDistribution.FromModeLowerandUpper(erf.CounterFactualValue, erf.CFVUncertaintyLower.Value, erf.CFVUncertaintyUpper.Value);
            return distribution;
        }
    }
}
