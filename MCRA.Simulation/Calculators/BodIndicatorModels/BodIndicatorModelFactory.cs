using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.BodIndicatorModels {
    public class BodIndicatorModelFactory {

        /// <summary>
        /// Creates a BoD indicator model for the specified data record.
        /// </summary>
        public static IBodIndicatorModel Create(BurdenOfDisease bod) {
            switch (bod.BodUncertaintyDistribution) {
                case BodIndicatorDistributionType.Constant: {
                        return new BodIndicatorConstantModel(bod);
                    }
                case BodIndicatorDistributionType.Normal: {
                        if (!bod.BodUncertaintyUpper.HasValue) {
                            var msg = $"Missing upper bound burden of disease indicator normal uncertainty distribution for Bod {bod.BodIndicator.GetDisplayName()}, {bod.Population.Name}.";
                            throw new Exception(msg);
                        }
                        var distribution = NormalDistribution.FromMeanAndUpper(bod.Value, bod.BodUncertaintyUpper.Value);
                        return new BodIndicatorDistributionModel<NormalDistribution>(distribution, bod);
                    }
                case BodIndicatorDistributionType.LogNormal: {
                        if (!bod.BodUncertaintyUpper.HasValue) {
                            var msg = $"Missing upper bound burden of disease indicator lognormal uncertainty distribution for Bod {bod.BodIndicator.GetDisplayName()}, {bod.Population.Name}.";
                            throw new Exception(msg);
                        }
                        var distribution = LogNormalDistribution.FromMeanAndUpper(bod.Value, bod.BodUncertaintyUpper.Value);
                        return new BodIndicatorDistributionModel<LogNormalDistribution>(distribution, bod);
                    }
                case BodIndicatorDistributionType.Triangular: {
                        if (!bod.BodUncertaintyLower.HasValue) {
                            var msg = $"Missing lower bound burden of disease indicator triangular uncertainty distribution for Bod {bod.BodIndicator.GetDisplayName()}, {bod.Population.Name}.";
                            throw new Exception(msg);
                        }
                        if (!bod.BodUncertaintyUpper.HasValue) {
                            var msg = $"Missing upper bound burden of disease indicator triangular uncertainty distribution for Bod {bod.BodIndicator.GetDisplayName()}, {bod.Population.Name}.";
                            throw new Exception(msg);
                        }
                        var distribution = TriangularDistribution.FromModeLowerandUpper(bod.Value, bod.BodUncertaintyLower.Value, bod.BodUncertaintyUpper.Value);
                        return new BodIndicatorDistributionModel<TriangularDistribution>(distribution, bod);
                    }
                default: {
                        var msg = $"No burden of disease indicator for distribution type ${bod.BodUncertaintyDistribution}.";
                        throw new NotImplementedException(msg);
                    }
            }
        }
    }
}
