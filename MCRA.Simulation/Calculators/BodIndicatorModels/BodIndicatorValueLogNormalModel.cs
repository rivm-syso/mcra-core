using MCRA.Data.Compiled.Objects;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.BodIndicatorModels {

    public sealed class BodIndicatorValueLogNormalModel(
        BurdenOfDisease bod
    ) : BodIndicatorValueDistributionModel<LogNormalDistribution>(bod), IBodIndicatorValueModel {
        protected override LogNormalDistribution getDistribution(BurdenOfDisease bod) {
            if (!bod.BodUncertaintyUpper.HasValue) {
                var msg = $"Missing upper burden of disease indicator lognormal uncertainty distribution for Bod {bod.BodIndicator.GetDisplayName()}, {bod.Population.Name}.";
                throw new Exception(msg);
            }
            var distribution = LogNormalDistribution.FromMeanAndUpper(
                bod.Value,
                bod.BodUncertaintyUpper.Value
            );
            return distribution;
        }
    }
}
