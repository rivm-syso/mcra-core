using MCRA.Data.Compiled.Objects;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.BodIndicatorModels {

    public sealed class BodIndicatorValueTriangularModel(
        BurdenOfDisease bod
    ) : BodIndicatorValueDistributionModel<TriangularDistribution>(bod), IBodIndicatorValueModel {
        protected override TriangularDistribution getDistribution(BurdenOfDisease bod) {
            if (!bod.BodUncertaintyLower.HasValue) {
                var msg = $"Missing lower bound for burden of disease indicator triangular uncertainty distribution for Bod {bod.BodIndicator.GetDisplayName()}, {bod.Population.Name}.";
                throw new Exception(msg);
            }
            if (!bod.BodUncertaintyUpper.HasValue) {
                var msg = $"Missing upper bounf for burden of disease indicator triangular uncertainty distribution for Bod {bod.BodIndicator.GetDisplayName()}, {bod.Population.Name}.";
                throw new Exception(msg);
            }
            var distribution = TriangularDistribution.FromModeLowerandUpper(bod.Value, bod.BodUncertaintyLower.Value, bod.BodUncertaintyUpper.Value);
            return distribution;
        }
    }
}
